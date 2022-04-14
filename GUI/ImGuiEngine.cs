﻿using DirectDimensional.Core;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings.DXGI;
using System.Runtime.InteropServices;
using System.Numerics;

namespace DirectDimensional.Editor.GUI {
    internal static unsafe class ImGuiEngine {
        public static readonly uint[] strides = new uint[1] { (uint)sizeof(Vertex) };
        public static readonly uint[] offsets = new uint[1] { 0 };

        private static IntPtr pTexturePtr, pSamplerPtr;

        public static void Initialize() {
            ImGuiContext.Initialize();

            pTexturePtr = Marshal.AllocHGlobal(IntPtr.Size);
            pSamplerPtr = Marshal.AllocHGlobal(IntPtr.Size);
        }

        public static void Shutdown() {
            ImGuiContext.Shutdown();

            Marshal.FreeHGlobal(pTexturePtr);
            Marshal.FreeHGlobal(pSamplerPtr);
        }

        public static void NewFrame() {
            ImGuiContext.Vertices.Clear();
            ImGuiContext.Indices.Clear();
            ImGuiContext.DrawCalls.Clear();
        }

        public static void Render() {
            ImGuiContext.WriteMeshDataToGPU();

            DXStateBackup.Backup();

            var ctx = Direct3DContext.DevCtx;

            ctx.IASetVertexBuffers(0u, ImGuiContext.VertexBuffers, strides, offsets);
            ctx.IASetIndexBuffer(ImGuiContext.IndexBuffer, DXGI_FORMAT.R16_UINT, 0);
            ctx.IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY.TriangleList);
            ctx.IASetInputLayout(ImGuiContext.InputLayout);
            ctx.VSSetShader(ImGuiContext.Material.VertexShader!.Shader);
            ctx.PSSetShader(ImGuiContext.Material.PixelShader!.Shader);

            ctx.RSSetState(ImGuiContext.RasterizerState);

            Vector4 v4 = Vector4.Zero;
            ctx.OMSetBlendState(ImGuiContext.BlendState, &v4.X, 0xFFFFFFFF);

            ctx.VSSetConstantBuffers(0, ImGuiContext.ProjectionBuffer);

            var drawList = ImGuiContext.DrawCalls;
            for (int i = 0; i < drawList.Count; i++) {
                var call = drawList[i];

                Marshal.WriteIntPtr(pTexturePtr, call.TexturePointer == IntPtr.Zero ? ImGuiContext.WhiteTexture.DXSRV!._nativePointer : call.TexturePointer);
                Marshal.WriteIntPtr(pSamplerPtr, call.SamplerPointer == IntPtr.Zero ? ImGuiContext.WhiteTexture.DXSampler!._nativePointer : call.SamplerPointer);

                ctx.PSSetShaderResources(0u, 1, pTexturePtr);
                ctx.PSSetSamplers(0u, 1, pSamplerPtr);
                ctx.RSSetScissorRects(call.ScissorsRect);

                ctx.DrawIndexed(call.IndexCount, call.IndexLocation, 0);
            }

            DXStateBackup.Restore();
        }
    }
}