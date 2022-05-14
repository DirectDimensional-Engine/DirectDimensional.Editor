using System.Numerics;
using DirectDimensional.Editor.GUI;
using DirectDimensional.Core;
using System.Runtime.InteropServices;
using System.Text;
using DirectDimensional.Core.Utilities;

using static StbTrueTypeSharp.StbTrueType;

namespace DirectDimensional.Editor {
    internal unsafe static class EditorLifecycle {
        public static void Initialize() {
            ImGuiEngine.Initialize();
        }

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
