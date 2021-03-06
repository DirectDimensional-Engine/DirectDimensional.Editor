using System.Numerics;
using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public struct Vertex {
        public Vector2 Position;
        public Color32 Color;
        public Vector2 TexCoord;

        public Vertex(Vector2 position, Color32 col, Vector2 uv) {
            Position = position;
            Color = col;
            TexCoord = uv;
        }

        public Vertex(Vector2 position, Vector2 uv) : this(position, Color32.White, uv) { }
        public Vertex(Vector2 position, Color32 color) : this(position, color, Vector2.Zero) { }
        public Vertex(Vector2 position) : this(position, Vector2.Zero) { }
    }
}
