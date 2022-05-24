using System;

namespace DirectDimensional.Editor.GUI {
#pragma warning disable CA1069
    [Flags]
    public enum ResizingBorderDirections {
        None = 0,

        Left = 0x0001,
        Right = 0x0002,
        Top = 0x0004,
        Bottom = 0x0008,

        TopLeft = 0x0010,
        TopRight = 0x0020,
        BottomLeft = 0x0040,
        BottomRight = 0x0080,

        HorizontalVertical = Left | Right | Top | Bottom,
        Diagonal = TopLeft | TopRight | BottomLeft | BottomRight,

        All = -1,
    }

    public enum TextOverflowMethod {
        CharacterWise, WordWise,
    }

    [Flags]
    public enum WindowFlags {
        None = 0,

        DisableTitlebar = 1 << 0,
        DisableResize = 1 << 1,
        PreventFocus = 1 << 2,

        DisableBackground = 1 << 3,

        DisableVScrollbar = 1 << 4,
        DisableHScrollbar = 1 << 5,
        DisableScrollbars = DisableVScrollbar | DisableHScrollbar,

        AutoContentFitX = 1 << 6,
        AutoContentFitY = 1 << 7,

        AutoContentFit = AutoContentFitX | AutoContentFitY,

        DisableBorder = 1 << 8,

        Tooltip = DisableTitlebar | DisableResize | PreventFocus | AutoContentFit | DisableScrollbars,
        Popup = DisableTitlebar | DisableResize | AutoContentFit | DisableScrollbars,

        Windowless = DisableTitlebar | DisableResize | DisableBackground | DisableScrollbars,
    }

    [Flags]
    public enum ButtonFlags {
        None = 0,

        NoLeftMouse = 1 << 0,
        AllowRightMouse = 1 << 1,
        AllowMiddleMouse = 1 << 2,

        AllMouse = AllowRightMouse | AllowMiddleMouse,

        NoTexture = 1 << 4,

        DetectHeld = 1 << 3,
    }

    [Flags]
    public enum SliderFlags {
        None = 0,
    }

    [Flags]
    public enum DragAreaFlags {
        None = 0,

        NoLeftMouse = 1 << 0,
        AllowRightMouse = 1 << 1,
        AllowMiddleMouse = 1 << 2,

        AllMouse = AllowRightMouse | AllowMiddleMouse,

        //WrapCursorInWnd = 1 << 3,
    }

    [Flags]
    public enum TooltipFlags {
        None = 0,

        Delay = 1 << 0,
    }

    public enum GradientDirection {
        Horizontal = 0,

        [Obsolete("Work in progress")]
        Vertical = 1,
    }

    [Flags]
    public enum KeyboardInputWidgetFlags {
        None = 0,
    }

    public enum HorizontalTextAnchor {
        Left, Middle, Right,
    }

    public enum VerticalTextAnchor {
        Top, Middle, Bottom,
    }
#pragma warning restore CA1069
}
