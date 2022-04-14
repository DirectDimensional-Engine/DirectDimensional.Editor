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
}
