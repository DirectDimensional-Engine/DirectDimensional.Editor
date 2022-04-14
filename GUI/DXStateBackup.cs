using System;
using System.Runtime.InteropServices;
using System.Numerics;
using DirectDimensional.Core;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings.DXGI;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.WinAPI;

using D3DBuffer = DirectDimensional.Bindings.Direct3D11.Buffer;
using D3DVertexShader = DirectDimensional.Bindings.Direct3D11.VertexShader;
using D3DPixelShader = DirectDimensional.Bindings.Direct3D11.PixelShader;

namespace DirectDimensional.Editor.GUI {
    internal static unsafe class DXStateBackup {
        private static ComArray<D3DBuffer> vertexBuffers;
        private static uint[] strides, offsets;

        private static D3DBuffer? indexBuffer;
        private static DXGI_FORMAT indexBufferFormat;
        private static uint indexBufferOffset;

        private static InputLayout? inputLayout;

        private static D3D11_PRIMITIVE_TOPOLOGY topology;

        private static D3DVertexShader? vertexShader;
        private static D3DPixelShader? pixelShader;

        private static BlendState? blendState;
        private static Vector4 blendFactor;
        private static uint sampleMask;

        private static RasterizerState? rasterizerState;

        private static readonly ComArray<RenderTargetView> rtvs;
        private static DepthStencilView? depthStencilView;

        private static readonly ComArray<D3DBuffer> _vsBuffers;

        private static readonly ComArray<ShaderResourceView> _psSRVs;
        private static readonly ComArray<SamplerState> _psSamplers;

        private static readonly D3D11_VIEWPORT[] _viewports;
        private static readonly RECT[] _rects;

        private static DepthStencilState? _dss;
        private static uint _depthStencilRef;

        static DXStateBackup() {
            vertexBuffers = new(1);

            strides = new uint[1];
            offsets = new uint[1];

            rtvs = new(1);
            _vsBuffers = new(1);

            _psSRVs = new(1);
            _psSamplers = new(1);

            _viewports = new D3D11_VIEWPORT[16];
            _rects = new RECT[16];
        }

        public static void Backup() {
            var ctx = Direct3DContext.DevCtx;

            ctx.IAGetVertexBuffers(0u, vertexBuffers, strides, offsets);
            indexBuffer = ctx.IAGetIndexBuffer(out indexBufferFormat, out indexBufferOffset);
            inputLayout = ctx.IAGetInputLayout();
            topology = ctx.IAGetPrimitiveTopology();

            vertexShader = ctx.VSGetShader();
            pixelShader = ctx.PSGetShader();

            blendState = ctx.OMGetBlendState(out blendFactor, out sampleMask);
            rasterizerState = ctx.RSGetState();

            ctx.OMGetRenderTargets(rtvs, out depthStencilView);

            ctx.VSGetConstantBuffers(0, _vsBuffers);

            ctx.PSGetShaderResources(0, _psSRVs);
            ctx.PSGetSamplers(0, _psSamplers);

            ctx.RSGetViewports(_viewports.AsSpan(), out _);
            ctx.RSGetScissorRects(_rects, out _);

            _dss = ctx.OMGetDepthStencilState(out _depthStencilRef);
        }

        public static void Restore() {
            var ctx = Direct3DContext.DevCtx;

            ctx.OMSetDepthStencilState(_dss, _depthStencilRef);

            ctx.IASetVertexBuffers(0u, vertexBuffers, strides, offsets);
            ctx.IASetIndexBuffer(indexBuffer, indexBufferFormat, indexBufferOffset); indexBuffer.CheckAndRelease();
            ctx.IASetInputLayout(inputLayout); inputLayout.CheckAndRelease();
            ctx.IASetPrimitiveTopology(topology);

            ctx.VSSetShader(vertexShader); vertexShader.CheckAndRelease();
            ctx.VSSetConstantBuffers(0u, _vsBuffers); _vsBuffers[0].CheckAndRelease();

            ctx.PSSetShader(pixelShader); pixelShader.CheckAndRelease();
            ctx.PSSetShaderResources(0u, _psSRVs); _psSRVs[0].CheckAndRelease();
            ctx.PSSetSamplers(0u, _psSamplers); _psSamplers[0].CheckAndRelease();

            fixed (float* pBlendFactor = &blendFactor.X) {
                ctx.OMSetBlendState(blendState, pBlendFactor, sampleMask); blendState.CheckAndRelease();
            }
            ctx.OMSetRenderTargets(rtvs, depthStencilView); depthStencilView.CheckAndRelease(); rtvs[0].CheckAndRelease();

            ctx.RSSetState(rasterizerState); rasterizerState.CheckAndRelease();
            ctx.RSSetViewports(_viewports);
            ctx.RSSetScissorRects(_rects);
        }
    }
}
