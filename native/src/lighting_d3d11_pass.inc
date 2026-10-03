// Lazy, independent shadow resources. All failures keep the original renderer.
bool initialize_lighting_gpu(BaseResources& base,unsigned resolution) {
    auto& g=base.lighting;
    if(g.failed) return false;
    if(!g.vertex_shaders[0]) {
        ComPtr<ID3DBlob> near_blob,far_blob,pixel_blob;
        if(!compile_shader("VSShadowNear","vs_4_0",&near_blob) ||
           !compile_shader("VSShadowFar","vs_4_0",&far_blob) ||
           !compile_shader("PSShadow","ps_4_0",&pixel_blob)) return false;
        if(FAILED(base.device->CreateVertexShader(near_blob->GetBufferPointer(),near_blob->GetBufferSize(),nullptr,g.vertex_shaders[0].GetAddressOf())) ||
           FAILED(base.device->CreateVertexShader(far_blob->GetBufferPointer(),far_blob->GetBufferSize(),nullptr,g.vertex_shaders[1].GetAddressOf())) ||
           FAILED(base.device->CreatePixelShader(pixel_blob->GetBufferPointer(),pixel_blob->GetBufferSize(),nullptr,g.pixel_shader.GetAddressOf()))) return false;
        const D3D11_INPUT_ELEMENT_DESC layout[]={
            {"POSITION",0,DXGI_FORMAT_R32G32B32_FLOAT,0,0,D3D11_INPUT_PER_VERTEX_DATA,0},
            {"TEXCOORD",0,DXGI_FORMAT_R32G32_FLOAT,0,12,D3D11_INPUT_PER_VERTEX_DATA,0},
            {"TEXCOORD",1,DXGI_FORMAT_R32G32B32_UINT,0,20,D3D11_INPUT_PER_VERTEX_DATA,0},
            {"TEXCOORD",2,DXGI_FORMAT_R32G32B32A32_SINT,0,32,D3D11_INPUT_PER_VERTEX_DATA,0},
        };
        if(FAILED(base.device->CreateInputLayout(layout,4,near_blob->GetBufferPointer(),near_blob->GetBufferSize(),g.layout.GetAddressOf()))) return false;
        D3D11_RASTERIZER_DESC raster{};
        raster.FillMode=D3D11_FILL_SOLID;raster.CullMode=D3D11_CULL_NONE;
        raster.DepthClipEnable=TRUE;raster.DepthBias=2;raster.SlopeScaledDepthBias=1.0F;
        if(FAILED(base.device->CreateRasterizerState(&raster,g.rasterizer.GetAddressOf()))) return false;
        D3D11_DEPTH_STENCIL_DESC depth{};
        depth.DepthEnable=TRUE;depth.DepthWriteMask=D3D11_DEPTH_WRITE_MASK_ALL;
        depth.DepthFunc=D3D11_COMPARISON_LESS_EQUAL;
        if(FAILED(base.device->CreateDepthStencilState(&depth,g.depth_state.GetAddressOf()))) return false;
        D3D11_SAMPLER_DESC sampler{};
        sampler.Filter=D3D11_FILTER_COMPARISON_MIN_MAG_LINEAR_MIP_POINT;
        sampler.AddressU=sampler.AddressV=sampler.AddressW=D3D11_TEXTURE_ADDRESS_BORDER;
        sampler.BorderColor[0]=sampler.BorderColor[1]=sampler.BorderColor[2]=sampler.BorderColor[3]=1;
        sampler.ComparisonFunc=D3D11_COMPARISON_LESS_EQUAL;
        sampler.MinLOD=0;sampler.MaxLOD=0;sampler.MaxAnisotropy=1;
        if(FAILED(base.device->CreateSamplerState(&sampler,g.sampler.GetAddressOf()))) return false;
    }
    if(!g.depth||g.resolution!=resolution) {
        // Transactional resize. Old views are not replaced until all new
        // resources have been created successfully.
        ComPtr<ID3D11Texture2D> depth;
        ComPtr<ID3D11ShaderResourceView> view;
        std::array<ComPtr<ID3D11DepthStencilView>,2> slices;
        D3D11_TEXTURE2D_DESC description{};
        description.Width=description.Height=resolution;
        description.MipLevels=1;description.ArraySize=2;
        description.Format=DXGI_FORMAT_R32_TYPELESS;description.SampleDesc.Count=1;
        description.Usage=D3D11_USAGE_DEFAULT;
        description.BindFlags=D3D11_BIND_DEPTH_STENCIL|D3D11_BIND_SHADER_RESOURCE;
        if(FAILED(base.device->CreateTexture2D(&description,nullptr,depth.GetAddressOf()))) return false;
        D3D11_SHADER_RESOURCE_VIEW_DESC srv{};
        srv.Format=DXGI_FORMAT_R32_FLOAT;srv.ViewDimension=D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
        srv.Texture2DArray.MipLevels=1;srv.Texture2DArray.ArraySize=2;
        if(FAILED(base.device->CreateShaderResourceView(depth.Get(),&srv,view.GetAddressOf()))) return false;
        for(UINT i=0;i<2;++i) {
            D3D11_DEPTH_STENCIL_VIEW_DESC dsv{};
            dsv.Format=DXGI_FORMAT_D32_FLOAT;dsv.ViewDimension=D3D11_DSV_DIMENSION_TEXTURE2DARRAY;
            dsv.Texture2DArray.FirstArraySlice=i;dsv.Texture2DArray.ArraySize=1;
            if(FAILED(base.device->CreateDepthStencilView(depth.Get(),&dsv,slices[i].GetAddressOf()))) return false;
        }
        g.depth=std::move(depth);g.view=std::move(view);g.slices=std::move(slices);g.resolution=resolution;
    }
    return true;
}

bool submit_lighting_pass(BaseResources& base,FrameInputResources& frame,
    lighting::Frame& light,unsigned resolution) {
    if(!light.enabled) return true;
    auto& g=base.lighting;auto& input=frame.lighting;auto* context=base.context.Get();
    const auto failure=[&]() {
        if(!g.failed) std::fprintf(stderr,"[Lighting-L01] GPU initialization/upload failed; stock renderer retained (restart to retry)\n");
        g.failed=true;return false;
    };
    if(g.failed) return false;
    if(!input.constants) {
        D3D11_BUFFER_DESC desc{};desc.ByteWidth=sizeof(lighting::Constants);
        desc.Usage=D3D11_USAGE_DYNAMIC;desc.BindFlags=D3D11_BIND_CONSTANT_BUFFER;
        desc.CPUAccessFlags=D3D11_CPU_ACCESS_WRITE;
        if(FAILED(base.device->CreateBuffer(&desc,nullptr,input.constants.GetAddressOf())))return failure();
    }
    if(light.shadows&&!initialize_lighting_gpu(base,resolution))return failure();
    D3D11_MAPPED_SUBRESOURCE mapped{};
    if(FAILED(context->Map(input.constants.Get(),0,D3D11_MAP_WRITE_DISCARD,0,&mapped)))return failure();
    std::memcpy(mapped.pData,&light.constants,sizeof(light.constants));context->Unmap(input.constants.Get(),0);
    if(!light.shadows)return true;
    if(light.casters.size()>(std::numeric_limits<UINT>::max)()/sizeof(lighting::ShadowVertex))return failure();
    const UINT bytes=static_cast<UINT>(light.casters.size()*sizeof(lighting::ShadowVertex));
    if(!input.vertices||input.vertex_bytes<bytes){
        D3D11_BUFFER_DESC desc{};desc.ByteWidth=bytes;desc.Usage=D3D11_USAGE_DYNAMIC;
        desc.BindFlags=D3D11_BIND_VERTEX_BUFFER;desc.CPUAccessFlags=D3D11_CPU_ACCESS_WRITE;
        ComPtr<ID3D11Buffer> buffer;
        if(FAILED(base.device->CreateBuffer(&desc,nullptr,buffer.GetAddressOf())))return failure();
        input.vertices=std::move(buffer);input.vertex_bytes=bytes;
    }
    if(FAILED(context->Map(input.vertices.Get(),0,D3D11_MAP_WRITE_DISCARD,0,&mapped)))return failure();
    std::memcpy(mapped.pData,light.casters.data(),bytes);context->Unmap(input.vertices.Get(),0);
    // Explicit unbind prevents an SRV/DSV hazard on consecutive frames.
    ID3D11ShaderResourceView* null_view=nullptr;context->PSSetShaderResources(4,1,&null_view);
    ID3D11ShaderResourceView* vram=frame.vram_view.Get();context->PSSetShaderResources(0,1,&vram);
    ID3D11Buffer* constants=input.constants.Get();context->VSSetConstantBuffers(2,1,&constants);
    context->PSSetConstantBuffers(2,1,&constants);
    ID3D11Buffer* vertices=input.vertices.Get();UINT stride=sizeof(lighting::ShadowVertex),offset=0;
    context->IASetVertexBuffers(0,1,&vertices,&stride,&offset);
    context->IASetInputLayout(g.layout.Get());
    context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    context->RSSetState(g.rasterizer.Get());
    D3D11_VIEWPORT viewport{0,0,float(resolution),float(resolution),0,1};context->RSSetViewports(1,&viewport);
    context->OMSetDepthStencilState(g.depth_state.Get(),0);
    context->OMSetBlendState(nullptr,nullptr,0xFFFFFFFFU);
    context->PSSetShader(g.pixel_shader.Get(),nullptr,0);
    for(unsigned i=0;i<2;++i){
        context->ClearDepthStencilView(g.slices[i].Get(),D3D11_CLEAR_DEPTH,1.0F,0);
        context->OMSetRenderTargets(0,nullptr,g.slices[i].Get());
        context->VSSetShader(g.vertex_shaders[i].Get(),nullptr,0);
        context->Draw(static_cast<UINT>(light.casters.size()),0);
    }
    context->OMSetRenderTargets(0,nullptr,nullptr);
    return true;
}
