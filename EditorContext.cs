using System;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Runtime;
using DirectDimensional.Core;

using D3DTexture2D = DirectDimensional.Bindings.Direct3D11.Texture2D;

namespace DirectDimensional.Editor {
    public static unsafe class EditorContext {
        private static ShaderResourceView _gameSRV = null!;
        public static ShaderResourceView GameSceneSRV => _gameSRV;

        public static void Initialize() {
            //RuntimeContext.RenderingOutput = Direct3DContext.BackBuffer;

            var clientSize = Window.Internal_ClientSize;

            D3D11_TEXTURE2D_DESC desc = default;
            desc.Width = (uint)clientSize.Width;
            desc.Height = (uint)clientSize.Height;
            desc.Format = Bindings.DXGI.DXGI_FORMAT.R8G8B8A8_UNORM;
            desc.SampleDesc.Count = 1;
            desc.SampleDesc.Quality = 0;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.Usage = D3D11_USAGE.Default;
            desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG.None;
            desc.BindFlags = D3D11_BIND_FLAG.ShaderResource | D3D11_BIND_FLAG.RenderTarget;
            desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG.None;

            Direct3DContext.Device.CreateTexture2D(desc, null, out var pTexture).ThrowExceptionIfError();

            D3D11_RENDER_TARGET_VIEW_DESC rDesc = default;
            rDesc.ViewDimension = D3D11_RTV_DIMENSION.Texture2D;
            rDesc.Texture2D.MipSlice = 0;
            rDesc.Format = desc.Format;

            try {
                Direct3DContext.Device.CreateRenderTargetView(pTexture!, &rDesc, out var rtv).ThrowExceptionIfError();

                RuntimeContext.RenderingOutput = rtv;

                D3D11_SHADER_RESOURCE_VIEW_DESC sDesc = default;
                sDesc.ViewDimension = D3D11_SRV_DIMENSION.Texture2D;
                sDesc.Format = desc.Format;
                sDesc.Texture2D.MostDetailedMip = 0;
                sDesc.Texture2D.MipLevels = desc.MipLevels;

                Direct3DContext.Device.CreateShaderResourceView(pTexture!, &sDesc, out _gameSRV!).ThrowExceptionIfError();
            } finally {
                pTexture!.Release();
            }
        }
    }
}
