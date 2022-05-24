using System;
using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public static class FontStack {
        private static readonly Stack<Font> _fontStack;

        public static Font Current => _fontStack.Peek();

        static FontStack() {
            _fontStack = new();

            Font font = new(Path.Combine(Editor.ApplicationDirectory, "Resources", "Roboto-Light.ttf"), 512, 256, 14, 32..127);
            font.Name = "Roboto-Light (ImGui Default)";

            Push(font);
        }

        public static void Push(Font font) {
            _fontStack.Push(font);
        }

        public static void Pop() {
            if (_fontStack.Count <= 1) return;

            _fontStack.Pop();
        }

        internal static void ClearStack() {
            while (_fontStack.Count > 1) _fontStack.Pop();
        }

        public readonly struct Laziness : IDisposable {
            private readonly bool Condition { get; init; }

            public Laziness(bool condition) {
                Condition = condition;
            }

            public void Dispose() {
                if (Condition) Pop();
            }
        }
        public static Laziness Lazy(Font font, bool condition) {
            if (condition) Push(font);
            return new(condition);
        }
        public static Laziness Lazy(Font font) {
            Push(font);
            return new(true);
        }
    }
}
