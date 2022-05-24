using DirectDimensional.Editor.GUI;
using DirectDimensional.Core;
using DirectDimensional.Core.Miscs;
using DirectDimensional.Core.Utilities;
using System.Numerics;
using System.Diagnostics;

namespace DirectDimensional.Editor {
    internal unsafe static class EditorLifecycle {
        public static void Initialize() {
            Engine.Initialize();
        }

        public static void Cycle() {
            Engine.NewFrame();
            Engine.Update();

            Engine.Render();
            Engine.EndFrame();
        }

        public static void Shutdown() {
            Engine.Shutdown();
        }
    }
}
