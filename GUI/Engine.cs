using DirectDimensional.Core;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings.WinAPI;
using DirectDimensional.Bindings.DXGI;
using System.Runtime.InteropServices;
using System.Numerics;

namespace DirectDimensional.Editor.GUI {
    internal static unsafe class Engine {
        public static readonly uint[] strides = new uint[1] { (uint)sizeof(Vertex) };
        public static readonly uint[] offsets = new uint[1] { 0 };

        private static IntPtr pTexturePtr, pSamplerPtr;

        internal static List<List<DrawCall>> GlobalDrawCalls { get; private set; }

        public static event Action? EndFrameCallback;

        static Engine() {
            GlobalDrawCalls = new(8);
        }

        public static void Initialize() {
            Context.Initialize();

            pTexturePtr = Marshal.AllocHGlobal(IntPtr.Size);
            pSamplerPtr = Marshal.AllocHGlobal(IntPtr.Size);
        }

        public static void Shutdown() {
            Context.Shutdown();

            Marshal.FreeHGlobal(pTexturePtr);
            Marshal.FreeHGlobal(pSamplerPtr);
        }

        public static void NewFrame() {
            GlobalDrawCalls.Clear();

            ImGui.NewFrame();
            Context.Vertices.Clear();
            Context.Indices.Clear();
            
            for (int i = 0; i < ImGui.Windows.Count; i++) {
                ImGui.Windows[i].ResetForNewFrame();
            }

            LowLevel.NewFrameReset();

            Identifier.ResetForNewFrame();
            Styling.ClearAll();
            Coloring.ClearAll();
            FontStack.ClearStack();
        }

        public static void Update() {
            ImGui.UpdateHoveringWindow();
            Input.Update();

            if (Behaviours.WrapCursorInWnd) {
                if (WinAPI.GetCursorPos(out POINT point) && WinAPI.GetClientRect(EditorWindow.WindowHandle, out RECT rect)) {
                    if (WinAPI.MapWindowPoints(EditorWindow.WindowHandle, IntPtr.Zero, (POINT*)&rect, 2) != 0) {
                        if (point.X <= rect.Left) {
                            WinAPI.SetCursorPos(rect.Right - 2, point.Y);
                        } else if (point.X >= rect.Right - 1) {
                            WinAPI.SetCursorPos(rect.Left + 1, point.Y);
                        }

                        if (point.Y >= rect.Bottom) {
                            WinAPI.SetCursorPos(point.X, rect.Top + 1);
                        } else if (point.Y <= rect.Top) {
                            WinAPI.SetCursorPos(point.X, rect.Bottom - 1);
                        }

                        Behaviours.DeactivateWrapCursorInWindow();
                    }
                }
            }
        }

        private static int SortWindowType(GuiWindow a, GuiWindow b) {
            bool c1 = a.Priority < b.Priority;
            bool c2 = b.Priority < a.Priority;

            return *(int*)&c2 - *(int*)&c1;
        }

        public static void Render() {
            ImGui.Windows.Sort(SortWindowType);

            for (int i = 0; i < ImGui.Windows.Count; i++) {
                GlobalDrawCalls.Add(ImGui.Windows[i].DrawCalls);
            }

            Context.WriteMeshDataToGPU();

            DXStateBackup.Backup();

            var ctx = Direct3DContext.DevCtx;

            ctx.IASetVertexBuffers(0u, Context.VertexBuffers, strides, offsets);
            ctx.IASetIndexBuffer(Context.IndexBuffer, DXGI_FORMAT.R16_UINT, 0);
            ctx.IASetInputLayout(Context.InputLayout);
            ctx.VSSetShader(Context.Material.VertexShader!.Shader);
            ctx.PSSetShader(Context.Material.PixelShader!.Shader);

            ctx.RSSetState(Context.RasterizerState);

            ctx.OMSetBlendState(Context.BlendState, null, 0xFFFFFFFF);
            ctx.VSSetConstantBuffers(0, Context.ProjectionBuffer);

            for (int i = 0; i < GlobalDrawCalls.Count; i++) {
                var drawList = GlobalDrawCalls[i];

                for (int d = 0; d < drawList.Count; d++) {
                    var call = drawList[d];

                    Marshal.WriteIntPtr(pTexturePtr, call.TexturePointer == IntPtr.Zero ? Context.WhiteTexture.DXSRV!._nativePointer : call.TexturePointer);
                    Marshal.WriteIntPtr(pSamplerPtr, call.SamplerPointer == IntPtr.Zero ? Context.WhiteTexture.DXSampler!._nativePointer : call.SamplerPointer);

                    ctx.PSSetShaderResources(0u, 1, pTexturePtr);
                    ctx.PSSetSamplers(0u, 1, pSamplerPtr);
                    ctx.RSSetScissorRects(call.ScissorsRect);
                    ctx.IASetPrimitiveTopology(call.Topology);

                    ctx.DrawIndexed(call.IndexCount, call.IndexLocation, 0);
                }
            }

            DXStateBackup.Restore();
        }

        /// <summary>
        /// Call after <seealso cref="Render"/>
        /// </summary>
        public static void EndFrame() {
            if (Mouse.LeftPressed) {
                ImGui.FocusWindow(ImGui.HoveringWindow);
            }

            EndFrameCallback?.Invoke();
            EndFrameCallback = null;
        }
    }
}
