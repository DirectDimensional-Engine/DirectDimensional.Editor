using DirectDimensional.Core;
using DirectDimensional.Core.Utilities;
using System.Numerics;

namespace DirectDimensional.Editor.GUI {
    internal static class Calculations {
        public static void CalculateVerticalScrollbarInformations(Rect scrollbarRect, float value, Vector2 range, out Rect handleRect) {
            float normalize = DDMath.Saturate(DDMath.InverseLerp(value, range.X, range.Y));
            float handleHeight = scrollbarRect.Height * normalize;

            float startY = DDMath.Remap(value, range.X, range.Y, scrollbarRect.Y, scrollbarRect.MaxY - handleHeight);
            handleRect = new Rect(scrollbarRect.X, startY, scrollbarRect.Width, handleHeight);
        }
    }
}
