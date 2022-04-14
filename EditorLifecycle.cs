using System.Numerics;
using DirectDimensional.Core;
using DirectDimensional.Runtime;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.DXGI;
using DirectDimensional.Bindings.Direct3D11;
using DirectDimensional.Core.Utilities;
using DirectDimensional.Editor.GUI;

using D3D11Buffer = DirectDimensional.Bindings.Direct3D11.Buffer;
using UIVertex = DirectDimensional.Editor.GUI.Vertex;

using DDVertexShader = DirectDimensional.Core.VertexShader;
using DDPixelShader = DirectDimensional.Core.PixelShader;

using DDTexture2D = DirectDimensional.Core.Texture2D;
using DirectDimensional.Core.Miscs;

namespace DirectDimensional.Editor {
    internal unsafe static class EditorLifecycle {
        public static void Initialize() {
            EditorContext.Initialize();
            ImGuiEngine.Initialize();
        }

        private static Vector2 windowPosition = default;
        private static Vector2 clientSize = new(300, 200);

        public static void Cycle() {
            Identifier.ValidateHashToPrepareFrame();
            ImGuiEngine.NewFrame();

            ImGui.BeginWindow("Window", ref windowPosition, ref clientSize, 25);

            for (int i = 0; i < 15; i++) {
                ImGui.DrawRect(new Rect(new Vector2(windowPosition.X + 10 + i * 25, windowPosition.Y + 25 + 15), new Vector2(20, 20)), Color32.Green);
            }

            ImGui.EndWindow();

            ImGuiEngine.Render();
        }

        public static void CleanUp() {
            ImGuiEngine.Shutdown();
        }
    }
}
