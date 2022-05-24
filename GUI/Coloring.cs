using System;
using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public enum ColoringID {
        WindowTitle,
        WindowBackground,
        WindowBorder,

        ButtonNormal,
        ButtonHovering,
        ButtonPressed,

        TextColor,

        InputFieldNormal,

        // Group Coloring
        ToolbarButtonBackground,
    }

    public static class Coloring {
        private static readonly Dictionary<ColoringID, Stack<Color32>> _colors;

        static Coloring() {
            var enums = Enum.GetValues<ColoringID>();
            _colors = new(enums.Length);

            for (int i = 0; i < enums.Length; i++) {
                _colors.Add(enums[i], new());
            }

            _colors[ColoringID.WindowTitle].Push(new Color32(0x00, 0x4B, 0x82));
            _colors[ColoringID.WindowBackground].Push(new Color32(0x40, 0x40, 0x40));
            _colors[ColoringID.WindowBorder].Push(new Color32(0x70, 0x70, 0x70));

            _colors[ColoringID.ButtonNormal].Push(new Color32(0x2F, 0x2F, 0x2F));
            _colors[ColoringID.ButtonHovering].Push(new Color32(0x35, 0x35, 0x35));
            _colors[ColoringID.ButtonPressed].Push(new Color32(0x2A, 0x2A, 0x2A));

            _colors[ColoringID.TextColor].Push(Color32.White);

            _colors[ColoringID.InputFieldNormal].Push(new Color32(0x26, 0x26, 0x26));

            _colors[ColoringID.ToolbarButtonBackground].Push(_colors[ColoringID.ButtonNormal].Peek());
        }

        public static Color32 Read(ColoringID id) {
            if (_colors.TryGetValue(id, out var stack)) {
                return stack.Peek();
            }

            return default;
        }

        public static void Push(ColoringID id, Color32 value) {
            if (_colors.TryGetValue(id, out var stack)) {
                stack.Push(value);
            }
        }

        public static Color32 Pop(ColoringID id) {
            if (_colors.TryGetValue(id, out var stack)) {
                if (stack.Count == 1) return default;

                return stack.Pop();
            }

            return default;
        }

        internal static void ClearAll() {
            foreach ((ColoringID _, Stack<Color32> stack) in _colors) {
                while (stack.Count > 1) stack.Pop();
            }
        }

        public struct Laziness : IDisposable {
            private readonly ColoringID id;

            internal Laziness(ColoringID id) {
                this.id = id;
            }

            public void Dispose() {
                Pop(id);
            }
        }
        public static Laziness Lazy(ColoringID id, in Color32 value) {
            Push(id, value);
            return new Laziness(id);
        }
    }
}
