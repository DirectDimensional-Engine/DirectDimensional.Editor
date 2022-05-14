using DirectDimensional.Core;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings.WinAPI;
using DirectDimensional.Bindings.DXGI;
using System.Runtime.InteropServices;
using System.Numerics;

namespace DirectDimensional.Editor.GUI {
    internal static unsafe class ImGuiEngine {
        public static readonly uint[] strides = new uint[1] { (uint)sizeof(Vertex) };
        public static readonly uint[] offsets = new uint[1] { 0 };

        private static IntPtr pTexturePtr, pSamplerPtr;

        internal static List<List<DrawCall>> GlobalDrawCalls { get; private set; }

        static ImGuiEngine() {
            GlobalDrawCalls = new(8);
        }

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
            GlobalDrawCalls.Clear();

            ImGui.NewFrame();
            ImGuiContext.Vertices.Clear();
            ImGuiContext.Indices.Clear();
            
            for (int i = 0; i < ImGui.StandardWindows.Count; i++) {
                ImGui.StandardWindows[i].ResetForNewFrame();
            }
            ImGui.TooltipWindow.ResetForNewFrame();

            ImGuiLowLevel.NewFrameReset();

            Identifier.ResetForNewFrame();
            Styling.ClearAll();
            Coloring.ClearAll();
        }

        public static void Update() {
            ImGui.UpdateHoveringWindow();
            ImGuiInput.Update();

            if (ImGuiBehaviour.WrapCursorInWnd) {
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

                        ImGuiBehaviour.DeactivateWrapCursorInWindow();
                    }
                }
            }
        }

        private static int SortWindowType(GuiWindow a, GuiWindow b) {
            bool c1 = a.Type < b.Type;
            bool c2 = b.Type < a.Type;

            return *(int*)&c2 - *(int*)&c1;
        }

        public static void Render() {
            ImGui.StandardWindows.Sort(SortWindowType);

            for (int i = 0; i < ImGui.StandardWindows.Count; i++) {
                GlobalDrawCalls.Add(ImGui.StandardWindows[i].DrawCalls);
            }

            GlobalDrawCalls.Add(ImGui.TooltipWindow.DrawCalls);

            ImGuiContext.WriteMeshDataToGPU();

            DXStateBackup.Backup();

            var ctx = Direct3DContext.DevCtx;

            ctx.IASetVertexBuffers(0u, ImGuiContext.VertexBuffers, strides, offsets);
            ctx.IASetIndexBuffer(ImGuiContext.IndexBuffer, DXGI_FORMAT.R16_UINT, 0);
            ctx.IASetInputLayout(ImGuiContext.InputLayout);
            ctx.VSSetShader(ImGuiContext.Material.VertexShader!.Shader);
            ctx.PSSetShader(ImGuiContext.Material.PixelShader!.Shader);

            ctx.RSSetState(ImGuiContext.RasterizerState);

            ctx.OMSetBlendState(ImGuiContext.BlendState, null, 0xFFFFFFFF);
            ctx.VSSetConstantBuffers(0, ImGuiContext.ProjectionBuffer);

            for (int i = 0; i < GlobalDrawCalls.Count; i++) {
                var drawList = GlobalDrawCalls[i];

                for (int d = 0; d < drawList.Count; d++) {
                    var call = drawList[d];

                    Marshal.WriteIntPtr(pTexturePtr, call.TexturePointer == IntPtr.Zero ? ImGuiContext.WhiteTexture.DXSRV!._nativePointer : call.TexturePointer);
                    Marshal.WriteIntPtr(pSamplerPtr, call.SamplerPointer == IntPtr.Zero ? ImGuiContext.WhiteTexture.DXSampler!._nativePointer : call.SamplerPointer);

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
                if (ImGui.HoveringWindow is StandardGuiWindow wnd) {
                    ImGui.FocusWindow(wnd);
                }
            }
        }
    }
}
