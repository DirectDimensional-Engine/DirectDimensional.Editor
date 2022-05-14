using System;
using System.Numerics;
using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public static class ImGuiInput {
        private static float _doubleClickThreshold = 0.3f;

        public static float DoubleClickThreshold {
            get => _doubleClickThreshold;
            set => _doubleClickThreshold = Math.Clamp(value, 0.1f, 0.8f);
        }

        /// <summary>
        /// Mouse position with coordinate offset caused by <seealso cref="ImGuiLowLevel.BeginCoordinateOffset(Vector2)"/>
        /// </summary>
        public static Vector2 MousePosition => Mouse.Position - ImGuiLowLevel.CurrentCoordinateOffset;

        private static bool _dbclkL, _dbclkR, _dbclkM;
        public static bool LeftDoubleClick => _dbclkL;
        public static bool RightDoubleClick => _dbclkR;
        public static bool MiddleDoubleClick => _dbclkM;

        private static float _dbclkLTime, _dbclkRTime, _dbclkMTime;

        internal static void Update() {
            CheckDoubleClickRoutine(Mouse.LeftPressed, ref _dbclkL, ref _dbclkLTime);
            CheckDoubleClickRoutine(Mouse.RightPressed, ref _dbclkR, ref _dbclkRTime);
            CheckDoubleClickRoutine(Mouse.MiddlePressed, ref _dbclkM, ref _dbclkMTime);
        }

        private static void CheckDoubleClickRoutine(bool pressedCondition, ref bool doubleClick, ref float clickTime) {
            if (pressedCondition && !doubleClick) {
                doubleClick = (float)EditorApplication.ElapsedTime - clickTime <= _doubleClickThreshold;
                clickTime = (float)EditorApplication.ElapsedTime;
            } else if (doubleClick) {
                doubleClick = false;
                clickTime = 0;
            }
        }
    }
}
