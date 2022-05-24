using DirectDimensional.Core;
using DirectDimensional.Core.Utilities;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;

namespace DirectDimensional.Editor.GUI {
    /// <summary>
    /// Contains raw behaviour of ImGui widget. Will not consider control case of Overlapping, etc...
    /// </summary>
    public static class Behaviours {
        /// <summary>
        /// Simulating button pressing action
        /// </summary>
        /// <param name="id">ID of the button, must be unique in the same ID group</param>
        /// <param name="rect">Rectangle of the button. <c>Position</c> will be applied coordinate offset</param>
        /// <param name="flags">Configuration flags of button behaviour</param>
        /// <param name="hover">Whether the button is being hovered</param>
        /// <returns>Whether the button is being pressed</returns>
        public static bool Button(ReadOnlySpan<char> id, Rect rect, ButtonFlags flags, out bool hover) {
            hover = false;
            if (ImGui.CurrentWindow == null) return false;

            bool pressed = false;

            using (Identifier.Lazy(id)) {
                hover = ImGui.CurrentWindowHovered && Identifier.HoveringID == 0 && rect.Collide(Input.MousePosition) && Utilities.IntersectScissorRect(Mouse.Position);

                if (hover) {
                    Identifier.SetHoveringID();

                    if (((flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftPressed) || ((flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightPressed) || ((flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddlePressed)) {
                        Identifier.SetActiveID();
                    }
                } else {
                    Identifier.ClearCurrentHoveringID();
                }

                if (Identifier.Activating) {
                    pressed = true;

                    if (((flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftReleased) || ((flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightReleased) || ((flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddleReleased)) {
                        Identifier.ClearActiveID();
                        pressed = false;
                    }
                }

                if (pressed) {
                    if ((flags & ButtonFlags.DetectHeld) != ButtonFlags.DetectHeld) {
                        var c = ((flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftPressed) || ((flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightPressed) || ((flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddlePressed);
                        if (!c) {
                            pressed = false;
                        }
                    }
                }
            }

            return pressed;
        }

        /// <summary>
        /// Same as <c>Button</c>, but use circle instead of a rect.
        /// </summary>
        /// <param name="id">ID of the button, must be unique in the same ID group</param>
        /// <param name="circle">Circle of the button. <c>Center</c> will be applied coordinate offset</param>
        /// <param name="flags">Configuration flags of button behaviour. Reserved, use <c>default</c> keyword for now.</param>
        /// <param name="hover">Whether the button is being hovered</param>
        /// <returns>Whether the button is being pressed</returns>
        public static bool CircularButton(ReadOnlySpan<char> id, Circle circle, ButtonFlags flags, out bool hover) {
            hover = false;
            if (ImGui.CurrentWindow == null) return false;

            bool pressed = false;

            using (Identifier.Lazy(id)) {
                bool hovered = ImGui.CurrentWindowHovered && Identifier.HoveringID == 0 && circle.CollideSqr(Input.MousePosition) && Utilities.IntersectScissorRect(Mouse.Position);

                if (hovered) {
                    Identifier.SetHoveringID();

                    if (((flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftPressed) || ((flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightPressed) || ((flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddlePressed)) {
                        Identifier.SetActiveID();
                    }
                } else {
                    Identifier.ClearCurrentHoveringID();
                }

                if (Identifier.Activating) {
                    pressed = true;

                    if (((flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftReleased) || ((flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightReleased) || ((flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddleReleased)) {
                        Identifier.ClearActiveID();
                        pressed = false;
                    }
                }

                if (pressed) {
                    if ((flags & ButtonFlags.DetectHeld) != ButtonFlags.DetectHeld) {
                        var c = ((flags & ButtonFlags.NoLeftMouse) != ButtonFlags.NoLeftMouse && Mouse.LeftPressed) || ((flags & ButtonFlags.AllowRightMouse) == ButtonFlags.AllowRightMouse && Mouse.RightPressed) || ((flags & ButtonFlags.AllowMiddleMouse) == ButtonFlags.AllowMiddleMouse && Mouse.MiddlePressed);
                        if (!c) {
                            pressed = false;
                        }
                    }
                }

                hover = hovered;
            }

            return pressed;
        }

        /// <summary>
        /// Simulating Draging area
        /// </summary>
        /// <param name="id">ID of the drag area, must be unique in the same ID group</param>
        /// <param name="rect">Dragging area. <c>Position</c> will be applied coordinate offset</param>
        /// <param name="flags">Configuration flags</param>
        /// <param name="hover">Whether the dragging area is being hovered</param>
        /// <param name="drag">Drag delta of the mouse since last application cycle</param>
        /// <returns></returns>
        public static bool DragArea(ReadOnlySpan<char> id, Rect rect, DragAreaFlags flags, out bool hover, out Vector2 drag) {
            hover = false;
            drag = default;
            if (ImGui.CurrentWindow == null) return false;

            bool isDragging = false;

            using (Identifier.Lazy(id)) {
                hover = ImGui.CurrentWindowHovered && Identifier.HoveringID == 0 && rect.Collide(Input.MousePosition) && Utilities.IntersectScissorRect(Mouse.Position);

                if (hover) {
                    Identifier.SetHoveringID();

                    if (((flags & DragAreaFlags.NoLeftMouse) != DragAreaFlags.NoLeftMouse && Mouse.LeftPressed) || ((flags & DragAreaFlags.AllowRightMouse) == DragAreaFlags.AllowRightMouse && Mouse.RightPressed) || ((flags & DragAreaFlags.AllowMiddleMouse) == DragAreaFlags.AllowMiddleMouse && Mouse.MiddlePressed)) {
                        Identifier.SetActiveID();
                    }
                } else {
                    Identifier.ClearCurrentHoveringID();
                }

                if (Identifier.Activating) {
                    //if ((flags & DragAreaFlags.WrapCursorInWnd) == DragAreaFlags.WrapCursorInWnd) 
                    //    NotifyWrapCursorInWindow();

                    isDragging = true;

                    if (((flags & DragAreaFlags.NoLeftMouse) != DragAreaFlags.NoLeftMouse && Mouse.LeftReleased) || ((flags & DragAreaFlags.AllowRightMouse) == DragAreaFlags.AllowRightMouse && Mouse.RightReleased) || ((flags & DragAreaFlags.AllowMiddleMouse) == DragAreaFlags.AllowMiddleMouse && Mouse.MiddleReleased)) {
                        Identifier.ClearActiveID();
                        isDragging = false;
                    }
                }

                if (isDragging) {
                    drag = Mouse.Move;
                }
            }

            return isDragging;
        }

        public static bool HorizontalScrollbar(ReadOnlySpan<char> id, Rect scrollbarRect, float viewportWidth, float contentWidth, ref float scrollingX, out Rect handleRect) {
            handleRect = default;
            if (ImGui.CurrentWindow == null) return false;

            bool scrolling = false;

            var handleSizeNormalize = DDMath.Saturate(viewportWidth / contentWidth);
            var handleSizeWidth = scrollbarRect.Width * handleSizeNormalize;

            if (Button(id, scrollbarRect, ButtonFlags.DetectHeld, out _)) {
                scrollingX = DDMath.Saturate((Mouse.Position.X - (scrollbarRect.Position.X + handleSizeWidth * 0.5f)) / (scrollbarRect.Width - handleSizeWidth)) * (1 - handleSizeNormalize) * contentWidth;
                scrolling = true;
            } else {
                scrollingX = Math.Min(scrollingX, contentWidth - viewportWidth);
            }

            var max = scrollbarRect.Max;
            handleRect = new Rect(DDMath.LerpUnclamped(scrollbarRect.Position.X, max.X, DDMath.Saturate(scrollingX / contentWidth)), scrollbarRect.Position.Y, handleSizeWidth, scrollbarRect.Height);

            return scrolling;
        }

        public static bool VerticalScrollbar(ReadOnlySpan<char> id, Rect scrollbarRect, float viewportWidth, float contentWidth, ref float scrollingY, out Rect handleRect) {
            handleRect = default;
            if (ImGui.CurrentWindow == null) return false;

            bool scrolling = false;

            var handleSizeNormalize = DDMath.Saturate(viewportWidth / contentWidth);
            var handleSizeHeight = scrollbarRect.Height * handleSizeNormalize;

            if (Button(id, scrollbarRect, ButtonFlags.DetectHeld, out _)) {
                scrollingY = DDMath.Saturate((Mouse.Position.Y - (scrollbarRect.Position.Y + handleSizeHeight * 0.5f)) / (scrollbarRect.Height - handleSizeHeight)) * (1 - handleSizeNormalize) * contentWidth;
                scrolling = true;
            } else {
                scrollingY = Math.Min(scrollingY, contentWidth - viewportWidth);
            }

            var max = scrollbarRect.Max;
            handleRect = new Rect(scrollbarRect.Position.X, DDMath.LerpUnclamped(scrollbarRect.Position.Y, max.Y, DDMath.Saturate(scrollingY / contentWidth)), scrollbarRect.Width, handleSizeHeight);

            return scrolling;
        }

        internal static bool WrapCursorInWnd = false;

        /// <summary>
        /// Notify the engine to wrap the cursor position around the editor window. Automatically disable every engine frame
        /// </summary>
        public static void NotifyWrapCursorInWindow() {
            if (WrapCursorInWnd == true) return;

            WrapCursorInWnd = true;
        }
        internal static void DeactivateWrapCursorInWindow() {
            if (WrapCursorInWnd == false) return;

            WrapCursorInWnd = false;
        }
    }
}
