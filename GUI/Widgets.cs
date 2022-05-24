using DirectDimensional.Core;
using DirectDimensional.Core.Utilities;
using System.Numerics;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using DDTexture2D = DirectDimensional.Core.Texture2D;

namespace DirectDimensional.Editor.GUI {
    public static unsafe class Widgets {
        // Configurable
        public static bool EnableLabel { get; set; } = true;

        private static float _labelPercent = 0.5f;
        public static float LabelPercentage {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => _labelPercent;
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set {
                _labelPercent = Math.Clamp(value, 0.2f, 0.8f);
            }
        }

        // Informations
        private static bool _lastWidgetHovered;
        public static bool LastWidgetHovered {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _lastWidgetHovered;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _lastWidgetHovered = value;
        }

        /// <summary>
        /// Reset last widget informations like <seealso cref="LastWidgetHovered"/> to their initial values. Only use for custom Widgets.
        /// </summary>
        public static void ResetWidgetInformations() {
            _lastWidgetHovered = false;
        }

        // Non-interactable widgets
        public static void Text(Rect rect, ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
            if (ImGui.CurrentWindow == null || text.IsEmpty || text.IsWhiteSpace() || rect.HasInvalidSize || !Utilities.LocalIntersectScissorRect(rect)) return;

            var textCol = Coloring.Read(ColoringID.TextColor);
            if (textCol.A == 0) return;

            var currFont = FontStack.Current;

            var absPos = Utilities.LocalToAbsolute(rect.Position);
            float absMinX = absPos.X, absMaxX = Utilities.LocalToAbsolute(rect.Max).X;
            rect.Position = absPos;
            Vector2 rectMax = rect.Max, rectCenter = rect.Center;

            ImGui.CurrentWindow.BeginDrawComposite();

            bool mask = Styling.Read<bool>(StylingID.TextMasking);
            if (mask) {
                LowLevel.BeginScissorRect(rect);
            }

            var fallback = Styling.Read<char>(StylingID.TextCharacterFallback);

            switch (anchorH) {
                default: {
                    switch (anchorV) {
                        default: {
                            float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Middle: {
                            float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringHeight(text)) / 2;

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Bottom: {
                            float yPosition = rectMax.Y - currFont.CalcStringHeight(text) + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, fallback);
                                yPosition += currFont.FontSize;
                            }
                            break;
                        }
                    }
                    break;
                }
                case HorizontalTextAnchor.Middle: {
                    switch (anchorV) {
                        default: {
                            var yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                var segmentWidth = currFont.CalcStringWidth(line, fallback);
                                RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Middle: {
                            var yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringHeight(text)) / 2;

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                var segmentWidth = currFont.CalcStringWidth(line, fallback);
                                RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Bottom: {
                            var yPosition = rectMax.Y - currFont.CalcStringHeight(text) + (currFont.Ascent + currFont.LineGap);
                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                var segmentWidth = currFont.CalcStringWidth(line, fallback);
                                RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, fallback);
                                yPosition += currFont.FontSize;
                            }
                            break;
                        }
                    }
                    break;
                }
                case HorizontalTextAnchor.Right: {
                    switch (anchorV) {
                        default: {
                            var yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Middle: {
                            var yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringHeight(text)) / 2;

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Bottom: {
                            var yPosition = rectMax.Y - currFont.CalcStringHeight(text) + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, fallback);
                                yPosition += currFont.FontSize;
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            ImGui.CurrentWindow.EndDrawComposite(FontStack.Current.Bitmap.DXSRV!, null);
            if (mask) {
                LowLevel.EndScissorRect();
            }
        }
        public static void TextComposite(Rect rect, ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
            if (ImGui.CurrentWindow == null || text.IsEmpty || text.IsWhiteSpace() || rect.HasInvalidSize || !Utilities.LocalIntersectScissorRect(rect)) return;

            var textCol = Coloring.Read(ColoringID.TextColor);
            if (textCol.A == 0) return;

            var currFont = FontStack.Current;

            var absPos = Utilities.LocalToAbsolute(rect.Position);
            float absMinX = absPos.X, absMaxX = Utilities.LocalToAbsolute(rect.Max).X;
            rect.Position = absPos;
            Vector2 rectMax = rect.Max, rectCenter = rect.Center;

            var fallback = Styling.Read<char>(StylingID.TextCharacterFallback);

            switch (anchorH) {
                default: {
                    switch (anchorV) {
                        default: {
                            float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Middle: {
                            float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringHeight(text)) / 2;

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Bottom: {
                            float yPosition = rectMax.Y - currFont.CalcStringHeight(text) + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, fallback);
                                yPosition += currFont.FontSize;
                            }
                            break;
                        }
                    }
                    break;
                }
                case HorizontalTextAnchor.Middle: {
                    switch (anchorV) {
                        default: {
                            var yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                var segmentWidth = currFont.CalcStringWidth(line, fallback);
                                RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Middle: {
                            var yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringHeight(text)) / 2;

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                var segmentWidth = currFont.CalcStringWidth(line, fallback);
                                RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Bottom: {
                            var yPosition = rectMax.Y - currFont.CalcStringHeight(text) + (currFont.Ascent + currFont.LineGap);
                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                var segmentWidth = currFont.CalcStringWidth(line, fallback);
                                RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, fallback);
                                yPosition += currFont.FontSize;
                            }
                            break;
                        }
                    }
                    break;
                }
                case HorizontalTextAnchor.Right: {
                    switch (anchorV) {
                        default: {
                            var yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Middle: {
                            var yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringHeight(text)) / 2;

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, fallback);
                                if (yPosition >= rectMax.Y) break; else yPosition += currFont.FontSize;
                            }
                            break;
                        }
                        case VerticalTextAnchor.Bottom: {
                            var yPosition = rectMax.Y - currFont.CalcStringHeight(text) + (currFont.Ascent + currFont.LineGap);

                            foreach (var line in text.EnumerateDelimiter("\n")) {
                                if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                    yPosition += currFont.FontSize;
                                    continue;
                                }

                                RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, fallback);
                                yPosition += currFont.FontSize;
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }
        // Extreme broken but i'm not bother to fix it yet
        //public static void TextWrapped(Rect rect, ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
        //    if (ImGui.CurrentWindow == null || text.IsEmpty || text.IsWhiteSpace() || rect.HasInvalidSize || !ImGuiUtilities.LocalIntersectScissorRect(rect)) return;

        //    var textCol = Coloring.Read(ColoringID.TextColor);
        //    if (textCol.A == 0) return;

        //    var currFont = ImGuiFont.Current;

        //    var absPos = ImGuiUtilities.LocalToAbsolute(rect.Position);
        //    float absMinX = absPos.X, absMaxX = ImGuiUtilities.LocalToAbsolute(rect.Max).X;
        //    rect.Position = absPos;
        //    Vector2 rectMax = rect.Max, rectCenter = rect.Center;

        //    ImGui.CurrentWindow.BeginDrawComposite();

        //    bool mask = Styling.Read<bool>(StylingID.TextMasking);
        //    if (mask) {
        //        ImGuiLowLevel.BeginScissorRect(rect);
        //    }

        //    var fallback = Styling.Read<char>(StylingID.TextCharacterFallback);

        //    switch (anchorH) {
        //        default: {
        //            switch (anchorV) {
        //                default: {
        //                    float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width, fallback);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            RenderingUtils.RenderTextSegmentL2R(segment, rect.Position.X, yPosition, absMinX, absMaxX, fallback);

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;

        //                            if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                        }

        //                        if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                    }
        //                    break;
        //                }
        //                case VerticalTextAnchor.Middle: {
        //                    float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringSizeW(text, fallback, rect.Width).Y) / 2;

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        var lineHeight = currFont.CalcStringSizeW(text, fallback,  rect.Width).Y;
        //                        if (yPosition + lineHeight < rect.Position.Y) continue;

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width, fallback);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            if (yPosition >= rect.Position.Y) {
        //                                RenderingUtils.RenderTextSegmentL2R(segment, rect.Position.X, yPosition, absMinX, absMaxX, fallback);
        //                            }

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;

        //                            if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                        }

        //                        if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                    }
        //                    break;
        //                }
        //                case VerticalTextAnchor.Bottom: {
        //                    float yPosition = rectMax.Y - currFont.CalcStringSizeW(text, fallback, rect.Width).Y + (currFont.Ascent + currFont.LineGap);

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        var lineHeight = currFont.CalcStringSizeW(text, rect.Width).Y;
        //                        if (yPosition + lineHeight < rect.Position.Y) continue;

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width, fallback);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            if (yPosition >= rect.Position.Y) {
        //                                RenderingUtils.RenderTextSegmentL2R(segment, rect.Position.X, yPosition, absMinX, absMaxX, fallback);
        //                            }

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;
        //                        }
        //                    }
        //                    break;
        //                }
        //            }
        //            break;
        //        }
        //        case HorizontalTextAnchor.Middle: {
        //            switch (anchorV) {
        //                default: {
        //                    float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width, fallback);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            RenderingUtils.RenderTextSegmentL2R(segment, rectCenter.X - currFont.CalcStringWidth(segment) / 2, yPosition, absMinX, absMaxX, fallback);

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;

        //                            if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                        }

        //                        if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                    }
        //                    break;
        //                }

        //                case VerticalTextAnchor.Middle: {
        //                    float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringSizeW(text, fallback, rect.Width).Y) / 2;

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        var lineHeight = currFont.CalcStringSizeW(line, fallback, rect.Width).Y;
        //                        if (yPosition + lineHeight < rect.Position.Y) continue;

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            if (yPosition >= rect.Position.Y) {
        //                                RenderingUtils.RenderTextSegmentL2R(segment, rectCenter.X - currFont.CalcStringWidth(segment) / 2, yPosition, absMinX, absMaxX, fallback);
        //                            }

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;

        //                            if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                        }

        //                        if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                    }
        //                    break;
        //                }
        //                case VerticalTextAnchor.Bottom: {
        //                    float yPosition = rectMax.Y - currFont.CalcStringSizeW(text, rect.Width).Y + (currFont.Ascent + currFont.LineGap);

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        var lineHeight = currFont.CalcStringSizeW(text, rect.Width).Y;
        //                        if (yPosition + lineHeight < rect.Position.Y) continue;

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            if (yPosition >= rect.Position.Y) {
        //                                RenderingUtils.RenderTextSegmentL2R(segment, rectCenter.X - currFont.CalcStringWidth(segment) / 2, yPosition, absMinX, absMaxX, fallback);
        //                            }

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;
        //                        }
        //                    }
        //                    break;
        //                }
        //            }
        //            break;
        //        }
        //        case HorizontalTextAnchor.Right: {
        //            switch (anchorV) {
        //                default: {
        //                    float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap);

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            RenderingUtils.RenderTextSegmentR2L(segment, rectMax.X, yPosition, absMinX, absMaxX, fallback);

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;

        //                            if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                        }

        //                        if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                    }
        //                    break;
        //                }
        //                case VerticalTextAnchor.Middle: {
        //                    float yPosition = rect.Position.Y + (currFont.Ascent + currFont.LineGap) + (rect.Height - currFont.CalcStringSizeW(text, rect.Width).Y) / 2;

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        var lineHeight = currFont.CalcStringSizeW(text, rect.Width).Y;
        //                        if (yPosition + lineHeight < rect.Position.Y) continue;

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            if (yPosition >= rect.Position.Y) {
        //                                RenderingUtils.RenderTextSegmentR2L(segment, rectMax.X, yPosition, absMinX, absMaxX, fallback);
        //                            }

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;

        //                            if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                        }

        //                        if (yPosition - currFont.FontSize >= rectMax.Y) break;
        //                    }
        //                    break;
        //                }
        //                case VerticalTextAnchor.Bottom: {
        //                    float yPosition = rectMax.Y - currFont.CalcStringSizeW(text, rect.Width).Y + (currFont.Ascent + currFont.LineGap);

        //                    foreach (var line in text.EnumerateDelimiter("\n")) {
        //                        if (line.IsEmpty || line.IsWhiteSpace()) {
        //                            yPosition += currFont.FontSize;
        //                            continue;
        //                        }

        //                        var lineHeight = currFont.CalcStringSizeW(text, rect.Width).Y;
        //                        if (yPosition + lineHeight < rect.Position.Y) continue;

        //                        int wrapBegin = 0;
        //                        while (wrapBegin < line.Length) {
        //                            int wrapCut = wrapBegin + currFont.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
        //                            var segment = line[wrapBegin..wrapCut];
        //                            if (wrapBegin != 0) segment = segment.TrimStart();

        //                            if (yPosition >= rect.Position.Y) {
        //                                RenderingUtils.RenderTextSegmentR2L(segment, rectMax.X, yPosition, absMinX, absMaxX, fallback);
        //                            }

        //                            wrapBegin = wrapCut;
        //                            yPosition += currFont.FontSize;
        //                        }
        //                    }
        //                    break;
        //                }
        //            }

        //            break;
        //        }
        //    }

        //    ImGui.CurrentWindow.EndDrawComposite(currFont.Bitmap.DXSRV!, null);
        //    if (mask) {
        //        ImGuiLowLevel.EndScissorRect();
        //    }
        //}

        public static void Texture(Rect rect, ShaderResourceView? textureView, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0 || !Utilities.LocalIntersectScissorRect(rect)) return;
            if (!textureView.Alive()) {
                Logger.Error("Texture View is null or not alive in the native side anymore to be rendered.");
                return;
            }

            rect.Position = Utilities.LocalToAbsolute(rect.Position);

            var vcount = LowLevel.VertexCount;
            var icount = Context.Indices.Count;

            var rm = rect.Max;
            Context.Vertices.Add(new(rect.Position, color, Vector2.Zero));
            Context.Vertices.Add(new(new Vector2(rm.X, rect.Y), color, Vector2.UnitX));
            Context.Vertices.Add(new(rm, color, Vector2.One));
            Context.Vertices.Add(new(new Vector2(rect.X, rm.Y), color, Vector2.UnitY));

            LowLevel.AddIndex(vcount + 3);
            LowLevel.AddIndex(vcount);
            LowLevel.AddIndex(vcount + 2);
            LowLevel.AddIndex(vcount + 1);

            ImGui.CurrentWindow.DrawCalls.Add(new() {
                IndexCount = 4,
                IndexLocation = (ushort)icount,

                ScissorsRect = LowLevel.CurrentScissorRect,
                Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleStrip,

                TexturePointer = textureView._nativePointer,
            });
        }
        public static void Texture(Rect rect, DDTexture2D texture, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0) return;
            if (!texture.IsRenderable) {
                Logger.Error("Texture cannot be rendered.");
                return;
            }

            Texture(rect, texture.DXSRV, color);
        }

        // Checkbox family
        public static bool Checkbox(Rect rect, ReadOnlySpan<char> label, bool value) {
            ResetWidgetInformations();
            if (ImGui.CurrentWindow == null || rect.HasInvalidSize || !Utilities.LocalIntersectScissorRect(rect)) return value;

            ImGui.DecodeWidgetIdentity(label, out var name, out var id);

            if (EnableLabel) {
                SeperateWidgetAndLabelRect(rect, out var text, out var widget);

                Text(text, name, HorizontalTextAnchor.Left, VerticalTextAnchor.Top);
                
                var smallestEdge = MathF.Min(rect.Width, rect.Height);
                var checkBoxRect = new Rect(widget.Position, smallestEdge);

                value ^= Behaviours.Button(id, checkBoxRect, ButtonFlags.None, out _lastWidgetHovered);
                Drawings.DrawRect(checkBoxRect, value ? Color32.Green : Color32.Red);
            } else {
                var smallestEdge = MathF.Min(rect.Width, rect.Height);
                var checkBoxRect = new Rect(rect.Position, smallestEdge);

                value ^= Behaviours.Button(id, checkBoxRect, ButtonFlags.None, out _lastWidgetHovered);
                Drawings.DrawRect(checkBoxRect, value ? Color32.Green : Color32.Red);
            }

            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte BitwiseCheckbox8(Rect rect, ReadOnlySpan<char> label, byte value, byte flags) {
            return (byte)(Checkbox(rect, label, (value & flags) == flags) ? (value | flags) : (value & ~flags));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short BitwiseCheckbox16(Rect rect, ReadOnlySpan<char> label, short value, short flags) {
            return (short)(Checkbox(rect, label, (value & flags) == flags) ? (value | flags) : (value & ~flags));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitwiseCheckbox32(Rect rect, ReadOnlySpan<char> label, int value, int flags) {
            return Checkbox(rect, label, (value & flags) == flags) ? (value | flags) : (value & ~flags);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long BitwiseCheckbox64(Rect rect, ReadOnlySpan<char> label, long value, long flags) {
            return Checkbox(rect, label, (value & flags) == flags) ? (value | flags) : (value & ~flags);
        }

        // Slider family
        public static int Slider(Rect rect, ReadOnlySpan<char> label, int input, int min, int max) {
            ResetWidgetInformations();
            if (ImGui.CurrentWindow == null || rect.HasInvalidSize || !Utilities.LocalIntersectScissorRect(rect)) return input;

            ImGui.DecodeWidgetIdentity(label, out var name, out var id);

            Circle handle = default;
            handle.Radius = 6;
            Rect holeRect;

            if (EnableLabel) {
                SeperateWidgetAndLabelRect(rect, out var text, out var widget);

                Text(text, name, HorizontalTextAnchor.Left, VerticalTextAnchor.Top);
                holeRect = new Rect(widget.X + handle.Radius, widget.Y + widget.Height / 2 - 3, widget.Width - handle.Radius * 2, 6);
            } else {
                holeRect = new Rect(rect.X + handle.Radius, rect.Y + rect.Height / 2 - 3, rect.Width - handle.Radius * 2, 6);
            }

            handle.Center = new Vector2(DDMath.Remap(input, min, max, holeRect.X, holeRect.MaxX), holeRect.CenterY);
            
            Identifier.Push(id);
            {
                var handleID = Identifier.Calculate("HANDLE");

                if (Behaviours.CircularButton("HANDLE", handle, ButtonFlags.DetectHeld, out _lastWidgetHovered)) {
                    input = Math.Clamp((int)MathF.Round(DDMath.Remap(Input.MousePosition.X, holeRect.Position.X, holeRect.MaxX, min, max)), min, max);
                    handle.Center = new Vector2(DDMath.Remap(input, min, max, holeRect.Position.X, holeRect.MaxX), holeRect.CenterY);
                }

                if (Behaviours.Button("SLIDER_BAR_CLICK", holeRect, ButtonFlags.None, out _lastWidgetHovered)) {
                    input = Math.Clamp((int)MathF.Round(DDMath.Remap(Input.MousePosition.X, holeRect.Position.X, holeRect.MaxX, min, max)), min, max);
                    Identifier.SetActiveID(handleID);
                }
            }
            Identifier.Pop();

            ImGui.CurrentWindow.BeginDrawComposite();

            Drawings.AddCompositeRect(holeRect, new Color32(20, 20, 20));
            Drawings.AddCompositeCircle(handle, new Color32(0x7F, 0x7F, 0x7F), 8);

            ImGui.CurrentWindow.EndDrawComposite();

            handle.Center = new Vector2(DDMath.Remap(input, min, max, holeRect.Position.X, holeRect.MaxX), holeRect.CenterY);

            return input;
        }
        public static float Slider(Rect rect, ReadOnlySpan<char> label, float input, float min, float max) {
            ResetWidgetInformations();
            if (ImGui.CurrentWindow == null || rect.HasInvalidSize || !Utilities.LocalIntersectScissorRect(rect)) return input;

            ImGui.DecodeWidgetIdentity(label, out var name, out var id);

            Circle handle = default;
            handle.Radius = 6;
            Rect holeRect;

            if (EnableLabel) {
                SeperateWidgetAndLabelRect(rect, out var text, out var widget);

                Text(text, name, HorizontalTextAnchor.Left, VerticalTextAnchor.Top);
                holeRect = new Rect(widget.X + handle.Radius, widget.Y + widget.Height / 2 - 3, widget.Width - handle.Radius * 2, 6);
            } else {
                holeRect = new Rect(rect.X + handle.Radius, rect.Y + rect.Height / 2 - 3, rect.Width - handle.Radius * 2, 6);
            }

            handle.Center = new Vector2(DDMath.Remap(input, min, max, holeRect.X, holeRect.MaxX), holeRect.CenterY);

            Identifier.Push(id);
            {
                var handleID = Identifier.Calculate("HANDLE");

                if (Behaviours.CircularButton("HANDLE", handle, ButtonFlags.DetectHeld, out _lastWidgetHovered)) {
                    input = Math.Clamp(DDMath.Remap(Input.MousePosition.X, holeRect.Position.X, holeRect.MaxX, min, max), min, max);
                    handle.Center = new Vector2(DDMath.Remap(input, min, max, holeRect.Position.X, holeRect.MaxX), holeRect.CenterY);
                }

                if (Behaviours.Button("SLIDER_BAR_CLICK", holeRect, ButtonFlags.None, out _lastWidgetHovered)) {
                    input = Math.Clamp(DDMath.Remap(Input.MousePosition.X, holeRect.Position.X, holeRect.MaxX, min, max), min, max);
                    Identifier.SetActiveID(handleID);
                }
            }
            Identifier.Pop();

            ImGui.CurrentWindow.BeginDrawComposite();

            Drawings.AddCompositeRect(holeRect, new Color32(20, 20, 20));
            Drawings.AddCompositeCircle(handle, new Color32(0x7F, 0x7F, 0x7F), 8);

            ImGui.CurrentWindow.EndDrawComposite();

            handle.Center = new Vector2(DDMath.Remap(input, min, max, holeRect.Position.X, holeRect.MaxX), holeRect.CenterY);

            return input;
        }

        // Button family
        public static bool Button(Rect rect, ReadOnlySpan<char> label, ButtonFlags flags = default) {
            ResetWidgetInformations();
            if (ImGui.CurrentWindow == null || !Utilities.LocalIntersectScissorRect(rect)) return false;

            bool holdingBtnState;
            bool pressed;

            ImGui.DecodeWidgetIdentity(label, out var text, out var id);

            holdingBtnState = Behaviours.Button(id, rect, flags | ButtonFlags.DetectHeld, out _lastWidgetHovered);

            if ((flags & ButtonFlags.DetectHeld) == ButtonFlags.DetectHeld) {
                pressed = holdingBtnState;
            } else {
                pressed = holdingBtnState && (((flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftPressed) || ((flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightPressed) || ((flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddlePressed));
            }

            if ((flags & ButtonFlags.NoTexture) != ButtonFlags.NoTexture) {
                var col = holdingBtnState ? Coloring.Read(ColoringID.ButtonPressed) : Coloring.Read(_lastWidgetHovered ? ColoringID.ButtonHovering : ColoringID.ButtonNormal);

                if (ImGui.CurrentWindow.IsDrawingComposite) {
                    Drawings.AddCompositeRect(rect, col);
                } else {
                    Drawings.DrawRect(rect, col);
                }
            }

            Text(rect, text, HorizontalTextAnchor.Middle, VerticalTextAnchor.Middle);

            return pressed;
        }

        public readonly struct MultiButtonProfile {
            public readonly Vector2 Position { get; init; }
            public readonly Vector2 Extrude { get; init; }
            public readonly string? Label { get; init; }
            public readonly ButtonFlags Flags { get; init; }

            public MultiButtonProfile(Vector2 pos, Vector2 extrude, string? label) : this(pos, extrude, label, ButtonFlags.None)  { }

            public MultiButtonProfile(Vector2 pos, Vector2 extrude, string? label, ButtonFlags flags) {
                Position = pos;
                Extrude = Vector2.Max(extrude, Vector2.Zero);
                Label = label;
                Flags = flags;
            }
        }
        public static unsafe int MultiButtons(ReadOnlySpan<MultiButtonProfile> profiles) {
            ResetWidgetInformations();
            if (ImGui.CurrentWindow == null) return -1;

            int pressedIndex = -1;
            int hoverIndex = -1;
            int holdingIndex = -1;

            int index = 0;

            Rect* btnCache = stackalloc Rect[profiles.Length];

            foreach (var profile in profiles) {
                bool holdingBtnState;
                bool pressed;

                ImGui.DecodeWidgetIdentity(profile.Label, out var text, out var id);

                var textSize = FontStack.Current.CalcStringSize(text);
                btnCache[index] = new Rect(profile.Position, textSize + profile.Extrude);

                holdingBtnState = Behaviours.Button(id, btnCache[index], profile.Flags | ButtonFlags.DetectHeld, out bool hovered);
                if ((profile.Flags & ButtonFlags.DetectHeld) == ButtonFlags.DetectHeld) {
                    pressed = holdingBtnState;
                } else {
                    pressed = holdingBtnState && (((profile.Flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftPressed) || ((profile.Flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightPressed) || ((profile.Flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddlePressed));
                }

                if (pressed) pressedIndex = index;
                if (hovered) {
                    hoverIndex = index;
                    _lastWidgetHovered = true;
                }
                if (holdingBtnState) holdingIndex = index;

                index++;
            }

            ImGui.CurrentWindow.BeginDrawComposite();

            index = 0;
            foreach (var profile in profiles) {
                if ((profile.Flags & ButtonFlags.NoTexture) != ButtonFlags.NoTexture) {
                    var col = holdingIndex == index ? Coloring.Read(ColoringID.ButtonPressed) : Coloring.Read(hoverIndex == index ? ColoringID.ButtonHovering : ColoringID.ButtonNormal);

                    if (ImGui.CurrentWindow.IsDrawingComposite) {
                        Drawings.AddCompositeRect(btnCache[index], col);
                    } else {
                        Drawings.DrawRect(btnCache[index], col);
                    }
                }

                index++;
            }

            ImGui.CurrentWindow.EndDrawComposite();

            ImGui.CurrentWindow.BeginDrawComposite();

            index = 0;
            foreach (var profile in profiles) {
                ImGui.DecodeWidgetIdentity(profile.Label, out var text, out _);

                TextComposite(btnCache[index], text, HorizontalTextAnchor.Middle, VerticalTextAnchor.Middle);

                index++;
            }

            ImGui.CurrentWindow.EndDrawComposite(FontStack.Current.Bitmap);

            return pressedIndex;
        }

        // Not widget related
        public static void SeperateWidgetAndLabelRect(Rect input, out Rect label, out Rect widget) {
            label = new Rect(input.X, input.Y, input.Width * _labelPercent, input.Height);
            widget = new Rect(input.X + label.Width, input.Y, input.Width - label.Width, input.Height);
        }
    }
}
