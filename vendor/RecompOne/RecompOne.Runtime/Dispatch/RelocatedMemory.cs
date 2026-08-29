using RecompOne.Runtime.Cdrom;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime.Dispatch;

internal sealed class RelocatedMemory : IMemory
{
    internal IMemory Inner { get; }
    readonly PSMemory? _playStationMemory;
    internal uint LinkedBase { get; }
    internal uint Size { get; }
    internal uint Delta { get; }

    internal RelocatedMemory(IMemory inner, uint linkedBase, uint size, uint delta)
    {
        Inner = inner is RelocatedMemory relocated ? relocated.Inner : inner;
        _playStationMemory = Inner as PSMemory;
        LinkedBase = linkedBase;
        Size = size;
        Delta = delta;
    }

    internal bool Matches(uint linkedBase, uint size, uint delta) =>
        LinkedBase == linkedBase && Size == size && Delta == delta;

    uint Address(uint address) =>
        address >= LinkedBase && address - LinkedBase < Size ? address + Delta : address;

    public byte ReadU8(uint address)
    {
        uint translated = Address(address);
        return _playStationMemory is not null
            ? _playStationMemory.ReadU8(translated)
            : Inner.ReadU8(translated);
    }

    public ushort ReadU16(uint address)
    {
        uint translated = Address(address);
        return _playStationMemory is not null
            ? _playStationMemory.ReadU16(translated)
            : Inner.ReadU16(translated);
    }

    public uint ReadU32(uint address)
    {
        uint translated = Address(address);
        return _playStationMemory is not null
            ? _playStationMemory.ReadU32(translated)
            : Inner.ReadU32(translated);
    }

    public void WriteU8(uint address, byte value)
    {
        uint translated = Address(address);
        if (_playStationMemory is not null)
            _playStationMemory.WriteU8(translated, value);
        else
            Inner.WriteU8(translated, value);
    }

    public void WriteU16(uint address, ushort value)
    {
        uint translated = Address(address);
        if (_playStationMemory is not null)
            _playStationMemory.WriteU16(translated, value);
        else
            Inner.WriteU16(translated, value);
    }

    public void WriteU32(uint address, uint value)
    {
        uint translated = Address(address);
        if (_playStationMemory is not null)
            _playStationMemory.WriteU32(translated, value);
        else
            Inner.WriteU32(translated, value);
    }

    public uint ReadWordLeft(uint current, uint address)
    {
        uint translated = Address(address);
        return _playStationMemory is not null
            ? _playStationMemory.ReadWordLeft(current, translated)
            : Inner.ReadWordLeft(current, translated);
    }

    public uint ReadWordRight(uint current, uint address)
    {
        uint translated = Address(address);
        return _playStationMemory is not null
            ? _playStationMemory.ReadWordRight(current, translated)
            : Inner.ReadWordRight(current, translated);
    }

    public void WriteWordLeft(uint address, uint value)
    {
        uint translated = Address(address);
        if (_playStationMemory is not null)
            _playStationMemory.WriteWordLeft(translated, value);
        else
            Inner.WriteWordLeft(translated, value);
    }

    public void WriteWordRight(uint address, uint value)
    {
        uint translated = Address(address);
        if (_playStationMemory is not null)
            _playStationMemory.WriteWordRight(translated, value);
        else
            Inner.WriteWordRight(translated, value);
    }
    public void LoadBytes(uint address, byte[] data) => Inner.LoadBytes(Address(address), data);
    public void ZeroRange(uint address, uint length) => Inner.ZeroRange(Address(address), length);
    public void SetCd(CdController cd) => Inner.SetCd(cd);
}
