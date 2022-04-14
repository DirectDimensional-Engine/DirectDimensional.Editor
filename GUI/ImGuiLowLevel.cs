using DirectDimensional.Core;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.WinAPI;

namespace DirectDimensional.Editor.GUI {
    public static unsafe class ImGuiLowLevel {
        private static readonly Stack<RECT> _scissorRectStack;
        public static RECT ScissorRect {
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

        private static int _oldIndexCount = -1;

        public static int VertexCount => ImGuiContext.Vertices.Count;
        public static int IndexCount => ImGuiContext.Indices.Count;

        static ImGuiLowLevel() {
            _scissorRectStack = new(12);
        }

        /// <summary>
        /// Begin draw complicated mesh in 1 single draw call
        /// </summary>
        public static void BeginDrawComposite() {
            if (_oldIndexCount >= 0) {
                Logger.Warn(nameof(BeginDrawComposite) + " cannot be called because engine is in drawing composite state. Operation cancelled.");
                return;
            }

            _oldIndexCount = ImGuiContext.Indices.Count;
        }

        /// <summary>
        /// Apply 1 draw call to system
        /// </summary>
        public static void EndDrawComposite() {
            if (_oldIndexCount < 0) {
                Logger.Warn(nameof(EndDrawComposite) + " cannot be called because engine is not in drawing composite state. Operation cancelled.");
                return;
            }

            ImGuiContext.DrawCalls.Add(new DrawCall {
                IndexCount = (uint)(ImGuiContext.Indices.Count - _oldIndexCount),
                IndexLocation = (uint)_oldIndexCount,
            });

            _oldIndexCount = -1;
        }

        /// <summary>
        /// Apply 1 draw call with texture to system
        /// </summary>
        public static void EndDrawComposite(ShaderResourceView? pTexture, SamplerState? pSampler) {
            if (_oldIndexCount < 0) {
                Logger.Warn(nameof(EndDrawComposite) + " cannot be called because engine is not in drawing composite state. Operation cancelled.");
                return;
            }

            ImGuiContext.DrawCalls.Add(new DrawCall {
                IndexCount = (uint)(ImGuiContext.Indices.Count - _oldIndexCount),
                IndexLocation = (uint)_oldIndexCount,

                TexturePointer = pTexture.GetNativePtr(),
                SamplerPointer = pSampler.GetNativePtr(),
                ScissorsRect = ScissorRect,
            });

            _oldIndexCount = -1;
        }

        public static bool EnsureDrawingComposite() {
            if (_oldIndexCount >= 0) {
                Logger.Warn("Engine is not in drawing composite state.");
                return false;
            }

            return true;
        }

        public static int DrawCount => ImGuiContext.DrawCalls.Count;

        public static void AddVertex(Vertex vertex) {
            ImGuiContext.Vertices.Add(vertex);
        }

        public static void AddVertices(params Vertex[] vertices) {
            ImGuiContext.Vertices.AddRange(vertices);
        }

        public static void AddIndex(int index) {
            ImGuiContext.Indices.Add((ushort)index);
        }

        public static void AddIndices(params int[] indices) {
            for (int i = 0; i < indices.Length; i++) {
                ImGuiContext.Indices.Add((ushort)indices[i]);
            }
        }

        public static void BeginScissorRect(Rect rect) {
            _scissorRectStack.Push(new RECT() {
                Left = (int)rect.Left,
                Right = (int)rect.Right,
                Bottom = (int)Math.Max(rect.Bottom, rect.Top),  // Prevent confusion between 2 coordinate systems
                Top = (int)Math.Min(rect.Bottom, rect.Top),
            });
        }

        public static bool EndScissorRect() {
            return _scissorRectStack.TryPop(out _);
        }

        public static void DrawTexturedRect(Vertex topLeft, Vertex topRight, Vertex bottomRight, Vertex bottomLeft, ShaderResourceView? srv, SamplerState? sampler) {
            var vcount = ImGuiContext.Vertices.Count;
            var icount = ImGuiContext.Indices.Count;

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

            ImGuiContext.DrawCalls.Add(new DrawCall {
                IndexCount = 6,
                IndexLocation = (ushort)icount,

                ScissorsRect = ScissorRect,
                TexturePointer = srv.GetNativePtr(),
                SamplerPointer = sampler.GetNativePtr(),
            });
        }
    }
}
