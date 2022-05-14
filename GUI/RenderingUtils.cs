using System.Numerics;
using DirectDimensional.Core;
using DirectDimensional.Core.Utilities;

using static StbTrueTypeSharp.StbTrueType;

namespace DirectDimensional.Editor.GUI {
    public static unsafe class RenderingUtils {
        /// <summary>
        /// Render a text segment with culling when outside of allowed zone. Doesn't handle vertical layout cases. Doesn't check for render conditions such as text color alpha, drawing composite, coordinate offset, etc... Text will be built from Left to Right
        /// </summary>
        /// <param name="text">Text to render</param>
        /// <param name="x">X position of the text to render.</param>
        /// <param name="minX">Minimum horizontal position to create character quad</param>
        /// <param name="maxX">Maximum horizontal position to create character quad</param>
        /// <param name="packed">Packed pointer for each character glyphs</param>
        public static void RenderTextSegmentL2R(ReadOnlySpan<char> text, float x, float y, float minX, float maxX, stbtt_packedchar* packed, Vector2 bitmapUVStep) {
            if (x >= maxX) return;
            
            var col = Coloring.Read(ColoringID.TextColor);
            float spaceW = (packed + ' ')->xadvance;

            var oldX = x;

            for (int i = 0; i < text.Length; i++) {
                var c = text[i];

                switch (c) {
                    case '\r': x = oldX; continue;
                    case ' ': x += spaceW; continue;
                    case '\t': x += spaceW * 4; continue;
                    default:
                        if (c < ' ') continue;

                        var ptr = packed + c;

                        var xadv = x + ptr->xadvance;

                        if (xadv >= minX && x <= maxX) {
                            ImGuiRender.AddCharacterQuad_Raw(x, y, ptr, bitmapUVStep, col);
                        }
                        x = xadv;
                        break;
                }

                if (x > maxX) return;
            }
        }

        /// <summary>
        /// Same as <seealso cref="RenderTextSegmentL2R(ReadOnlySpan{char}, ref float, float, float, float, stbtt_packedchar*, Vector2)"/>, but text will be built from Right to Left
        /// </summary>
        public static void RenderTextSegmentR2L(ReadOnlySpan<char> text, float x, float y, float minX, float maxX, stbtt_packedchar* packed, Vector2 bitmapUVStep) {
            if (x < minX) return;

            var col = Coloring.Read(ColoringID.TextColor);
            float spaceW = (packed + ' ')->xadvance;

            var oldX = x;

            for (int i = text.Length - 1; i >= 0; i--) {
                var c = text[i];

                switch (c) {
                    case '\r': x = oldX; continue;
                    case ' ': x -= spaceW; continue;
                    case '\t': x -= spaceW * 4; continue;
                    default:
                        if (c < ' ') continue;

                        var ptr = packed + c;

                        x -= ptr->xadvance;
                        if (x < maxX) {
                            ImGuiRender.AddCharacterQuad_Raw(x, y, ptr, bitmapUVStep, col);
                        }
                        break;
                }

                if (x < minX) return;
            }
        }
    }
}
