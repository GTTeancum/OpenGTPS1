using RecompOne.Runtime.Cdrom;

namespace RecompOne.Runtime.Memory;

public sealed class Dma
{
    const uint Start = 0x01000000u;
    const int OrderingTableHistoryLength = 8;

    readonly IMemory _mem;
    readonly PSMemory? _psMemory;
    readonly Gpu _gpu;
    readonly Spu _spu;
    readonly Mdec _mdec;
    readonly Action _raiseIrq;
    CdController? _cd;
    readonly uint[] _orderingTableHeads = new uint[OrderingTableHistoryLength];
    readonly uint[] _orderingTableEntryCounts = new uint[OrderingTableHistoryLength];
    int _orderingTableHistoryCursor;

    uint _dicr;

    public Dma(IMemory mem, Gpu gpu, Spu spu, Mdec mdec, Action raiseIrq)
    {
        _mem = mem;
        _psMemory = mem as PSMemory;
        _gpu = gpu;
        _spu = spu;
        _mdec = mdec;
        _raiseIrq = raiseIrq;
    }

    public void SetCd(CdController cd) => _cd = cd;

    public uint ReadDicr() => _dicr;

    public void WriteDicr(uint val)
    {
        uint flags = (_dicr >> 24) & 0x7Fu;
        flags &= ~((val >> 24) & 0x7Fu);
        _dicr = (val & 0x00FFFFFFu) | (flags << 24);
        if ((_dicr & 0x8000u) != 0 || (((_dicr >> 23) & 1u) != 0 && flags != 0))
            _dicr |= 0x80000000u;
    }

    public void Run(int channel, uint madr, uint bcr, uint chcr)
    {
        Log.Dma($"ch{channel} madr=0x{madr:X8} bcr=0x{bcr:X8} chcr=0x{chcr:X8}");
        switch (channel)
        {
            case 0: TransferMdecIn(madr, bcr); break;
            case 1: TransferMdecOut(madr, bcr); break;
            case 2: TransferGpu(madr, bcr, chcr); break;
            case 3: TransferCd(madr, bcr); break;
            case 4: TransferSpu(madr, bcr, chcr); break;
            case 6: ClearOrderingTable(madr, bcr); break;
            default: return;
        }
        // Completion is asynchronous on the PS1. Publishing DICR here makes
        // a DMA started by a completion callback visible to the same guest
        // IRQ handler before that callback can update its chunk state.
        Runtime.DeferHardwareAction(() => Complete(channel));
    }

    void TransferMdecIn(uint madr, uint bcr)
    {
        uint words = WordCount(bcr);
        for (uint i = 0; i < words; i++)
            _mdec.Write0(_mem.ReadU32(madr + i * 4u));
    }

    void TransferMdecOut(uint madr, uint bcr)
    {
        uint words = WordCount(bcr);
        uint wordOr = 0;
        bool trace = Log.DmaOn;
        for (uint i = 0; i < words; i++)
        {
            uint word = _mdec.ReadData();
            if (trace) wordOr |= word;
            _mem.WriteU32(madr + i * 4u, word);
        }
        if (trace) Log.Dma($"MDEC out words={words} or=0x{wordOr:X8}");
    }

    void TransferGpu(uint madr, uint bcr, uint chcr)
    {
        uint sync = (chcr >> 9) & 3u;
        if (sync == 2)
        {
            uint ramAddressMask =
                (Runtime.Mode == RunMode.Devkit
                    ? MemoryMap.DevkitRamSize
                    : MemoryMap.RetailRamSize) - 4u;
            uint addr = madr & ramAddressMask;
            bool traceOrderingTable =
                Host.InputManager.CurrentPoll == TraceOrderingTablePoll;
            int visitedTags = 0;
            int positiveLengthTags = 0;
            int orderingTableEntryTags = 0;
            int maximumOrderingTableIndex = 0;
            int zeroLengthTags = 0;
            uint minimumZeroLengthAddress = uint.MaxValue;
            uint maximumZeroLengthAddress = 0u;
            try
            {
                for (int guard = 0; guard < 0x100000; guard++)
                {
                    visitedTags++;
                    uint header = ReadGpuRamWord(addr);
                    uint count = header >> 24;
                    if (count == 0u)
                    {
                        zeroLengthTags++;
                        minimumZeroLengthAddress = Math.Min(
                            minimumZeroLengthAddress,
                            addr);
                        maximumZeroLengthAddress = Math.Max(
                            maximumZeroLengthAddress,
                            addr);
                    }
                    if (TryGetOrderingTableIndex(addr, out int orderingTableIndex))
                    {
                        _gpu.SetOrderingTableIndex(orderingTableIndex);
                        orderingTableEntryTags++;
                        maximumOrderingTableIndex = Math.Max(
                            maximumOrderingTableIndex,
                            orderingTableIndex);
                    }
                    if (count > 0u)
                        positiveLengthTags++;
                    for (uint i = 0; i < count; i++)
                    {
                        uint sourceAddress = addr + 4u + i * 4u;
                        _gpu.WriteGp0(
                            ReadGpuRamWord(sourceAddress),
                            sourceAddress);
                    }
                    uint next = header & 0xFFFFFFu;
                    if (next == 0xFFFFFFu || (next & 0x800000u) != 0)
                        break;
                    addr = next & ramAddressMask;
                }
            }
            finally
            {
                if (zeroLengthTags >= 16 &&
                    minimumZeroLengthAddress != uint.MaxValue &&
                    maximumZeroLengthAddress >= minimumZeroLengthAddress)
                {
                    RememberOrderingTable(
                        maximumZeroLengthAddress,
                        (maximumZeroLengthAddress -
                            minimumZeroLengthAddress) / 4u + 1u);
                }
                if (traceOrderingTable)
                {
                    Console.Error.WriteLine(
                        $"[GT2-OT-DMA] poll={Host.InputManager.CurrentPoll} " +
                        $"start=0x{(madr & ramAddressMask):X8} " +
                        $"visited={visitedTags} positive={positiveLengthTags} " +
                        $"zero={zeroLengthTags} " +
                        $"zeroRange=0x{minimumZeroLengthAddress:X8}.." +
                        $"0x{maximumZeroLengthAddress:X8} " +
                        $"entries={orderingTableEntryTags} " +
                        $"maxIndex={maximumOrderingTableIndex}");
                }
                _gpu.SetOrderingTableIndex(0);
            }
        }
        else if ((chcr & 1u) != 0)
        {
            uint words = WordCount(bcr);
            uint wordOr = 0;
            bool trace = Log.DmaOn;
            for (uint i = 0; i < words; i++)
            {
                uint word = ReadGpuRamWord(madr + i * 4u);
                if (trace) wordOr |= word;
                _gpu.WriteGp0(word);
            }
            if (trace) Log.Dma($"GPU in words={words} or=0x{wordOr:X8}");
        }
        else
        {
            uint words = WordCount(bcr);
            for (uint i = 0; i < words; i++)
                _mem.WriteU32(madr + i * 4u, _gpu.ReadData());
        }
    }

    uint ReadGpuRamWord(uint address) =>
        _psMemory != null
            ? _psMemory.ReadDmaRamU32(address)
            : _mem.ReadU32(address);

    void TransferSpu(uint madr, uint bcr, uint chcr)
    {
        if ((chcr & 1u) == 0) return;
        uint bytes = WordCount(bcr) * 4u;
        var buf = new byte[bytes];
        for (uint i = 0; i < bytes; i++)
            buf[i] = _mem.ReadU8(madr + i);
        _spu.DmaWrite(_spu.TransferAddrBytes(), buf);
    }

    void TransferCd(uint madr, uint bcr)
    {
        if (_cd == null) return;
        _cd.DmaReadData(_mem, madr, WordCount(bcr) * 4u);
    }

    void ClearOrderingTable(uint madr, uint bcr)
    {
        uint count = bcr & 0xFFFFu;
        if (count == 0) return;
        RememberOrderingTable(madr, count);
        uint addr = madr;
        for (uint i = 0; i < count - 1; i++)
        {
            _mem.WriteU32(addr, (addr - 4u) & 0x00FFFFFFu);
            addr -= 4u;
        }
        _mem.WriteU32(addr, 0x00FFFFFFu);
    }

    void RememberOrderingTable(uint head, uint count)
    {
        for (int i = 0; i < OrderingTableHistoryLength; i++)
        {
            if (_orderingTableHeads[i] != head)
                continue;
            _orderingTableEntryCounts[i] = count;
            return;
        }
        _orderingTableHeads[_orderingTableHistoryCursor] = head;
        _orderingTableEntryCounts[_orderingTableHistoryCursor] = count;
        _orderingTableHistoryCursor =
            (_orderingTableHistoryCursor + 1) % OrderingTableHistoryLength;
    }

    internal bool TryGetOrderingTableIndex(uint address, out int index)
    {
        for (int i = 0; i < OrderingTableHistoryLength; i++)
        {
            uint head = _orderingTableHeads[i];
            uint count = _orderingTableEntryCounts[i];
            if (count == 0u || address > head)
                continue;
            uint distance = head - address;
            if ((distance & 3u) != 0u || distance / 4u >= count)
                continue;
            index = checked((int)(distance / 4u));
            return true;
        }
        index = 0;
        return false;
    }

    static readonly int TraceOrderingTablePoll =
        int.TryParse(
            Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_OT_WALK_POLL"),
            out int traceOrderingTablePoll)
            ? traceOrderingTablePoll
            : -1;

    void Complete(int channel)
    {
        bool master = (_dicr & (1u << 23)) != 0;
        bool enabled = (_dicr & (1u << (16 + channel))) != 0;
        if (!master || !enabled) return;
        _dicr |= 1u << (24 + channel);
        _raiseIrq();
    }

    static uint WordCount(uint bcr)
    {
        uint size = bcr & 0xFFFFu;
        uint blocks = (bcr >> 16) & 0xFFFFu;
        uint total = blocks == 0 ? size : size * blocks;
        return total == 0 ? 0x10000u : total;
    }
}
