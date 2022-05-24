using DirectDimensional.Core;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;
using DirectDimensional.Editor.GUI.Grouping;
using System.Runtime.CompilerServices;

using DDTexture2D = DirectDimensional.Core.Texture2D;

namespace DirectDimensional.Editor.GUI {
    public static unsafe class ImGui {
        private static readonly Stack<GuiWindow> _windowStack;
        private static readonly List<GuiWindow> _windows;

        internal static List<GuiWindow> Windows => _windows;

        private static GuiWindow? _focusWindow, _hoveringWindow;
        public static GuiWindow? FocusingWindow => _focusWindow;
        public static GuiWindow? CurrentWindow => _windowStack.Count == 0 ? null : _windowStack.Peek();
        public static GuiWindow? HoveringWindow => _hoveringWindow;

        static ImGui() {
            _windowStack = new();
            _windows = new(8);
        }

        internal static void NewFrame() {
            if (_windowStack.Count != 0) {
                Logger.Warn("Window stack isn't empty. Make sure you called end window functions with correspond begin window functions.");
            }

            _windowStack.Clear();
        }

        public static void FocusWindow(GuiWindow? window) {
            if (window != null && !window.IsFocusable) return;

            _focusWindow = window;

            if (window != null) {
                _windows.Remove(window);
                _windows.Add(window);
            }
        }

        public static GuiWindow? SearchWindow(string name) {
            for (int i = 0; i < _windows.Count; i++) {
                if (_windows[i].Name == name) {
                    return _windows[i];
                }
            }

            return null;
        }

        public static T? SearchWindow<T>(string name) where T : GuiWindow {
            for (int i = 0; i < _windows.Count; i++) {
                if (_windows[i].Name == name && _windows[i] is T cast) {
                    return cast;
                }
            }

            return null;
        }

        /// <summary>
        /// Query focus window from last window to first window in the list
        /// </summary>
        /// <param name="predicate"></param>
        public static void QueryFocusWindow(Func<GuiWindow, bool> predicate) {
            for (int i = _windows.Count - 1; i >= 0; i--) {
                if (!_windows[i].IsFocusable) continue;

                if (predicate(_windows[i])) {
                    FocusWindow(_windows[i]);
                    break;
                }
            }
        }

        internal static void UpdateHoveringWindow() {
            var mp = Mouse.Position;

            for (int i = _windows.Count - 1; i >= 0; i--) {
                var wnd = _windows[i];
                if (!_windows[i].IsFocusable) continue;

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

        private static void ClearNextWindowData() {
            NextWindowX = NextWindowY = NextWindowWidth = NextWindowHeight = null;
        }

#pragma warning disable CS8774
        /// <summary>
        /// <para>Render a standard window.</para>
        /// <br>Notes:</br>
        /// <br>1. Call <seealso cref="EndStandardWindow"/> at the end of window.</br>
        /// <br>2. Avoid using specialized flags built like <seealso cref="WindowFlags.Tooltip"/> or</br>
        /// </summary>
        /// <param name="name">Name of the window.</param>
        /// <param name="flags">Special flags to change how window works</param>
        [MemberNotNull(nameof(CurrentWindow))]
        public static void BeginStandardWindow(string name, WindowFlags flags = WindowFlags.None) {
            var wnd = SearchWindow(name);
            if (wnd == null) {
                wnd = new(name);

                wnd.Size = Configuration.NewWindowSize;
                _windows.Add(wnd);
            }

            wnd.Flags = flags;

            bool contentFitX = (flags & WindowFlags.AutoContentFitX) == WindowFlags.AutoContentFitX;
            bool contentFitY = (flags & WindowFlags.AutoContentFitY) == WindowFlags.AutoContentFitY;

            AssignNextWindowPos(wnd);

            if (!contentFitX && NextWindowWidth != null) {
                wnd.Size = new(NextWindowWidth.Value, wnd.Size.Y);
                NextWindowWidth = null;
            }

            if (!contentFitY && NextWindowHeight != null) {
                wnd.Size = new(wnd.Size.X, NextWindowHeight.Value);
                NextWindowHeight = null;
            }

            _windowStack.Push(wnd);

            if (wnd.LastRenderingFrame != EditorApplication.FrameCount - 1) {
                FocusWindow(wnd);
            }

            wnd.LastRenderingFrame = EditorApplication.FrameCount;

            Identifier.Push(name);

            bool hasTitlebar = wnd.HasTitlebar;
            bool hasBackground = wnd.RenderBackground;

            if (hasTitlebar && Behaviours.Button("__WND_TITLEBAR_DRAGMODE__", wnd.TitlebarRect, ButtonFlags.DetectHeld, out _)) {
                wnd.Position += Mouse.Move;
            }

            // Store old display rect size so that 
            var rdisplayLast = wnd.DisplayRect;

            if (contentFitX && contentFitY) {
                wnd.DisplayRect = new Rect(wnd.DisplayRect.Position, wnd.Context.ContentSize);
            } else {
                if ((flags & WindowFlags.DisableResize) != WindowFlags.DisableResize) {
                    if (Behaviours.Button("__RESIZE_L__", new Rect(wnd.Position + new Vector2(0, wnd.Size.Y - GuiWindow.ResizeHandleSize), GuiWindow.ResizeHandleSize), ButtonFlags.DetectHeld, out _)) {
                        Vector2 move = default;

                        if (!contentFitX) move.X = Mouse.Move.X;
                        if (!contentFitY) move.Y = Mouse.Move.Y;

                        wnd.Position += new Vector2(move.X, 0);
                        wnd.Size += new Vector2(-move.X, move.Y);
                    }

                    if (Behaviours.Button("__RESIZE_R__", new Rect(wnd.Position + wnd.Size - new Vector2(GuiWindow.ResizeHandleSize), GuiWindow.ResizeHandleSize), ButtonFlags.DetectHeld, out _)) {
                        Vector2 move = default;

                        if (!contentFitX) move.X = Mouse.Move.X;
                        if (!contentFitY) move.Y = Mouse.Move.Y;

                        wnd.Size += move;
                    }
                }

                var dr = wnd.DisplayRect;

                if (contentFitX) wnd.DisplayRect = new Rect(dr.X, dr.Y, wnd.Context.ContentSize.X, dr.Height);
                if (contentFitY) wnd.DisplayRect = new Rect(dr.X, dr.Y, dr.Width, wnd.Context.ContentSize.Y);
            }

            bool hasHScrollbar = !contentFitX && wnd.HasHScrollbar;
            bool hasVScrollbar = !contentFitY && wnd.HasVScrollbar;
            var rdisplay = wnd.DisplayRect;

            wnd.BeginDrawComposite();

            var scontent = wnd.Context.ContentSize;
            var rclient = wnd.ClientRect;

            if (hasTitlebar) {
                Drawings.AddCompositeRect(wnd.TitlebarRect, Coloring.Read(ColoringID.WindowTitle));

                if (hasBackground) Drawings.AddCompositeRect(new(wnd.Position + new Vector2(0, GuiWindow.TitlebarHeight), wnd.Size - new Vector2(0, GuiWindow.TitlebarHeight)), Coloring.Read(ColoringID.WindowBackground));
            } else {
                if (hasBackground) Drawings.AddCompositeRect(new(wnd.Position, wnd.Size), Coloring.Read(ColoringID.WindowBackground));
            }

            // Components
            {
                wnd.EnableScrollX = scontent.X > rdisplayLast.Width + 3 && hasHScrollbar;
                if (hasHScrollbar) {
                    if (wnd.EnableScrollX) {
                        var hscrollArea = new Rect(rclient.Position.X, rclient.Max.Y - GuiWindow.ScrollbarSize, rclient.Width - GuiWindow.ResizeHandleSize, GuiWindow.ScrollbarSize);

                        float scrollX = wnd.Scrolling.X;
                        if (Behaviours.HorizontalScrollbar("__SCROLLBAR_X__", hscrollArea, rdisplay.Width, scontent.X, ref scrollX, out var hscrollHandle)) {
                            wnd.Scrolling = new(scrollX, wnd.Scrolling.Y);
                        }

                        Drawings.AddCompositeRect(hscrollArea, Color32.Green);
                        Drawings.AddCompositeRect(hscrollHandle, Color32.Blue);
                    } else {
                        wnd.Scrolling = new(0, wnd.Scrolling.X);
                    }
                } else {
                    wnd.EnableScrollX = false;
                }

                wnd.EnableScrollY = scontent.Y > rdisplayLast.Height + 3 && hasVScrollbar;
                if (wnd.EnableScrollY) {
                    var vscrollArea = new Rect(rclient.Max.X - GuiWindow.ScrollbarSize, rclient.Position.Y, GuiWindow.ScrollbarSize, rclient.Height - GuiWindow.ResizeHandleSize);

                    float scrollY = wnd.Scrolling.Y;
                    if (Behaviours.VerticalScrollbar("__SCROLLBAR_Y__", vscrollArea, rdisplay.Height, scontent.Y, ref scrollY, out var vscrollHandle)) {
                        wnd.Scrolling = new(wnd.Scrolling.X, scrollY);
                    }

                    Drawings.AddCompositeRect(vscrollArea, Color32.Green);
                    Drawings.AddCompositeRect(vscrollHandle, Color32.Blue);
                } else {
                    wnd.Scrolling = new(wnd.Scrolling.X, 0);
                }
            }

            wnd.EndDrawComposite();

            if (hasTitlebar) {
                var r = wnd.TitlebarRect;
                r.Extrude(-3, 0);

                Styling.Push(StylingID.TextMasking, true);
                Widgets.Text(r, wnd.DisplayName, HorizontalTextAnchor.Left, VerticalTextAnchor.Middle);
                Styling.Pop(StylingID.TextMasking);
            }

            if (wnd.HasBorder) Drawings.DrawFrame(new Rect(wnd.Position, wnd.Size), Coloring.Read(ColoringID.WindowBorder));

            rdisplay = wnd.DisplayRect;
            LowLevel.BeginCoordinateOffset(rdisplay.Position - wnd.Scrolling);
            LowLevel.BeginScissorRect(rdisplay);

            wnd.Context.Reset();
        }
#pragma warning restore CS8774

        public static void EndStandardWindow() {
            if (CurrentWindow == null) return;

            CurrentWindow.EndWindowCall();

            LowLevel.EndScissorRect();
            LowLevel.EndCoordinateOffset();

            Identifier.Pop();

            _windowStack.Pop();
        }

        public static void BeginTooltip(string name) {
            BeginTooltip(name, new(0, 20));
        }
        public static void BeginTooltip(string name, Vector2 mouseOffset) {
            LowLevel.BeginRectGroupFullscreen();

            var mp = Mouse.Position;

            NextWindowX = mp.X + mouseOffset.X;
            NextWindowY = mp.Y + mouseOffset.Y;
            BeginStandardWindow(name, WindowFlags.Tooltip);
            CurrentWindow.Priority = 100000;
        }
        public static void EndTooltip() {
            EndStandardWindow();

            LowLevel.EndRectGroup();
        }

        //public static void InitializePopupContext(string name) {
        //    if (CurrentWindow == null) {
        //        Logger.Log("Cannot initialize popup context if no Standard Window is taking place");
        //        return;
        //    }
        //}

        //public static bool BeginPopup(string name) {
        //    var id = Identifier.Calculate(name);
        //    if (!opened) {
        //        ClearNextWindowData();
        //        return false;
        //    }

        //    BeginStandardWindow(name, WindowFlags.Popup);
        //    CurrentWindow.Priority = 50000;

        //    return true;
        //}

        //public static void EndPopup() {
        //    EndStandardWindow();
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
