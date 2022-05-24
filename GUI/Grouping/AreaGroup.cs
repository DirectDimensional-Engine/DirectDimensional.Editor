using System;
using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI.Grouping {
    public sealed class AreaGroup : ImGuiGroup {
        public AreaGroup(Rect rect, bool relative = true, bool clipLast = true) {
            if (relative) {
                LowLevel.BeginRectGroupRelative(rect, clipLast);
            } else {
                LowLevel.BeginRectGroupAbsolute(rect, clipLast);
            }
        }

        public override void Dispose() {
            LowLevel.EndRectGroup();
        }
    }
}
