using System;
using System.Numerics;
using DirectDimensional.Bindings.WinAPI;

namespace DirectDimensional.Editor {
    public static class EditorWindow {
        internal static IntPtr WindowHandle { get; set; }

        public static Vector2 ClientSize {
            get {
                WinAPI.GetClientRect(WindowHandle, out var rect);

                return new(rect.Right, rect.Bottom);
            }
        }
    }
}
