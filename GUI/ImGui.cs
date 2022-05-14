using DirectDimensional.Core;
using DirectDimensional.Bindings.WinAPI;
using DirectDimensional.Core.Utilities;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using DirectDimensional.Bindings.Direct3D11;

using static StbTrueTypeSharp.StbTrueType;

using DDTexture2D = DirectDimensional.Core.Texture2D;

namespace DirectDimensional.Editor.GUI {
    public static unsafe class ImGui {
        private static readonly Stack<GuiWindow> _windowStack;

        private static readonly List<StandardGuiWindow> _standardWnds;
        private static readonly TooltipWindow _tooltipWnd;

        internal static List<StandardGuiWindow> StandardWindows => _standardWnds;
        internal static TooltipWindow TooltipWindow => _tooltipWnd;

        private static GuiWindow? _focusWindow, _hoveringWindow;
        public static GuiWindow? FocusingWindow => _focusWindow;
        public static GuiWindow? CurrentWindow => _windowStack.Count == 0 ? null : _windowStack.Peek();
        public static GuiWindow? HoveringWindow => _hoveringWindow;

        static ImGui() {
            _windowStack = new();
            _standardWnds = new(8);
            _tooltipWnd = new("__TOOLTIP__");
        }

        internal static void NewFrame() {
            if (_windowStack.Count != 0) {
                Logger.Warn("Window stack isn't empty. Make sure you called end window functions with correspond begin window functions.");
            }

            _windowStack.Clear();
        }

        public static void FocusWindow(StandardGuiWindow? window) {
            if (window != null && !window.IsFocusable) return;

            _focusWindow = window;

            if (window != null) {
                _standardWnds.Remove(window);
                _standardWnds.Add(window);
            }
        }

        public static StandardGuiWindow? SearchStandardWindow(string name) {
            for (int i = 0; i < _standardWnds.Count; i++) {
                if (_standardWnds[i].Name == name) {
                    return _standardWnds[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Query focus window from last window to first window in the list
        /// </summary>
        /// <param name="predicate"></param>
        public static void QueryFocusWindow(Func<StandardGuiWindow, bool> predicate) {
            for (int i = _standardWnds.Count - 1; i >= 0; i--) {
                if (!_standardWnds[i].IsFocusable) continue;

                if (predicate(_standardWnds[i])) {
                    FocusWindow(_standardWnds[i]);
                    break;
                }
            }
        }

        internal static void UpdateHoveringWindow() {
            var mp = Mouse.Position;

            for (int i = _standardWnds.Count - 1; i >= 0; i--) {
                var wnd = _standardWnds[i];
                if (!_standardWnds[i].IsFocusable) continue;

                if (mp.X >= wnd.Position.X && mp.Y >= wnd.Position.Y) {
                    if (mp.X < wnd.Position.X + wnd.Size.X && mp.Y < wnd.Position.Y + wnd.Size.Y) {
                        _hoveringWindow = wnd;
                        return;
                    }
                }
            }

            _hoveringWindow = null;
        }

        public static bool CurrentWindowFocused => _focusWindow != null && _focusWindow == CurrentWindow;
        public static bool CurrentWindowHovered => _hoveringWindow != null && CurrentWindow == _hoveringWindow;

        public static float? NextWindowX { get; set; }
        public static float? NextWindowY { get; set; }
        public static float? NextWindowWidth { get; set; }
        public static float? NextWindowHeight { get; set; }

        private static void AssignNextWindowPos(GuiWindow wnd) {
            if (NextWindowX != null) {
                wnd.Position = new(NextWindowX.Value, wnd.Position.Y);
                NextWindowX = null;
            }

            if (NextWindowY != null) {
                wnd.Position = new(wnd.Position.X, NextWindowY.Value);
                NextWindowY = null;
            }
        }
        private static void AssignNextWindowSize(GuiWindow wnd) {
            if (NextWindowWidth != null) {
                wnd.Size = new(NextWindowWidth.Value, wnd.Size.Y);
                NextWindowWidth = null;
            }

            if (NextWindowHeight != null) {
                wnd.Size = new(wnd.Size.X, NextWindowHeight.Value);
                NextWindowHeight = null;
            }
        }

#pragma warning disable CS8774
        [MemberNotNull(nameof(CurrentWindow))]
        public static void BeginStandardWindow(string name, StandardWindowFlags flags = StandardWindowFlags.None) {
            if (_windowStack.Count != 0) {
                Logger.Error("Cannot Begin Standard Window as the window stack isn't empty.");
                return;
            }

            if (SearchStandardWindow(name) is not StandardGuiWindow wnd) {
                wnd = new(name);
                _standardWnds.Add(wnd);
            }

            wnd.Flags = flags;

            AssignNextWindowPos(wnd);
            AssignNextWindowSize(wnd);

            _windowStack.Push(wnd);

            if (wnd.LastRenderingFrame != EditorApplication.FrameCount - 1) {
                FocusWindow(wnd);
            }

            wnd.LastRenderingFrame = EditorApplication.FrameCount;

            Identifier.Push(name);

            bool hasTitlebar = wnd.HasTitlebar;
            bool hasBackground = wnd.RenderBackground;

            if (hasTitlebar && ImGuiBehaviour.Button("__WND_TITLEBAR_DRAGMODE__", wnd.TitlebarRect, ButtonFlags.DetectHeld, out _)) {
                wnd.Position += Mouse.Move;
            }

            if ((flags & StandardWindowFlags.DisableResize) != StandardWindowFlags.DisableResize) {
                if (ImGuiBehaviour.Button("__RESIZE1__", new Rect(wnd.Position + new Vector2(0, wnd.Size.Y - StandardGuiWindow.ResizeHandleSize), StandardGuiWindow.ResizeHandleSize), ButtonFlags.DetectHeld, out _)) {
                    var move = Mouse.Move;

                    wnd.Position += new Vector2(move.X, 0);
                    wnd.Size += new Vector2(-move.X, move.Y);
                }

                if (ImGuiBehaviour.Button("__RESIZE2__", new Rect(wnd.Position + wnd.Size - new Vector2(StandardGuiWindow.ResizeHandleSize), StandardGuiWindow.ResizeHandleSize), ButtonFlags.DetectHeld, out _)) {
                    wnd.Size += Mouse.Move;
                }
            }

            bool hasScrollbar = (flags & StandardWindowFlags.DisableScrollbar) != StandardWindowFlags.DisableScrollbar;
            
            var rdisplay = wnd.DisplayRect;
            if (hasScrollbar || hasTitlebar || hasBackground) {
                wnd.BeginDrawComposite();

                if (hasTitlebar) {
                    ImGuiRender.AddCompositeRect(new Rect(wnd.Position, new Vector2(wnd.Size.X, StandardGuiWindow.TitlebarHeight)), Coloring.Read(ColoringID.WindowTitle));
                    if (hasBackground) ImGuiRender.AddCompositeRect(new(wnd.Position + new Vector2(0, StandardGuiWindow.TitlebarHeight), wnd.Size - new Vector2(0, StandardGuiWindow.TitlebarHeight)), Coloring.Read(ColoringID.WindowBackground));
                } else {
                    if (hasBackground) ImGuiRender.AddCompositeRect(new(wnd.Position, wnd.Size), Coloring.Read(ColoringID.WindowBackground));
                }

                if (hasScrollbar) {
                    var scontent = wnd.Context.ContentSize;
                    var rclient = wnd.ClientRect;

                    wnd.EnableScrollY = scontent.Y > rdisplay.Height;
                    if (wnd.EnableScrollY) {
                        var scrollbarArea = new Rect(rclient.Max.X - StandardGuiWindow.HorizontalScrollbarWidth, rclient.Position.Y, StandardGuiWindow.HorizontalScrollbarWidth, rclient.Size.Y - StandardGuiWindow.ResizeHandleSize);

                        float scrollY = wnd.Scrolling.Y;
                        if (ImGuiBehaviour.VerticalScrollbar("__SCROLLBAR_Y__", scrollbarArea, rdisplay.Height, scontent.Y, ref scrollY, out var handleRect)) {
                            wnd.Scrolling = new(wnd.Scrolling.X, scrollY);
                        }

                        ImGuiRender.AddCompositeRect(scrollbarArea, Color32.Green);
                        ImGuiRender.AddCompositeRect(handleRect, Color32.Blue);
                    } else {
                        wnd.Scrolling = new(wnd.Scrolling.X, 0);
                    }
                }

                wnd.EndDrawComposite();

                if (hasBackground) ImGuiRender.DrawFrame(new Rect(wnd.Position, wnd.Size), Coloring.Read(ColoringID.WindowBorder));
            }

            rdisplay = wnd.DisplayRect;
            ImGuiLowLevel.BeginCoordinateOffset(rdisplay.Position - wnd.Scrolling);
            ImGuiLowLevel.BeginScissorRect(rdisplay);

            wnd.Context.Reset();
        }
#pragma warning restore CS8774

        public static void EndStandardWindow() {
            if (CurrentWindow == null || CurrentWindow.Type != WindowType.Standard) return;

            // Padding to the bottom content
            CurrentWindow.Context.GetRect(4);

            ImGuiLowLevel.EndScissorRect();
            ImGuiLowLevel.EndCoordinateOffset();

            Identifier.Pop();

            _windowStack.Pop();
        }

        /// <summary>
        /// Begin tooltip window when the mouse is hovering over the area. Remember to call <seealso cref="EndTooltipWindow"/> if the method returns true.
        /// </summary>
        /// <param name="hoveringArea">Hovering area relative to the last window</param>
        /// <returns>Whether the tooltip is active</returns>
        //public static bool BeginTooltipWindow(Rect hoveringArea) {
        //    if (CurrentWindow == null || CurrentWindow.Type == WindowType.Tooltip) {
        //        Logger.Warn("Cannot begin Tooltip window as the current window is invalid (Null or was a Tooltip window)");
        //        return false;
        //    }

        //    if (hoveringArea.Collide(ImGuiInput.MousePosition)) {
        //        //Styling.Push(StylingID.WindowMinimumSize, new Vector2(5, 16));

        //        _tooltipWnd.Position = Mouse.Position + new Vector2(12, 16);

        //        AssignNextWindowSize(_tooltipWnd);

        //        _windowStack.Push(_tooltipWnd);

        //        Rect clientRect = new(_tooltipWnd.Position, _tooltipWnd.Size);

        //        ImGuiLowLevel.BeginFullscreenScissorRect();
        //        ImGuiLowLevel.BeginCoordinateOffset(-ImGuiLowLevel.CoordinateOffset);

        //        ImGuiRender.DrawRect(clientRect, ImGuiColoring.Read(ColoringID.WindowBackground));
        //        ImGuiRender.DrawFrame(clientRect, ImGuiColoring.Read(ColoringID.WindowBorder));

        //        ImGuiLowLevel.BeginCoordinateOffset(clientRect.Position);
        //        ImGuiLowLevel.BeginScissorRect(clientRect);

        //        _tooltipWnd.Context.Reset();

        //        return true;
        //    }

        //    return false;
        //}
        //public static void EndTooltipWindow() {
        //    if (CurrentWindow == null || CurrentWindow.Type != WindowType.Tooltip) return;

        //    //Styling.Pop(StylingID.WindowMinimumSize);

        //    ImGuiLowLevel.EndScissorRect();
        //    ImGuiLowLevel.EndCoordinateOffset();
        //    ImGuiLowLevel.EndCoordinateOffset();
        //    ImGuiLowLevel.EndScissorRect();

        //    _windowStack.Pop();
        //}

        /// <summary>
        /// Decode widget's identity string into 2 parts: Display name, and control ID.
        /// </summary>
        /// <param name="input">Input identity, in the form of <c>[Name]___[ID]</c> (Both name and id can be optional)</param>
        /// <param name="name">Output display name of Widget</param>
        /// <param name="id">Output control ID of Widget</param>
        public static void DecodeWidgetIdentity(ReadOnlySpan<char> input, out ReadOnlySpan<char> name, out ReadOnlySpan<char> id) {
            var sep = input.IndexOf("___");

            switch (sep) {
                case -1:
                    name = input;
                    id = input;
                    break;

                case 0:
                    name = ReadOnlySpan<char>.Empty;

                    if (input.Length == 3) {
                        id = ReadOnlySpan<char>.Empty;
                    } else {
                        id = input[(sep + 3)..];
                    }
                    break;

                default:
                    name = input[0..sep];
                    
                    if (sep == input.Length - 3) {
                        id = ReadOnlySpan<char>.Empty;
                    } else {
                        id = input[(sep + 3)..];
                    }
                    break;
            }
        }
    }
}
