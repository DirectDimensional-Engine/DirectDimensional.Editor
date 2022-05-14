using System.Numerics;
using DirectDimensional.Editor.GUI;
using DirectDimensional.Core;
using System.Runtime.InteropServices;
using System.Text;
using DirectDimensional.Core.Utilities;

using static StbTrueTypeSharp.StbTrueType;

namespace DirectDimensional.Editor {
    internal unsafe static class EditorLifecycle {
        private static HorizontalTextAnchor hanchor = HorizontalTextAnchor.Left;
        private static VerticalTextAnchor vanchor = VerticalTextAnchor.Top;

        public static void Initialize() {
            ImGuiEngine.Initialize();

            KeyboardAxisRegister.Register(0, KeyboardCode.Left, KeyboardCode.Right);
            KeyboardAxisRegister.Register(1, KeyboardCode.Down, KeyboardCode.Up);
        }

        private static int repeat = 0;
        private static float height = 300;

        public static void Cycle() {
            ImGuiEngine.NewFrame();
            ImGuiEngine.Update();

            // Area to put ImGui codes

            ImGuiEngine.Render();
            ImGuiEngine.EndFrame();
        }

        public static void Shutdown() {
            ImGuiEngine.Shutdown();
        }
    }
}
