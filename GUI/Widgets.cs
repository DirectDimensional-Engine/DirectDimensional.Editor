using DirectDimensional.Core;
using DirectDimensional.Core.Utilities;
using System.Numerics;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Bindings;

using DDTexture2D = DirectDimensional.Core.Texture2D;

using static StbTrueTypeSharp.StbTrueType;

namespace DirectDimensional.Editor.GUI {
    public static unsafe class Widgets {
        public static void Text(Rect rect, ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
            if (ImGui.CurrentWindow == null || text.IsEmpty || rect.HasInvalidSize || !Utils.LocalIntersectScissorRect(rect)) return;
            
            var textCol = Coloring.Read(ColoringID.TextColor);
            if (textCol.A == 0) return;

            var desc = EditorResources.FontBitmap.Description;
            Vector2 uvStep = new(1f / desc.Width, 1f / desc.Height);

            float scale = stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(EditorResources.FontInfo, &ascent, &descent, &lineGap);

            float lineSpace = (ascent - descent + lineGap) * scale;

            fixed (stbtt_packedchar* pPacked = &EditorResources.PackedChar[0]) {
                var spaceAdvance = (pPacked + ' ')->xadvance;

                var absPos = Utils.LocalToAbsolute(rect.Position);
                float absMinX = absPos.X, absMaxX = Utils.LocalToAbsolute(rect.Max).X;
                rect.Position = absPos;
                Vector2 rectMax = rect.Max, rectCenter = rect.Center;

                ImGui.CurrentWindow.BeginDrawComposite();

                bool mask = Styling.Read<bool>(StylingID.TextMasking);
                if (mask) {
                    ImGuiLowLevel.BeginScissorRect(rect);
                }

                switch (anchorH) {
                    default: {
                        switch (anchorV) {
                            default: {
                                float yPosition = rect.Position.Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, pPacked, uvStep);
                                    if (yPosition >= rectMax.Y) break; else yPosition += lineSpace;
                                }
                                break;
                            }
                            case VerticalTextAnchor.Middle: {
                                float yPosition = rect.Position.Y + (ascent + lineGap) * scale + (rect.Height - Utilities.CalcStringHeight(text)) / 2;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, pPacked, uvStep);
                                    if (yPosition >= rectMax.Y) break; else yPosition += lineSpace;
                                }
                                break;
                            }
                            case VerticalTextAnchor.Bottom: {
                                float yPosition = rectMax.Y - Utilities.CalcStringHeight(text) + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    RenderingUtils.RenderTextSegmentL2R(line, rect.Position.X, yPosition, float.NegativeInfinity, rectMax.X, pPacked, uvStep);
                                    yPosition += lineSpace;
                                }
                                break;
                            }
                        }
                        break;
                    }
                    case HorizontalTextAnchor.Middle: {
                        switch (anchorV) {
                            default: {
                                var yPosition = rect.Position.Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var segmentWidth = Utilities.CalcStringWidth(line);
                                    RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                    if (yPosition >= rectMax.Y) break; else yPosition += lineSpace;
                                }
                                break;
                            }

                            case VerticalTextAnchor.Middle: {
                                var yPosition = rect.Position.Y + (ascent + lineGap) * scale + (rect.Height - Utilities.CalcStringHeight(text)) / 2;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var segmentWidth = Utilities.CalcStringWidth(line);
                                    RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                    if (yPosition >= rectMax.Y) break; else yPosition += lineSpace;
                                }
                                break;
                            }

                            case VerticalTextAnchor.Bottom: {
                                var yPosition = rectMax.Y - Utilities.CalcStringHeight(text) + (ascent + lineGap) * scale;
                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var segmentWidth = Utilities.CalcStringWidth(line);
                                    RenderingUtils.RenderTextSegmentL2R(line, rectCenter.X - segmentWidth / 2, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                    yPosition += lineSpace;
                                }
                                break;
                            }
                        }
                        break;
                    }
                    case HorizontalTextAnchor.Right: {
                        switch (anchorV) {
                            default: {
                                var yPosition = rect.Position.Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                    if (yPosition >= rectMax.Y) break; else yPosition += lineSpace;
                                }
                                break;
                            }

                            case VerticalTextAnchor.Middle: {
                                var yPosition = rect.Position.Y + (ascent + lineGap) * scale + (rect.Height - Utilities.CalcStringHeight(text)) / 2;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                    if (yPosition >= rectMax.Y) break; else yPosition += lineSpace;
                                }
                                break;
                            }

                            case VerticalTextAnchor.Bottom: {
                                var yPosition = rectMax.Y - Utilities.CalcStringHeight(text) + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || yPosition < rect.Position.Y || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    RenderingUtils.RenderTextSegmentR2L(line, rectMax.X, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                    yPosition += lineSpace;
                                }
                                break;
                            }
                        }
                        break;
                    }
                }

                ImGui.CurrentWindow.EndDrawComposite(EditorResources.FontTextureView, null);
                if (mask) {
                    ImGuiLowLevel.EndScissorRect();
                }
            }
        }
        public static void TextWrapped(Rect rect, ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
            if (ImGui.CurrentWindow == null || text.IsEmpty || rect.HasInvalidSize || !Utils.LocalIntersectScissorRect(rect)) return;

            var textCol = Coloring.Read(ColoringID.TextColor);
            if (textCol.A == 0) return;

            var desc = EditorResources.FontBitmap.Description;
            Vector2 uvStep = new(1f / desc.Width, 1f / desc.Height);

            float scale = stbtt_ScaleForPixelHeight(EditorResources.FontInfo, EditorResources.FontPixelHeight);

            int ascent, descent, lineGap;
            stbtt_GetFontVMetrics(EditorResources.FontInfo, &ascent, &descent, &lineGap);

            float lineSpace = (ascent - descent + lineGap) * scale;

            fixed (stbtt_packedchar* pPacked = &EditorResources.PackedChar[0]) {
                var spaceAdvance = (pPacked + ' ')->xadvance;

                var absPos = Utils.LocalToAbsolute(rect.Position);
                float absMinX = absPos.X, absMaxX = Utils.LocalToAbsolute(rect.Max).X;
                rect.Position = absPos;
                Vector2 rectMax = rect.Max, rectCenter = rect.Center;

                ImGui.CurrentWindow.BeginDrawComposite();

                bool mask = Styling.Read<bool>(StylingID.TextMasking);
                if (mask) {
                    ImGuiLowLevel.BeginScissorRect(rect);
                }

                switch (anchorH) {
                    default: {
                        switch (anchorV) {
                            default: {
                                float yPosition = rect.Position.Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        RenderingUtils.RenderTextSegmentL2R(segment, rect.Position.X, yPosition, absMinX, absMaxX, pPacked, uvStep);

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;

                                        if (yPosition - lineSpace >= rectMax.Y) break;
                                    }

                                    if (yPosition - lineSpace >= rectMax.Y) break;
                                }
                                break;
                            }
                            case VerticalTextAnchor.Middle: {
                                float yPosition = rect.Position.Y + (ascent + lineGap) * scale + (rect.Height - Utilities.CalcStringSizeW(text, rect.Width).Y) / 2;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var lineHeight = Utilities.CalcStringSizeW(text, rect.Width).Y;
                                    if (yPosition + lineHeight < rect.Position.Y) continue;

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        if (yPosition >= rect.Position.Y) {
                                            RenderingUtils.RenderTextSegmentL2R(segment, rect.Position.X, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                        }

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;

                                        if (yPosition - lineSpace >= rectMax.Y) break;
                                    }

                                    if (yPosition - lineSpace >= rectMax.Y) break;
                                }
                                break;
                            }
                            case VerticalTextAnchor.Bottom: {
                                float yPosition = rectMax.Y - Utilities.CalcStringSizeW(text, rect.Width).Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var lineHeight = Utilities.CalcStringSizeW(text, rect.Width).Y;
                                    if (yPosition + lineHeight < rect.Position.Y) continue;

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        if (yPosition >= rect.Position.Y) {
                                            RenderingUtils.RenderTextSegmentL2R(segment, rect.Position.X, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                        }

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    }

                    case HorizontalTextAnchor.Middle: {
                        switch (anchorV) {
                            default: {
                                float yPosition = rect.Position.Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        RenderingUtils.RenderTextSegmentL2R(segment, rectCenter.X - Utilities.CalcStringWidth(segment) / 2, yPosition, absMinX, absMaxX, pPacked, uvStep);

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;

                                        if (yPosition - lineSpace >= rectMax.Y) break;
                                    }

                                    if (yPosition - lineSpace >= rectMax.Y) break;
                                }
                                break;
                            }

                            case VerticalTextAnchor.Middle: {
                                float yPosition = rect.Position.Y + (ascent + lineGap) * scale + (rect.Height - Utilities.CalcStringSizeW(text, rect.Width).Y) / 2;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var lineHeight = Utilities.CalcStringSizeW(text, rect.Width).Y;
                                    if (yPosition + lineHeight < rect.Position.Y) continue;

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        if (yPosition >= rect.Position.Y) {
                                            RenderingUtils.RenderTextSegmentL2R(segment, rectCenter.X - Utilities.CalcStringWidth(segment) / 2, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                        }

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;

                                        if (yPosition - lineSpace >= rectMax.Y) break;
                                    }

                                    if (yPosition - lineSpace >= rectMax.Y) break;
                                }
                                break;
                            }
                            case VerticalTextAnchor.Bottom: {
                                float yPosition = rectMax.Y - Utilities.CalcStringSizeW(text, rect.Width).Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var lineHeight = Utilities.CalcStringSizeW(text, rect.Width).Y;
                                    if (yPosition + lineHeight < rect.Position.Y) continue;

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        if (yPosition >= rect.Position.Y) {
                                            RenderingUtils.RenderTextSegmentL2R(segment, rectCenter.X - Utilities.CalcStringWidth(segment) / 2, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                        }

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    }

                    case HorizontalTextAnchor.Right: {
                        switch (anchorV) {
                            default: {
                                float yPosition = rect.Position.Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        RenderingUtils.RenderTextSegmentR2L(segment, rectMax.X, yPosition, absMinX, absMaxX, pPacked, uvStep);

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;

                                        if (yPosition - lineSpace >= rectMax.Y) break;
                                    }

                                    if (yPosition - lineSpace >= rectMax.Y) break;
                                }
                                break;
                            }
                            case VerticalTextAnchor.Middle: {
                                float yPosition = rect.Position.Y + (ascent + lineGap) * scale + (rect.Height - Utilities.CalcStringSizeW(text, rect.Width).Y) / 2;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var lineHeight = Utilities.CalcStringSizeW(text, rect.Width).Y;
                                    if (yPosition + lineHeight < rect.Position.Y) continue;

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        if (yPosition >= rect.Position.Y) {
                                            RenderingUtils.RenderTextSegmentR2L(segment, rectMax.X, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                        }

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;

                                        if (yPosition - lineSpace >= rectMax.Y) break;
                                    }

                                    if (yPosition - lineSpace >= rectMax.Y) break;
                                }
                                break;
                            }
                            case VerticalTextAnchor.Bottom: {
                                float yPosition = rectMax.Y - Utilities.CalcStringSizeW(text, rect.Width).Y + (ascent + lineGap) * scale;

                                foreach (var line in text.EnumerateLines()) {
                                    if (line.IsEmpty || line.IsWhiteSpace()) {
                                        yPosition += lineSpace;
                                        continue;
                                    }

                                    var lineHeight = Utilities.CalcStringSizeW(text, rect.Width).Y;
                                    if (yPosition + lineHeight < rect.Position.Y) continue;

                                    int wrapBegin = 0;
                                    while (wrapBegin < line.Length) {
                                        int wrapCut = wrapBegin + Utilities.CalcWordWrapIndex(line[wrapBegin..], rect.Width);
                                        var segment = line[wrapBegin..wrapCut];
                                        if (wrapBegin != 0) segment = segment.TrimStart();

                                        if (yPosition >= rect.Position.Y) {
                                            RenderingUtils.RenderTextSegmentR2L(segment, rectMax.X, yPosition, absMinX, absMaxX, pPacked, uvStep);
                                        }

                                        wrapBegin = wrapCut;
                                        yPosition += lineSpace;
                                    }
                                }
                                break;
                            }
                        }

                        break;
                    }
                }

                ImGui.CurrentWindow.EndDrawComposite(EditorResources.FontTextureView, null);
                if (mask) {
                    ImGuiLowLevel.EndScissorRect();
                }
            }
        }

        public static void Texture(Rect rect, ShaderResourceView? textureView, Color32 color) {
            if (ImGui.CurrentWindow == null || color.A == 0) return;
            if (!textureView.Alive()) {
                Logger.Error("Texture View is null or not alive in the native side anymore to be rendered.");
                return;
            }

            rect.Position += ImGuiLowLevel.CurrentCoordinateOffset;

            var vcount = ImGuiLowLevel.VertexCount;
            var icount = ImGuiContext.Indices.Count;

            var rm = rect.Max;
            ImGuiContext.Vertices.Add(new(rect.Position, color, Vector2.Zero));
            ImGuiContext.Vertices.Add(new(new Vector2(rm.X, rect.Y), color, Vector2.UnitX));
            ImGuiContext.Vertices.Add(new(rm, color, Vector2.One));
            ImGuiContext.Vertices.Add(new(new Vector2(rect.X, rm.Y), color, Vector2.UnitY));

            ImGuiLowLevel.AddIndex(vcount + 3);
            ImGuiLowLevel.AddIndex(vcount);
            ImGuiLowLevel.AddIndex(vcount + 2);
            ImGuiLowLevel.AddIndex(vcount + 1);

            ImGui.CurrentWindow.DrawCalls.Add(new() {
                IndexCount = 4,
                IndexLocation = (ushort)icount,

                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
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

        public static bool Checkbox(Rect rect, string id, bool value) {
            if (ImGui.CurrentWindow == null || rect.HasInvalidSize || !Utils.LocalIntersectScissorRect(rect)) return value;

            value ^= ImGuiBehaviour.Button(id, rect, ButtonFlags.None, out _);
            ImGuiRender.DrawRect(rect, value ? Color32.Green : Color32.Red);

            return value;
        }
        public static int Slider(Rect rect, string id, int value, int min, int max) {
            if (ImGui.CurrentWindow == null || rect.HasInvalidSize || !Utils.LocalIntersectScissorRect(rect)) return value;

            Circle handle = default;
            handle.Radius = Styling.Read<int>(StylingID.SliderHandleRadius);

            var holeSize = Styling.Read<int>(StylingID.SliderHoleSize);
            var holeRect = new Rect(rect.X + handle.Radius, rect.Y + (rect.Height - holeSize) / 2, rect.Width - handle.Radius * 2, holeSize);

            handle.Center = new Vector2(DDMath.Remap(value, min, max, holeRect.Position.X, holeRect.Max.X), holeRect.Center.Y);

            Identifier.Push(id);
            {
                var handleID = Identifier.Calculate("HANDLE");

                if (ImGuiBehaviour.CircularButton("HANDLE", handle, ButtonFlags.DetectHeld, out _)) {
                    value = Math.Clamp((int)MathF.Round(DDMath.Remap(ImGuiInput.MousePosition.X, holeRect.Position.X, holeRect.Max.X, min, max)), min, max);
                    handle.Center = new Vector2(DDMath.Remap(value, min, max, holeRect.Position.X, holeRect.Max.X), holeRect.Center.Y);
                }

                if (ImGuiBehaviour.Button("SLIDER_BAR_CLICK", holeRect, ButtonFlags.None, out _)) {
                    value = Math.Clamp((int)MathF.Round(DDMath.Remap(ImGuiInput.MousePosition.X, holeRect.Position.X, holeRect.Max.X, min, max)), min, max);
                    Identifier.SetActiveID(handleID);
                }
            }
            Identifier.Pop();

            ImGui.CurrentWindow.BeginDrawComposite();

            ImGuiRender.AddCompositeRect(holeRect, new Color32(20, 20, 20));
            ImGuiRender.AddCompositeCircle(handle, new Color32(0x7F, 0x7F, 0x7F), 8);

            ImGui.CurrentWindow.EndDrawComposite();

            handle.Center = new Vector2(DDMath.Remap(value, min, max, holeRect.Position.X, holeRect.Max.X), holeRect.Center.Y);

            return value;
        }
        public static float Slider(Rect rect, string id, float value, float min, float max) {
            if (ImGui.CurrentWindow == null || rect.HasInvalidSize || !Utils.LocalIntersectScissorRect(rect)) return value;

            Circle handle = default;
            handle.Radius = Styling.Read<int>(StylingID.SliderHandleRadius);

            var holeSize = Styling.Read<int>(StylingID.SliderHoleSize);
            var holeRect = new Rect(rect.X + handle.Radius, rect.Y + (rect.Height - holeSize) / 2, rect.Width - handle.Radius * 2, holeSize);

            handle.Center = new Vector2(DDMath.Remap(value, min, max, holeRect.Position.X, holeRect.Max.X), holeRect.Center.Y);

            Identifier.Push(id);
            {
                var handleID = Identifier.Calculate("HANDLE");

                if (ImGuiBehaviour.CircularButton("HANDLE", handle, ButtonFlags.DetectHeld, out _)) {
                    value = Math.Clamp(DDMath.Remap(ImGuiInput.MousePosition.X, holeRect.Position.X, holeRect.Max.X, min, max), min, max);
                    handle.Center = new Vector2(DDMath.Remap(value, min, max, holeRect.Position.X, holeRect.Max.X), holeRect.Center.Y);
                }

                if (ImGuiBehaviour.Button("SLIDER_BAR_CLICK", holeRect, ButtonFlags.None, out _)) {
                    value = Math.Clamp(DDMath.Remap(ImGuiInput.MousePosition.X, holeRect.Position.X, holeRect.Max.X, min, max), min, max);
                    Identifier.SetActiveID(handleID);
                }
            }
            Identifier.Pop();

            ImGui.CurrentWindow.BeginDrawComposite();

            ImGuiRender.AddCompositeRect(holeRect, new Color32(20, 20, 20));
            ImGuiRender.AddCompositeCircle(handle, new Color32(0x7F, 0x7F, 0x7F), 8);

            ImGui.CurrentWindow.EndDrawComposite();

            handle.Center = new Vector2(DDMath.Remap(value, min, max, holeRect.Position.X, holeRect.Max.X), holeRect.Center.Y);

            return value;
        }
        public static bool Button(Rect rect, string id, string? text, ButtonFlags flags = default) {
            bool holdingBtnState;
            bool pressed;

            holdingBtnState = ImGuiBehaviour.Button(id, rect, flags | ButtonFlags.DetectHeld, out bool hovered);

            if ((flags & ButtonFlags.DetectHeld) == ButtonFlags.DetectHeld) {
                pressed = holdingBtnState;
            } else {
                pressed = holdingBtnState && (((flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftPressed) || ((flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightPressed) || ((flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddlePressed));
            }

            if (holdingBtnState) {
                ImGuiRender.DrawRect(rect, Coloring.Read(ColoringID.ButtonPressed));
            } else {
                ImGuiRender.DrawRect(rect, Coloring.Read(hovered ? ColoringID.ButtonHovering : ColoringID.ButtonNormal));
            }

            if (!string.IsNullOrWhiteSpace(text)) {
                Text(rect, text, HorizontalTextAnchor.Middle, VerticalTextAnchor.Middle);
            }

            return pressed;
        }

        //public static string StringInput(Rect rect, string id, string input, int maxLength = -1) {
        //    if (ImGui.CurrentWindow == null || rect.HasInvalidSize || !Utils.LocalIntersectScissorRect(rect)) return input;

        //    Identifier.Push(id);
        //    {
        //        bool hovered = rect.Collide(ImGuiInput.MousePosition);

        //        if (Mouse.LeftPressed) {
        //            if (hovered) {
        //                if (!Identifier.Activating) {
        //                    Identifier.SetActiveID();
        //                }
        //            } else {
        //                Identifier.ClearCurrentActiveID();
        //            }
        //        }

        //        if (Identifier.Activating) {
        //            ImGuiRender.DrawRect(rect, Color32.Green);
        //        } else {
        //            ImGuiRender.DrawRect(rect, Color32.Red);
        //        }
        //    }
        //    Identifier.Pop();

        //    return input;
        //}
    }
}
