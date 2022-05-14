using DirectDimensional.Core;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.WinAPI;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DirectDimensional.Editor.GUI {
    /// <summary>
    /// Backend to control every special control function of ImGui. Not recommended to use.
    /// </summary>
    public static unsafe class ImGuiLowLevel {
        private static readonly Stack<RECT> _scissorRectStack;

        private static Vector2 _coordinate;
        private static readonly Stack<Vector2> _coordinateStack;

        public static int ScissorRectCount => _scissorRectStack.Count;

        public readonly struct ScissorRectScope : IDisposable {
            public void Dispose() {
                EndScissorRect();
            }
        }
        public static ScissorRectScope ScissorRect(Rect r, bool clipLast = true) {
            BeginScissorRect(r, clipLast);
            return default;
        }

        public static RECT CurrentScissorRect {
            get {
                if (_scissorRectStack.TryPeek(out var res)) return res;

                return new RECT() {
                    Bottom = 100000,
                    Top = -100000,
                    Left = -100000,
                    Right = 100000,
                };
            }
        }

        public readonly struct CoordinateOffsetScope : IDisposable {
            public void Dispose() {
                EndCoordinateOffset();
            }
        }
        public static CoordinateOffsetScope CoordinateOffset(Vector2 origin) {
            BeginCoordinateOffset(origin);
            return default;
        }

        public static Vector2 CurrentCoordinateOffset => _coordinate;

        public static int VertexCount => ImGuiContext.Vertices.Count;
        public static int IndexCount => ImGuiContext.Indices.Count;

        static ImGuiLowLevel() {
            _coordinateStack = new();
            _scissorRectStack = new(12);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddVertex(Vertex vertex) {
            ImGuiContext.Vertices.Add(vertex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddVertices(params Vertex[] vertices) {
            ImGuiContext.Vertices.AddRange(vertices);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIndex(int index) {
            ImGuiContext.Indices.Add((ushort)index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AddIndices(params int[] indices) {
            for (int i = 0; i < indices.Length; i++) {
                ImGuiContext.Indices.Add((ushort)indices[i]);
            }
        }

        public static void BeginFullscreenScissorRect() {
            _scissorRectStack.Push(new RECT() {
                Bottom = 100000,
                Top = -100000,
                Left = -100000,
                Right = 100000,
            });
        }

        public static void BeginScissorRect(Rect rect, bool clipLast = true) {
            if (clipLast && _scissorRectStack.TryPeek(out var peek)) {
                var rm = rect.Max;

                RECT r = new() {
                    Left = (int)Math.Max(rect.Position.X, peek.Left),
                    Top = (int)Math.Max(rect.Position.Y, peek.Top),
                };

                r.Right = (int)Math.Min(rm.X, peek.Right);
                r.Bottom = (int)Math.Min(rm.Y, peek.Bottom);

                // handle the situation when the RECT is invalid, then we push a rect with the size of 0, which mean no rendering
                r.Right = Math.Max(r.Right, r.Left);
                r.Bottom = Math.Max(r.Bottom, r.Top);

                _scissorRectStack.Push(r);
            } else {
                var max = rect.Max;

                _scissorRectStack.Push(new RECT() {
                    Left = (int)rect.Position.X,
                    Top = (int)rect.Position.Y,

                    Right = (int)max.X,
                    Bottom = (int)max.Y,
                });
            }
        }

        public static bool EndScissorRect() {
            return _scissorRectStack.TryPop(out _);
        }

        public static void BeginCoordinateOffset(Vector2 origin) {
            _coordinateStack.Push(origin);
            _coordinate += origin;
        }

        public static void BeginOriginCoordinate() {
            _coordinateStack.Push(-_coordinate);
            _coordinate = default;
        }

        public static bool EndCoordinateOffset() {
            if (_coordinateStack.TryPop(out var vector)) {
                _coordinate -= vector;
                return true;
            }
            return false;
        }

        public static void DrawTexturedRect(Vertex topLeft, Vertex topRight, Vertex bottomRight, Vertex bottomLeft, ShaderResourceView? srv, SamplerState? sampler) {
            if (ImGui.CurrentWindow == null) return;
            
            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

            topLeft.Position += CurrentCoordinateOffset;
            topRight.Position += CurrentCoordinateOffset;
            bottomRight.Position += CurrentCoordinateOffset;
            bottomLeft.Position += CurrentCoordinateOffset;

            ImGuiContext.Vertices.Add(topLeft);
            ImGuiContext.Vertices.Add(topRight);
            ImGuiContext.Vertices.Add(bottomRight);
            ImGuiContext.Vertices.Add(bottomLeft);

            ImGuiContext.Indices.Add((ushort)vcount);
            ImGuiContext.Indices.Add((ushort)(vcount + 1));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 2));
            ImGuiContext.Indices.Add((ushort)(vcount + 3));
            ImGuiContext.Indices.Add((ushort)vcount);

            ImGui.CurrentWindow.DrawCalls.Add(new DrawCall {
                IndexCount = 6,
                IndexLocation = (ushort)icount,

                ScissorsRect = CurrentScissorRect,
                TexturePointer = srv.GetNativePtr(),
                SamplerPointer = sampler.GetNativePtr(),
            });
        }

        internal static void NewFrameReset() {
            _scissorRectStack.Clear();
            _coordinateStack.Clear();

            _coordinate = default;
        }
    }
}
