using System;
using System.Numerics;
using static StbTrueTypeSharp.StbTrueType;
using System.Text;
using DirectDimensional.Core.Utilities;

namespace DirectDimensional.Editor {
    public static unsafe class Utilities {
        /// <summary>
        /// Calculate the largest width of the text based on line and carriage return.
        /// </summary>
        /// <param name="str">Text to calculate</param>
        /// <returns></returns>
        public static float CalcStringWidth(ReadOnlySpan<char> str) {
            float scale = stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);
            int width = 0;

            int spaceWidth;
            stbtt_GetCodepointHMetrics(EditorResources.FontInfo, ' ', &spaceWidth, null);

            int currLineW = 0;
            for (int i = 0; i < str.Length; i++) {
                var c = str[i];

                switch (c) {
                    case ' ':
                        currLineW += spaceWidth;
                        break;

                    case '\t':
                        currLineW += spaceWidth * 4;
                        break;

                    case '\n':
                        width = Math.Max(width, currLineW);
                        currLineW = 0;
                        break;

                    case '\r':
                        width = Math.Max(width, currLineW);
                        currLineW = 0;
                        break;

                    default:
                        if (c < ' ') continue;

                        int advanceWidth;
                        stbtt_GetCodepointHMetrics(EditorResources.FontInfo, str[i], &advanceWidth, null);

                        currLineW += advanceWidth;
                        break;
                }
            }

            return Math.Max(width, currLineW) * scale;
        }

        /// <summary>
        /// Calculate the height of the text based on the line feed character (or <c>\n</c> character)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float CalcStringHeight(ReadOnlySpan<char> str) {
            str = str.TrimEnd('\n').Trim('\r');

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(EditorResources.FontInfo, &ascent, &descent, &lineGap);

            int lineHeight = ascent - descent + lineGap;
            return (str.Count('\n') + 1) * lineHeight * stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);
        }

        /// <summary>
        /// Calculate string size to fit inside a rectangle with a width. Character truncate.
        /// </summary>
        /// <param name="str">String to calculate</param>
        /// <param name="maxWidth">Width of rectangle to fit text into</param>
        /// <returns></returns>
        public static Vector2 CalcStringSizeC(ReadOnlySpan<char> str, float maxWidth = 100000) {
            if (maxWidth <= 0) maxWidth = 100000;

            str = str.TrimEnd('\n').Trim('\r');

            float scale = stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(EditorResources.FontInfo, &ascent, &descent, &lineGap);

            int lineHeight = ascent - descent + lineGap;
            int sizeX = 0, sizeY = lineHeight;

            int spaceWidth;
            stbtt_GetCodepointHMetrics(EditorResources.FontInfo, ' ', &spaceWidth, null);

            int maxWidth2 = (int)(maxWidth / scale);

            int currLineW = 0;
            for (int i = 0; i < str.Length; i++) {
                var c = str[i];

                switch (c) {
                    case ' ':
                        currLineW += spaceWidth;
                        break;

                    case '\t':
                        currLineW += spaceWidth * 4;
                        break;

                    case '\n':
                        sizeX = Math.Max(sizeX, currLineW);
                        sizeY += lineHeight;
                        currLineW = 0;
                        break;

                    case '\r':
                        sizeX = Math.Max(sizeX, currLineW);
                        currLineW = 0;
                        break;

                    default:
                        if (c < ' ') continue;

                        int advanceWidth;
                        stbtt_GetCodepointHMetrics(EditorResources.FontInfo, str[i], &advanceWidth, null);

                        if (currLineW + advanceWidth > maxWidth2) {
                            sizeX = Math.Max(sizeX, currLineW);
                            currLineW = 0;
                            sizeY += lineHeight;
                        }

                        currLineW += advanceWidth;
                        break;
                }
            }

            return new Vector2(Math.Max(sizeX, currLineW), sizeY) * scale;
        }

        /// <summary>
        /// Calculate string size to fit inside a rectangle with a width. Word truncate.
        /// </summary>
        /// <param name="str">String to calculate</param>
        /// <param name="maxWidth">Width of rectangle to fit text into</param>
        /// <returns></returns>
        public static Vector2 CalcStringSizeW(ReadOnlySpan<char> str, float maxWidth = 100000) {
            if (maxWidth <= 0) maxWidth = 100000;

            str = str.TrimEnd('\n').Trim('\r');

            float scale = stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(EditorResources.FontInfo, &ascent, &descent, &lineGap);

            int lineHeight = ascent - descent + lineGap;
            int sizeX = 0, sizeY = 0;

            int spaceWidth;
            stbtt_GetCodepointHMetrics(EditorResources.FontInfo, ' ', &spaceWidth, null);

            int maxWidth2 = (int)(maxWidth / scale);

            int currLineW = 0;

            int begin = 0;
            while (begin < str.Length) {
                var wrap = begin + CalcWordWrapIndex(str[begin..], maxWidth);

                var slice = str[begin..wrap];
                if (begin != 0) slice = slice.TrimStart();

                for (int i = 0; i < slice.Length; i++) {
                    var c = slice[i];

                    switch (c) {
                        case ' ':
                            currLineW += spaceWidth;
                            break;

                        case '\t':
                            currLineW += spaceWidth * 4;
                            break;

                        case '\n':
                            sizeX = Math.Max(sizeX, currLineW);
                            sizeY += lineHeight;
                            currLineW = 0;
                            break;

                        case '\r':
                            sizeX = Math.Max(sizeX, currLineW);
                            currLineW = 0;
                            break;

                        default:
                            if (c < ' ') continue;

                            int advanceWidth;
                            stbtt_GetCodepointHMetrics(EditorResources.FontInfo, str[i], &advanceWidth, null);
                            currLineW += advanceWidth;
                            break;
                    }
                }

                sizeX = Math.Max(sizeX, currLineW);
                sizeY += lineHeight;

                currLineW = 0;

                begin = wrap;
            }

            return new Vector2(sizeX, sizeY) * scale;
        }

        /// <summary>
        /// Calculate where to slice text (word wise) to fit in an area
        /// </summary>
        /// <param name="str">String text</param>
        /// <param name="width">The width of the area</param>
        /// <returns>Exclusive range to cut input string</returns>
        public static int CalcWordWrapIndex(ReadOnlySpan<char> str, float width) {
            int current = 0;

            float scale = stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);
            int widthUnscaled = (int)(width / scale);

            int lineWidth = 0;
            int spaceWidth = 0;
            int wordWidth = 0;

            bool insideWord = true;

            int prevWordEndPos = -1;
            int currWordEndPos = 0;

            while (current < str.Length) {
                var c = str[current];

                if (c == '\n' || c == '\r') {
                    lineWidth = spaceWidth = wordWidth = 0;
                    insideWord = true;
                    current++;

                    continue;
                }

                int characterWidth;
                if (c == '\t') {
                    stbtt_GetCodepointHMetrics(EditorResources.FontInfo, ' ', &characterWidth, null);

                    characterWidth *= 4;
                } else {
                    stbtt_GetCodepointHMetrics(EditorResources.FontInfo, c, &characterWidth, null);
                }

                if (c == ' ' || c == '\t') {
                    if (insideWord) {
                        lineWidth += spaceWidth;
                        spaceWidth = 0;

                        prevWordEndPos = currWordEndPos;
                    }

                    spaceWidth += characterWidth;
                    insideWord = false;
                } else {
                    wordWidth += characterWidth;

                    currWordEndPos = current;

                    lineWidth += characterWidth + spaceWidth;
                    insideWord = true;
                    spaceWidth = 0;
                }

                if (lineWidth > widthUnscaled) {
                    current = (prevWordEndPos == -1 ? currWordEndPos : prevWordEndPos) + 1; // Make it exclusive
                    break;
                }

                current++;
            }

            return current;
        }
    }
}
