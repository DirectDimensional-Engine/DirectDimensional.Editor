using System;
using DirectDimensional.Bindings.WinAPI;

namespace DirectDimensional.Editor {
    internal static class EditorCursor {
        private static StandardCursorID _cursor;

        public static StandardCursorID Cursor {
            get => _cursor;
            set {
                if (value == _cursor) return;

                WinAPI.SetCursor(WinAPI.LoadCursorW(IntPtr.Zero, value));
                _cursor = value;
            }
        }
    }
}
