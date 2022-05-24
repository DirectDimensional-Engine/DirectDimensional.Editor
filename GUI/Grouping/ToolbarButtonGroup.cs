using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI.Grouping {
    public enum ToolbarBtnGroupFlags {
        None = 0,

        NoBackground = 1 << 0,
    }

    public sealed class ToolbarButtonGroup : ImGuiGroup {
        public static ToolbarButtonGroup? Current { get; private set; }

        private readonly float _areaHeight;
        private float _btnRelX = 0;

        public ToolbarButtonGroup(ReadOnlySpan<char> id, Rect rect, int buttonCapacity = 8, ToolbarBtnGroupFlags flags = default) {
            if (Current != null) {
                Logger.Log("Cannot create a new instance of '" + nameof(ToolbarButtonGroup) + "' as the last group instance hasn't been disposed yet");
                return;
            }

            Identifier.Push(id);
            Current = this;
            _areaHeight = rect.Height;

            LowLevel.BeginRectGroupRelative(rect);

            if ((flags & ToolbarBtnGroupFlags.NoBackground) != ToolbarBtnGroupFlags.NoBackground) {
                Drawings.DrawRect(new Rect(0, 0, rect.Width, rect.Height), Coloring.Read(ColoringID.ToolbarButtonBackground));
            }
        }

        public bool Button(ReadOnlySpan<char> label, float horizontalExtrude = 15) {
            if (!InstanceValid) return false;

            ImGui.DecodeWidgetIdentity(label, out var text, out _);
            float width = FontStack.Current.CalcStringWidth(text);

            bool pressed = Widgets.Button(new Rect(_btnRelX, 0, width + horizontalExtrude, _areaHeight), label);
            _btnRelX += width + horizontalExtrude;

            return pressed;
        }

        public override void Dispose() {
            if (!InstanceValid) return;

            LowLevel.EndRectGroup();

            Current = null;
            Identifier.Pop();
        }

        private bool InstanceValid => ReferenceEquals(Current, this);
    }
}
