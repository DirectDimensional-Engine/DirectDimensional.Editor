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
        public static void RenderTextSegmentL2R(ReadOnlySpan<char> text, float x, float y, float minX, float maxX, char fallback) {
            if (x >= maxX) return;

            var currFont = FontStack.Current;

            var col = Coloring.Read(ColoringID.TextColor);
            var oldX = x;

            stbtt_packedchar? _fallback = currFont.GetPackedChar(fallback);

            for (int i = 0; i < text.Length; i++) {
                var c = text[i];

                switch (c) {
                    case '\r': x = oldX; break;
                    case ' ': x += currFont.SpaceWidth; break;
                    case '\t': x += currFont.SpaceWidth * 4; break;
                    default:
                        if (currFont.TryGetPackedChar(c, out var packed)) {
                            var xadv = x + packed.xadvance;

                            if (xadv >= minX && x <= maxX) {
                                Drawings.AddCharacterQuad_Raw(x, y, packed, currFont.BitmapUVStep, col);
                            }
                            x = xadv;
                        } else if (_fallback.HasValue) {
                            var xadv = x + _fallback.Value.xadvance;

                            if (xadv >= minX && x <= maxX) {
                                Drawings.AddCharacterQuad_Raw(x, y, _fallback.Value, currFont.BitmapUVStep, col);
                            }
                            x = xadv;
                        }
                        break;
                }

                if (x > maxX) {
                    var nextReturn = text[i..].IndexOf('\r');

                    if (nextReturn != -1) {
                        i += nextReturn;
                        x = oldX;
                    } else break;
                }
            }
        }

        /// <summary>
        /// Same as <seealso cref="RenderTextSegmentL2R(ReadOnlySpan{char}, float, float, float, float)"/>, but text will be built from Right to Left
        /// </summary>
        public static void RenderTextSegmentR2L(ReadOnlySpan<char> text, float x, float y, float minX, float maxX, char fallback) {
            if (x < minX) return;

            var currFont = FontStack.Current;
            var col = Coloring.Read(ColoringID.TextColor);
            var oldX = x;

            stbtt_packedchar? _fallback = currFont.GetPackedChar(fallback);

            for (int i = text.Length - 1; i >= 0; i--) {
                var c = text[i];

                switch (c) {
                    case '\r': x = oldX; continue;
                    case ' ': x -= currFont.SpaceWidth; continue;
                    case '\t': x -= currFont.SpaceWidth * 4; continue;
                    default:
                        if (c < ' ') continue;

                        if (currFont.TryGetPackedChar(c, out var packed)) {
                            x -= packed.xadvance;
                            if (x < maxX) {
                                Drawings.AddCharacterQuad_Raw(x, y, packed, currFont.BitmapUVStep, col);
                            }
                        } else if (_fallback.HasValue) {
                            x -= _fallback.Value.xadvance;
                            if (x < maxX) {
                                Drawings.AddCharacterQuad_Raw(x, y, _fallback.Value, currFont.BitmapUVStep, col);
                            }
                        }
                        break;
                }

                if (x < minX) {
                    var nextReturn = text[..i].IndexOf('\r');
                    if (nextReturn != -1) {
                        i = nextReturn - 1;
                        x = oldX;
                    } else break;
                }
            }
        }
    }
}
