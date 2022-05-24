using DirectDimensional.Core;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Numerics;

namespace DirectDimensional.Editor.GUI {
    public enum StylingID {
        /// <summary>
        /// Define whether to create a scissor rect to cutoff text mesh outside area. Use <seealso cref="bool"/> value.
        /// </summary>
        TextMasking,

        /// <summary>
        /// The size of window content padding. Use <seealso cref="Vector4"/> value. XYZW correspond to Left, Right, Top, Bottom.
        /// </summary>
        WindowContentPadding,

        /// <summary>
        /// The value to move the cursor of Drawing Context between elements. Use <seealso cref="int"/> value.
        /// </summary>
        LayoutElementSpacing,

        /// <summary>
        /// Fallback value if Font cannot found character. Use <seealso cref="char"/> value.
        /// </summary>
        TextCharacterFallback,
    }

    public static unsafe class Styling {
        private static readonly Dictionary<StylingID, Stack<IntPtr>> _dicts;

        static Styling() {
            var values = Enum.GetValues<StylingID>();

            _dicts = new(values.Length);
            for (int i = 0; i < values.Length; i++) {
                _dicts[values[i]] = new Stack<IntPtr>(6);
            }

            Push(StylingID.TextMasking, true);
            Push(StylingID.WindowContentPadding, new Vector4(3, 3, 1, 1));
            Push(StylingID.LayoutElementSpacing, 3);
            Push(StylingID.TextCharacterFallback, '?');
        }

        public static void Push<T>(StylingID id, in T value) where T : unmanaged {
            if (_dicts.TryGetValue(id, out var stack)) {
                var ptr = Marshal.AllocHGlobal(sizeof(T));

                fixed (T* pValue = &value) {
                    Unsafe.CopyBlock(ptr.ToPointer(), pValue, (uint)sizeof(T));
                }

                stack.Push(ptr);
            }
        }

        public static bool Pop(StylingID id) {
            if (_dicts.TryGetValue(id, out var stack)) {
                if (stack.Count <= 1) return false;

                Marshal.FreeHGlobal(stack.Pop());
                return true;
            }

            return false;
        }

        public static T Read<T>(StylingID id) where T : unmanaged {
            if (_dicts.TryGetValue(id, out var stack)) {
                return *(T*)stack.Peek().ToPointer();
            }

            return default;
        }

        internal static void ClearAll() {
            foreach ((StylingID _, Stack<IntPtr> stack) in _dicts) {
                while (stack.Count > 1) {
                    Marshal.FreeHGlobal(stack.Pop());
                }
            }
        }

        public readonly struct Laziness : IDisposable {
            private readonly StylingID id;

            public Laziness(StylingID id) {
                this.id = id;
            }

            public void Dispose() {
                Pop(id);
            }
        }
        public static Laziness Lazy<T>(StylingID id, in T value) where T : unmanaged {
            Push(id, value);
            return new(id);
        }
    }
}