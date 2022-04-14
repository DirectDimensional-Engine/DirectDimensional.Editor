using System;
using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public enum ImGuiColoringID {
        WindowTitle,
        WindowBackground,
    }

    public static class ImGuiColoring {
        private static readonly Color32[] _colors;

        static ImGuiColoring() {
            _colors = new Color32[typeof(ImGuiColoringID).GetFields().Length];

            SetColor(ImGuiColoringID.WindowTitle, new Color32(0x00, 0x4B, 0x82));
            SetColor(ImGuiColoringID.WindowBackground, new Color32(0x40, 0x40, 0x40));
        }

        public static Color32 GetColor(ImGuiColoringID id) {
            return _colors[(int)id];
        }

        public static void SetColor(ImGuiColoringID id, Color32 value) {
            _colors[(int)id] = value;
        }
    }
}
