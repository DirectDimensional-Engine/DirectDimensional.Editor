using DirectDimensional.Core;
using DirectDimensional.Core.Utilities;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;

namespace DirectDimensional.Editor.GUI {
    /// <summary>
    /// Contains raw behaviour of ImGui widget. Will not consider control case of Overlapping, etc...
    /// </summary>
    public static class ImGuiBehaviour {
        /// <summary>
        /// Simulating button pressing action
        /// </summary>
        /// <param name="id">ID of the button, must be unique in the same ID group</param>
        /// <param name="rect">Rectangle of the button. <c>Position</c> will be applied coordinate offset</param>
        /// <param name="flags">Configuration flags of button behaviour</param>
        /// <param name="hover">Whether the button is being hovered</param>
        /// <returns>Whether the button is being pressed</returns>
        public static bool Button(string id, Rect rect, ButtonFlags flags, out bool hover) {
            hover = false;
            if (ImGui.CurrentWindow == null) return false;

            bool pressed = false;

            Identifier.Push(id);
            {
                hover = ImGui.CurrentWindowHovered && Identifier.HoveringID == 0 && rect.Collide(ImGuiInput.MousePosition);

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
            Identifier.Pop();

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
        public static bool CircularButton(string id, Circle circle, ButtonFlags flags, out bool hover) {
            hover = false;
            if (ImGui.CurrentWindow == null) return false;

            bool pressed = false;

            Identifier.Push(id);
            {
                bool hovered = ImGui.CurrentWindowHovered && Identifier.HoveringID == 0 && circle.CollideSqr(ImGuiInput.MousePosition);

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
            Identifier.Pop();

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
        public static bool DragArea(string id, Rect rect, DragAreaFlags flags, out bool hover, out Vector2 drag) {
            hover = false;
            drag = default;
            if (ImGui.CurrentWindow == null) return false;

            bool isDragging = false;

            Identifier.Push(id);
            {
                hover = ImGui.CurrentWindowHovered && Identifier.HoveringID == 0 && rect.Collide(ImGuiInput.MousePosition);

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
            Identifier.Pop();

            return isDragging;
        }

        public static bool VerticalScrollbar(string id, Rect scrollbarRect, float viewportHeight, float contentHeight, ref float scrollingY, out Rect handleRect) {
            handleRect = default;
            if (ImGui.CurrentWindow == null) return false;

            bool scrolling = false;

            var handleSizeNormalize = DDMath.Saturate(viewportHeight / contentHeight);
            var handleSizeHeight = scrollbarRect.Height * handleSizeNormalize;

            if (Button(id, scrollbarRect, ButtonFlags.DetectHeld, out _)) {
                // Stolen directly from Dear ImGui repo, thank you Ocornut, very cool
                scrollingY = DDMath.Saturate((Mouse.Position.Y - (scrollbarRect.Position.Y + handleSizeHeight * 0.5f)) / (scrollbarRect.Height - handleSizeHeight)) * (1 - handleSizeNormalize) * contentHeight;
                scrolling = true;
            } else {
                scrollingY = Math.Min(scrollingY, contentHeight - viewportHeight);
            }

            float handleYnorm = DDMath.Saturate(scrollingY / contentHeight);
            var max = scrollbarRect.Max;
            handleRect = new Rect(scrollbarRect.Position.X, DDMath.LerpUnclamped(scrollbarRect.Position.Y, max.Y, handleYnorm), StandardGuiWindow.HorizontalScrollbarWidth, handleSizeHeight);

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
