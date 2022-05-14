using System.Numerics;

namespace DirectDimensional.Editor.GUI {
    public enum StylingID {
        /// <summary>
        /// Define whether to create a scissor rect to cutoff text mesh outside area. Use <seealso cref="bool"/> value.
        /// </summary>
        TextMasking,

        /// <summary>
        /// Slider radius. Use <seealso cref="int"/> value.
        /// </summary>
        SliderHandleRadius,

        /// <summary>
        /// The size of the rectangle hole of slider. Use <seealso cref="int"/> value.
        /// </summary>
        SliderHoleSize,

        /// <summary>
        /// The size of window content padding. Use <seealso cref="Vector4"/> value. XYZW correspond to Left, Right, Top, Bottom.
        /// </summary>
        WindowContentPadding,
    }
}
