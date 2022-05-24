using System;
using System.Numerics;
using DirectDimensional.Core;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.Direct3D11;
using System.Runtime.CompilerServices;

using DDTexture2D = DirectDimensional.Core.Texture2D;

namespace DirectDimensional.Editor.GUI {
    public class GuiWindow {
        public const float ScrollbarSize = 12;

        public const float TitlebarHeight = 22;
        public const float ResizeHandleSize = 12;

        public string Name { get; private set; }
        public string DisplayName { get; set; }

        // If the value is different than CurrentFrame - 1, it means the window just been created (not create in OOP)
        public int LastRenderingFrame { get; internal set; } = -1;

        public bool IsFocused => ImGui.FocusingWindow == this;

        protected Rect _windowRect;
        public Rect TitlebarRect => new(_windowRect.Position, new Vector2(_windowRect.Size.X, TitlebarHeight));

        public Vector2 Position {
            get => _windowRect.Position;
            set {
                _windowRect.Position = Vector2.Clamp(value, Vector2.Zero, EditorWindow.ClientSize - Vector2.One * TitlebarHeight);
            }
        }

        public Vector2 Size {
            get => _windowRect.Size;
            set {
                _windowRect.Size = Vector2.Max(value, Vector2.Zero);
            }
        }

        public Rect ClientRect {
            get {
                var r = new Rect(Position, Size);

                int borderExtrude = HasBorder ? -1 : 0;

                if (HasTitlebar) {
                    r.Extrude(borderExtrude, borderExtrude, borderExtrude, -TitlebarHeight);
                } else {
                    r.Extrude(borderExtrude); // Border shrink
                }

                return r;
            }

            set {
                int borderExtrude = HasBorder ? 1 : 0;

                if (HasTitlebar) {
                    value.Extrude(borderExtrude, borderExtrude, borderExtrude, TitlebarHeight);
                } else {
                    value.Extrude(borderExtrude);
                }

                Position = value.Position;
                Size = value.Size;
            }
        }

        public Rect DisplayRect {
            get {
                Rect r = ClientRect;
                r.Extrude(-Styling.Read<Vector4>(StylingID.WindowContentPadding));

                if (EnableScrollX) {
                    r.Extrude(0, 0, -ScrollbarSize, 0);
                }

                if (EnableScrollY) {
                    r.Extrude(0, -ScrollbarSize, 0, 0);
                }

                return r;
            }

            set {
                value.Extrude(Styling.Read<Vector4>(StylingID.WindowContentPadding));
                if (EnableScrollX) {
                    value.Extrude(0, 0, ScrollbarSize, 0);
                }

                if (EnableScrollY) {
                    value.Extrude(0, ScrollbarSize, 0, 0);
                }

                ClientRect = value;
            }
        }

        public bool EnableScrollX { get; internal set; }
        public bool EnableScrollY { get; internal set; }

        internal uint Priority { get; set; } = 0;

        public WindowFlags Flags { get; internal set; }
        public bool IsFocusable => (Flags & WindowFlags.PreventFocus) != WindowFlags.PreventFocus;
        public bool HasTitlebar => (Flags & WindowFlags.DisableTitlebar) != WindowFlags.DisableTitlebar;
        public bool RenderBackground => (Flags & WindowFlags.DisableBackground) != WindowFlags.DisableBackground;

        public bool HasHScrollbar => (Flags & WindowFlags.DisableHScrollbar) != WindowFlags.DisableHScrollbar;
        public bool HasVScrollbar => (Flags & WindowFlags.DisableVScrollbar) != WindowFlags.DisableVScrollbar;

        public bool HasBorder => (Flags & WindowFlags.DisableBorder) != WindowFlags.DisableBorder;

        public event Action? OnEndWindow;

        public List<DrawCall> DrawCalls { get; private set; }
        private int _oldIndexCount = -1;

        public WindowDrawingContext Context { get; private set; }

        protected Vector2 _scrolling;
        public Vector2 Scrolling {
            get => _scrolling;
            set => _scrolling = Vector2.Max(Vector2.Zero, value);
        }

        public GuiWindow(string name) {
            Name = name;
            DrawCalls = new(32);

            Context = new(this);
            DisplayName = name;
        }

        private uint _drawCompositeCount = 0;

        /// <summary>
        /// Increment underlying counter to do complicated mesh drawing
        /// </summary>
        public void BeginDrawComposite() {
            if (_drawCompositeCount == 0) {
                _oldIndexCount = GUI.Context.Indices.Count;
            }
            _drawCompositeCount++;
        }

        /// <summary>
        /// Decrement underlying counter to do complicated mesh drawing. Append 1 draw call if counter hit 0
        /// </summary>
        public void EndDrawComposite() {
            if (_drawCompositeCount == 0) {
                Logger.Warn(nameof(EndDrawComposite) + " cannot be called because the window is not in drawing composite state");
                return;
            }

            if (--_drawCompositeCount == 0) {
                var ic = GUI.Context.Indices.Count - _oldIndexCount;
                if (ic != 0) {
                    DrawCalls.Add(new DrawCall {
                        IndexCount = (uint)ic,
                        IndexLocation = (uint)_oldIndexCount,

                        ScissorsRect = LowLevel.CurrentScissorRect,
                    });
                }

                _oldIndexCount = -1;
            }
        }

        /// <summary>
        /// Decrement underlying counter to do complicated mesh drawing. Append 1 draw call if counter hit 0
        /// </summary>
        public void EndDrawComposite(DDTexture2D? texture) {
            if (texture == null) {
                EndDrawComposite(null, null);
            } else {
                if (texture.IsRenderable) {
                    EndDrawComposite(texture.DXSRV, texture.DXSampler);
                } else {
                    Logger.Warn("EndDrawComposite: Texture is not renderable, fallback to default texture");
                    EndDrawComposite(null, null);
                }
            }
        }

        /// <summary>
        /// Apply 1 draw call with texture to system
        /// </summary>
        public void EndDrawComposite(ShaderResourceView? pTexture, SamplerState? pSampler) {
            if (_drawCompositeCount == 0) {
                Logger.Warn(nameof(EndDrawComposite) + " cannot be called because the window is not in drawing composite state");
                return;
            }

            if (--_drawCompositeCount == 0) {
                var ic = GUI.Context.Indices.Count - _oldIndexCount;
                if (ic != 0) {
                    DrawCalls.Add(new DrawCall {
                        IndexCount = (uint)ic,
                        IndexLocation = (uint)_oldIndexCount,

                        TexturePointer = pTexture.GetNativePtr(),
                        SamplerPointer = pSampler.GetNativePtr(),
                        ScissorsRect = LowLevel.CurrentScissorRect,

                        Topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleList,
                    });
                }

                _oldIndexCount = -1;
            }
        }

        public bool IsDrawingComposite {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => _drawCompositeCount > 0;
        }

        /// <summary>
        /// Use for debugging
        /// </summary>
        public uint DrawCompositeCounter => _drawCompositeCount;

        public void Move(Vector2 delta) {
            Position += delta;
        }

        protected bool hasRegistered = false;

        public virtual void ResetForNewFrame() {
            DrawCalls.Clear();
            hasRegistered = false;
            _oldIndexCount = -1;

            if (_drawCompositeCount != 0) {
                Logger.Warn("Drawing composite leak detected for window '" + Name + "'");
            }
            _drawCompositeCount = 0;
        }

        public virtual void RegisterToGlobalDrawing() {
            if (hasRegistered) return;

            Engine.GlobalDrawCalls.Add(DrawCalls);
            hasRegistered = true;
        }

        internal void EndWindowCall() {
            OnEndWindow?.Invoke();
            OnEndWindow = null;
        }
    }
}
