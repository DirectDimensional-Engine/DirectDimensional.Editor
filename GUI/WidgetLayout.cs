using DirectDimensional.Core;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DirectDimensional.Editor.GUI {
    public static class WidgetLayout {
        public static void Text(Vector2 size, ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
            if (ImGui.CurrentWindow == null) return;
            Widgets.Text(ImGui.CurrentWindow.Context.GetRect(size), text, anchorH, anchorV);
        }
        public static void Text(ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
            if (ImGui.CurrentWindow == null) return;
            Widgets.Text(ImGui.CurrentWindow.Context.GetRect(FontStack.Current.CalcStringSize(text, Styling.Read<char>(StylingID.TextCharacterFallback))), text, anchorH, anchorV);
        }

        //public static void TextWrapped(Vector2 size, ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
        //    if (ImGui.CurrentWindow == null) return;
        //    Widgets.TextWrapped(ImGui.CurrentWindow.Context.GetRect(size), text, anchorH, anchorV);
        //}
        //public static void TextWrapped(float width, ReadOnlySpan<char> text, HorizontalTextAnchor anchorH = HorizontalTextAnchor.Left, VerticalTextAnchor anchorV = VerticalTextAnchor.Top) {
        //    if (ImGui.CurrentWindow == null) return;
        //    Widgets.TextWrapped(ImGui.CurrentWindow.Context.GetRect(ImGuiFont.Current.CalcStringHeightW(text, width)), text, anchorH, anchorV);
        //}

        public static void Texture(Vector2 size, Texture2D texture, Color32 color) {
            if (ImGui.CurrentWindow == null) return;

            var rect = ImGui.CurrentWindow.Context.GetRect(size);
            Widgets.Texture(rect, texture, color);
        }

        public static int Slider(ReadOnlySpan<char> label, Vector2 size, int input, int min, int max) {
            if (ImGui.CurrentWindow == null) return input;

            return Widgets.Slider(ImGui.CurrentWindow.Context.GetRect(size), label, input, min, max);
        }
        public static float Slider(ReadOnlySpan<char> label, Vector2 size, float input, float min, float max) {
            if (ImGui.CurrentWindow == null) return input;

            return Widgets.Slider(ImGui.CurrentWindow.Context.GetRect(size), label, input, min, max);
        }

        public static bool Button(ReadOnlySpan<char> label, Vector2 size, ButtonFlags flags = default) {
            if (ImGui.CurrentWindow == null) return false;
            return Widgets.Button(ImGui.CurrentWindow.Context.GetRect(size), label, flags);
        }
        public static bool Button(ReadOnlySpan<char> label, ButtonFlags flags = default) {
            if (ImGui.CurrentWindow == null) return false;

            ImGui.DecodeWidgetIdentity(label, out var text, out _);
            return Widgets.Button(ImGui.CurrentWindow.Context.GetRect(FontStack.Current.CalcStringSize(text) + new Vector2(10, 4)), label, flags);
        }

        public static bool Checkbox(ReadOnlySpan<char> label, Vector2 size, bool value) {
            if (ImGui.CurrentWindow == null) return false;

            return Widgets.Checkbox(ImGui.CurrentWindow.Context.GetRect(size), label, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte BitwiseCheckbox8(ReadOnlySpan<char> label, Vector2 size, byte value, byte flags) {
            if (ImGui.CurrentWindow == null) return value;
            return Widgets.BitwiseCheckbox8(ImGui.CurrentWindow.Context.GetRect(size), label, value, flags);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short BitwiseCheckbox16(ReadOnlySpan<char> label, Vector2 size, short value, short flags) {
            if (ImGui.CurrentWindow == null) return value;
            return Widgets.BitwiseCheckbox16(ImGui.CurrentWindow.Context.GetRect(size), label, value, flags);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitwiseCheckbox32(ReadOnlySpan<char> label, Vector2 size, int value, int flags) {
            if (ImGui.CurrentWindow == null) return value;
            return Widgets.BitwiseCheckbox32(ImGui.CurrentWindow.Context.GetRect(size), label, value, flags);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long BitwiseCheckbox64(ReadOnlySpan<char> label, Vector2 size, long value, long flags) {
            if (ImGui.CurrentWindow == null) return value;
            return Widgets.BitwiseCheckbox64(ImGui.CurrentWindow.Context.GetRect(size), label, value, flags);
        }
    }
}
