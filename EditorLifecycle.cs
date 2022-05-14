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

            hanchor = (HorizontalTextAnchor)DDMath.Wrap((int)hanchor + KeyboardAxisRegister.Pressed(0), 3);
            vanchor = (VerticalTextAnchor)DDMath.Wrap((int)vanchor - KeyboardAxisRegister.Pressed(1), 3);

            ImGui.BeginStandardWindow("A Window");
            var dr = ImGui.CurrentWindow.DisplayRect;

            var drawRect = new Rect(0, 0, dr.Width, height);
            ImGuiRender.DrawRect(drawRect, Color32.Red.WithAlpha(40));

            int vcount = ImGuiLowLevel.VertexCount;
            int icount = ImGuiLowLevel.IndexCount;
            {
                var sb = new StringBuilder(repeat * 8);
                for (int i = 0; i < repeat; i++) {
                    sb.Append("Text line number ").AppendFormat("{0,2:D2}", i).AppendLine();
                }

                Widgets.TextWrapped2(drawRect, sb.ToString(), hanchor, vanchor);
            }
            int vcount2 = ImGuiLowLevel.VertexCount - vcount;
            int icount2 = ImGuiLowLevel.IndexCount - icount;

            Widgets.Text(new Rect(0, 410, dr.Width, 18), vcount2 + "/" + icount2);

            height = Widgets.Slider(new Rect(0, 430, dr.Width, 18), "HEIGHT", height, 0, 400);
            repeat = Widgets.Slider(new Rect(0, 450, dr.Width, 18), "REPEAT", repeat, 0, 50);

            ImGui.EndStandardWindow();

            ImGuiEngine.Render();
            ImGuiEngine.EndFrame();
        }

        public static void Shutdown() {
            ImGuiEngine.Shutdown();
        }
    }
}
