using DirectDimensional.Bindings;
using DirectDimensional.Core;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings.D3DCompiler;
using DirectDimensional.Bindings.DXGI;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using D3D11Buffer = DirectDimensional.Bindings.Direct3D11.Buffer;

using DDTexture2D = DirectDimensional.Core.Texture2D;

namespace DirectDimensional.Editor.GUI {
    internal static unsafe class ImGuiContext {
        public static ComArray<D3D11Buffer> ProjectionBuffer = null!;

        public static ComArray<D3D11Buffer> VertexBuffers { get; private set; }
        public static D3D11Buffer IndexBuffer { get; private set; } = null!;

        public static Material Material { get; private set; } = null!;
        public static BlendState BlendState { get; private set; } = null!;

        public static InputLayout InputLayout { get; private set; } = null!;

        public static DDTexture2D WhiteTexture { get; private set; } = null!;

        public static List<DrawCall> DrawCalls { get; private set; }
        public static List<Vertex> Vertices { get; private set; }
        public static List<ushort> Indices { get; private set; }

        public static RasterizerState RasterizerState { get; private set; } = null!;

        public static DepthStencilState DepthState { get; private set; } = null!;

        static ImGuiContext() {
            DrawCalls = new(64);
            Vertices = new(128);
            Indices = new(256);

            VertexBuffers = new(1);
        }

        private static Matrix4x4 ProjectionMatrix() {
            Matrix4x4 projection = new();

            var rect = Window.ClientSize;

            projection.M11 = 2f / rect.X;
            projection.M41 = -1;
            projection.M22 = -2f / rect.Y;
            projection.M42 = 1;
            projection.M33 = 0.5f;
            projection.M44 = 1;

            return projection;
        }

        public static void Initialize() {
            EditorApplication.OnResize += Resize;

            {
                ProjectionBuffer = new(1);

                Matrix4x4 projection = ProjectionMatrix();

                Direct3DContext.Device.CreateConstantBuffer(64, &projection, out var buffer).ThrowExceptionIfError();
                ProjectionBuffer[0] = buffer;
            }

            Material = Material.Compile(@"
            struct VSInput {
                float3 position : Position;
                float4 color    : Color;
                float2 uv       : TexCoord;
            };

            struct PSInput {
                float4 position : SV_Position;
                float4 color    : Color;
                float2 uv       : TexCoord;
            };

            cbuffer _Buffer : register(b0) {
                float4x4 _Projection;
            };

            PSInput main(VSInput input) {
                PSInput output;

                output.position = mul(_Projection, float4(input.position.xy, 1, 1));
                output.color = input.color;
                output.uv = input.uv;

                return output;
            }
            ", "ImGUIVS",
            @"
            struct PSInput {
                float4 position : SV_Position;
                float4 color    : Color;
                float2 uv       : TexCoord;
            };

            Texture2D _MainTex : register(t0);
            SamplerState _Sampler : register(s0);

            float4 main(PSInput input) : SV_Target {
                float4 sample = _MainTex.Sample(_Sampler, input.uv);
                return input.color * sample;
            }
            ", "ImGUIPS", out var bytecode, D3DCOMPILE.None)!;

            var elements = new D3D11_INPUT_ELEMENT_DESC[] {
                new D3D11_INPUT_ELEMENT_DESC("Position", 0, DXGI_FORMAT.R32G32B32_FLOAT, 0, 0, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),
                new D3D11_INPUT_ELEMENT_DESC("Color", 0, DXGI_FORMAT.R8G8B8A8_UNORM, 0, 12, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),
                new D3D11_INPUT_ELEMENT_DESC("TexCoord", 0, DXGI_FORMAT.R32G32_FLOAT, 0, 16, D3D11_INPUT_CLASSIFICATION.PerVertexData, 0),
            };

            Direct3DContext.Device.CreateInputLayout(elements, bytecode!, out var _il).ThrowExceptionIfError();
            InputLayout = _il!;

            foreach (var elem in elements) {
                elem.Dispose();
            }

            bytecode.CheckAndRelease();

            WhiteTexture = new(4, 4, TextureFlags.Render);

            Direct3DContext.Device.CreateVertexBuffer<Vertex>(Vertices.Capacity, true, out var _vb).ThrowExceptionIfError();
            VertexBuffers[0] = _vb!;

            Direct3DContext.Device.CreateIndexBuffer<ushort>(Indices.Capacity, true, out var _ib).ThrowExceptionIfError();
            IndexBuffer = _ib!;

            {
                D3D11_RASTERIZER_DESC desc = D3D11_RASTERIZER_DESC.Default;

                desc.FillMode = D3D11_FILL_MODE.Solid;
                desc.CullMode = D3D11_CULL_MODE.None;
                desc.ScissorEnable = 1;
                desc.DepthClipEnable = 1;

                Direct3DContext.Device.CreateRasterizerState(desc, out var _rs).ThrowExceptionIfError();
                RasterizerState = _rs!;
            }

            {
                D3D11_BLEND_DESC desc = default;

                desc.AlphaToCoverageEnable = false;
                desc.RenderTarget0.BlendEnable = true;
                desc.RenderTarget0.SrcBlend = D3D11_BLEND.SrcAlpha;
                desc.RenderTarget0.DestBlend = D3D11_BLEND.InvSrcAlpha;
                desc.RenderTarget0.BlendOp = D3D11_BLEND_OP.Add;
                desc.RenderTarget0.SrcBlendAlpha = D3D11_BLEND.One;
                desc.RenderTarget0.DestBlendAlpha = D3D11_BLEND.InvSrcAlpha;
                desc.RenderTarget0.BlendOpAlpha = D3D11_BLEND_OP.Add;
                desc.RenderTarget0.RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE.All;

                Direct3DContext.Device.CreateBlendState(desc, out var _blendState).ThrowExceptionIfError();
                BlendState = _blendState!;

                D3D11_DEPTH_STENCIL_DESC desc2 = default;
                desc2.DepthEnable = false;
                desc2.DepthWriteMask = D3D11_DEPTH_WRITE_MASK.All;
                desc2.DepthFunc = D3D11_COMPARISON_FUNC.Always;
                desc2.StencilEnable = false;
                desc2.FrontFace.StencilFailOp = desc2.FrontFace.StencilDepthFailOp = desc2.FrontFace.StencilPassOp = D3D11_STENCIL_OP.Keep;
                desc2.FrontFace.StencilFunc = D3D11_COMPARISON_FUNC.Always;
                desc2.BackFace = desc2.FrontFace;

                Direct3DContext.Device.CreateDepthStencilState(desc2, out var _dss).ThrowExceptionIfError();
                DepthState = _dss!;
            }
        }

        private static void Resize() {
            D3D11_MAPPED_SUBRESOURCE msr;
            Direct3DContext.DevCtx.Map(ProjectionBuffer[0]!, 0, D3D11_MAP.WriteDiscard, &msr);

            Matrix4x4 projection = ProjectionMatrix();

            Unsafe.CopyBlock(msr.pData.ToPointer(), &projection, 64);

            Direct3DContext.DevCtx.Unmap(ProjectionBuffer[0]!, 0);
        }

        public static void Shutdown() {
            EditorApplication.OnResize -= Resize;

            WhiteTexture?.Destroy();
            Material?.Destroy();

            InputLayout.CheckAndRelease();
            BlendState.CheckAndRelease();

            DepthState.CheckAndRelease();
            VertexBuffers.TrueDispose();
            IndexBuffer.CheckAndRelease();

            Vertices.Clear();
            Indices.Clear();
            DrawCalls.Clear();

            RasterizerState.CheckAndRelease();

            ProjectionBuffer?.TrueDispose();
        }

        public static void WriteMeshDataToGPU() {
            if ((int)(VertexBuffers[0]!.Description.ByteWidth / sizeof(Vertex)) < Vertices.Count) {
                VertexBuffers[0]!.Release();

                Direct3DContext.Device.CreateVertexBuffer<Vertex>(Vertices.Count, true, out var _vb).ThrowExceptionIfError();
                VertexBuffers[0] = _vb!;
            }

            if ((int)(IndexBuffer.Description.ByteWidth / sizeof(ushort)) < Indices.Count) {
                IndexBuffer.Release();

                Direct3DContext.Device.CreateIndexBuffer<ushort>(Indices.Count, true, out var _ib).ThrowExceptionIfError();
                IndexBuffer = _ib!;
            }

            D3D11_MAPPED_SUBRESOURCE vmsr, imsr;

            Direct3DContext.DevCtx.Map(VertexBuffers[0]!, 0u, D3D11_MAP.WriteDiscard, &vmsr);
            Direct3DContext.DevCtx.Map(IndexBuffer, 0u, D3D11_MAP.WriteDiscard, &imsr);

            fixed (Vertex* pVertex = CollectionsMarshal.AsSpan(Vertices)) {
                fixed (ushort* pIndex = CollectionsMarshal.AsSpan(Indices)) {
                    Unsafe.CopyBlock(vmsr.pData.ToPointer(), pVertex, (uint)(Vertices.Count * sizeof(Vertex)));
                    Unsafe.CopyBlock(imsr.pData.ToPointer(), pIndex, (uint)(Indices.Count * sizeof(ushort)));
                }
            }

            Direct3DContext.DevCtx.Unmap(VertexBuffers[0]!, 0u);
            Direct3DContext.DevCtx.Unmap(IndexBuffer, 0u);
        }
    }
}
