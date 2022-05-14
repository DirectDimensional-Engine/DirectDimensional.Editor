using System;

namespace DirectDimensional.Editor.GUI {
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

    public enum WindowType {
        Undefined = -1, Standard = 0, DropdownMenu, Tooltip,
    }

    [Flags]
    public enum StandardWindowFlags {
        None = 0,

        DisableTitlebar = 1 << 0,
        DisableResize = 1 << 1,
        PreventFocus = 1 << 2,

        DisableBackground = 1 << 3,
        DisableScrollbar = 1 << 4,

        //TooltipWindow = RemoveTitlebar | PreventResize | PreventFocus,

        WindowlessGUI = DisableTitlebar | DisableResize | DisableBackground | DisableScrollbar,
    }

    [Flags]
    public enum ButtonFlags {
        None = 0,

        NoLeftMouse = 1 << 0,
        AllowRightMouse = 1 << 1,
        AllowMiddleMouse = 1 << 2,

        AllMouse = AllowRightMouse | AllowMiddleMouse,

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
}
