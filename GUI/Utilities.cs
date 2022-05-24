using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using DirectDimensional.Core;
using DirectDimensional.Core.Utilities;

namespace DirectDimensional.Editor.GUI {
    public static class Utilities {
        /// <summary>
        /// Check whether the given point is inside DirectX's scissor rect
        /// </summary>
        /// <param name="point">Point relative to application window position.</param>
        public static bool IntersectScissorRect(Vector2 point) {
            if (LowLevel.ScissorRectCount == 0) return true;

            var rect = LowLevel.CurrentScissorRect;

            var x = (int)point.X;
            var y = (int)point.Y;

            return rect.Left <= x && x < rect.Right && rect.Top <= y && y < rect.Bottom;
        }

        /// <summary>
        /// Check whether the given rectangle is intersect with DirectX's scissor rect
        /// </summary>
        /// <param name="rect">Rect with position relative to application window position</param>
        /// <returns></returns>
        public static bool IntersectScissorRect(Rect rect) {
            if (LowLevel.ScissorRectCount == 0) return true;

            var sr = LowLevel.CurrentScissorRect;
            var max = rect.Max;

            return max.X >= sr.Left && rect.Position.X <= sr.Right && max.Y >= sr.Top && rect.Position.Y <= sr.Bottom;
        }

        public static bool IntersectScissorRect(float minX, float minY, float maxX, float maxY) {
            if (LowLevel.ScissorRectCount == 0) return true;

            var sr = LowLevel.CurrentScissorRect;
            return maxX >= sr.Left && minX <= sr.Right && maxY >= sr.Top && minY <= sr.Bottom;
        }

        /// <summary>
        /// Check whether the given point is inside DirectX's scissor rect
        /// </summary>
        /// <param name="point">Point relative to application window position.</param>
        public static bool LocalIntersectScissorRect(Vector2 point) {
            if (LowLevel.ScissorRectCount == 0) return true;

            var rect = LowLevel.CurrentScissorRect;

            point += LowLevel.CurrentCoordinateOffset;

            var x = (int)point.X;
            var y = (int)point.Y;

            return rect.Left <= x && x < rect.Right && rect.Top <= y && y < rect.Bottom;
        }

        /// <summary>
        /// The same as <seealso cref="IntersectScissorRect(Rect)"/>, but with Coordinate offset applied to Rect's position
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static bool LocalIntersectScissorRect(Rect rect) {
            if (LowLevel.ScissorRectCount == 0) return true;

            var sr = LowLevel.CurrentScissorRect;

            var min = rect.Position + LowLevel.CurrentCoordinateOffset;
            var max = rect.Max + LowLevel.CurrentCoordinateOffset;

            return max.X >= sr.Left && min.X <= sr.Right && max.Y >= sr.Top && min.Y <= sr.Bottom;
        }

        public static bool LocalIntersectScissorRect(float minX, float minY, float maxX, float maxY) {
            if (LowLevel.ScissorRectCount == 0) return true;

            var sr = LowLevel.CurrentScissorRect;

            return maxX >= sr.Left && minX <= sr.Right && maxY >= sr.Top && minY <= sr.Bottom;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector2 LocalToAbsolute(Vector2 point) {
            return point + LowLevel.CurrentCoordinateOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Vector2 AbsoluteToLocal(Vector2 point) {
            return point - LowLevel.CurrentCoordinateOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Rect LocalToAbsolute(Rect rect) {
            return new(rect.Position + LowLevel.CurrentCoordinateOffset, rect.Size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Rect AbsoluteToLocal(Rect rect) {
            return new(rect.Position - LowLevel.CurrentCoordinateOffset, rect.Size);
        }
    }
}
