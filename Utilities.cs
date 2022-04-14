using System;
using System.Numerics;
using static StbTrueTypeSharp.StbTrueType;

namespace DirectDimensional.Editor {
    public static unsafe class Utilities {
        public static Vector2 CalcStringSize(string str) {
            float maxWidth = 0;

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(EditorResources.FontInfo, &ascent, &descent, &lineGap);
            float scale = stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);

            Vector2 size = new(0, ascent - descent + lineGap);

            for (int i = 0; i < str.Length; i++) {
                int advanceWidth;
                stbtt_GetCodepointHMetrics(EditorResources.FontInfo, str[i], &advanceWidth, null);

                size.X += advanceWidth;

                if (i < str.Length - 1) {
                    size.X += stbtt_GetCodepointKernAdvance(EditorResources.FontInfo, str[i], str[i + 1]);
                }

                if (str[i] == '\n') {
                    maxWidth = MathF.Max(maxWidth, size.X);
                    size.X = 0;
                    size.Y += ascent - descent + lineGap;
                }
            }

            return new Vector2(MathF.Max(maxWidth, size.X), size.Y) * scale;
        }
    }
}
