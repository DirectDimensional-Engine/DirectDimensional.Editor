using System;
using System.Numerics;
using DirectDimensional.Core;
using DirectDimensional.Bindings;
using DirectDimensional.Bindings.Direct3D11;
using System.Runtime.CompilerServices;

using DDTexture2D = DirectDimensional.Core.Texture2D;

namespace DirectDimensional.Editor.GUI {
    public abstract class GuiWindow {
        public string Name { get; private set; }

        // If the value is different than CurrentFrame - 1, it means the window just been created (not create in OOP)
        public int LastRenderingFrame { get; internal set; } = -1;

        public bool IsFocused => ImGui.FocusingWindow == this;

        protected Rect _windowRect;
        public abstract Vector2 Position { get; set; }
        public abstract Vector2 Size { get; set; }

        public WindowType Type { get; protected set; } = WindowType.Undefined;

        public List<DrawCall> DrawCalls { get; private set; }
        private int _oldIndexCount = -1;

        /// <summary>
        /// Rectangle that display every widget drawn inside the window (not include built-in widgets like window's slider)
        /// </summary>
        public abstract Rect DisplayRect { get; }

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
        }

        public abstract bool IsFocusable { get; }

        /// <summary>
        /// Begin draw complicated mesh in 1 single draw call
        /// </summary>
        public void BeginDrawComposite() {
            if (_oldIndexCount >= 0) {
                Logger.Warn(nameof(BeginDrawComposite) + " cannot be called because the window is in drawing composite state");
                return;
            }

            _oldIndexCount = ImGuiContext.Indices.Count;
        }

        /// <summary>
        /// Apply 1 draw call to system
        /// </summary>
        public void EndDrawComposite() {
            if (_oldIndexCount < 0) {
                Logger.Warn(nameof(EndDrawComposite) + " cannot be called because the window is not in drawing composite state");
                return;
            }

            var ic = ImGuiContext.Indices.Count - _oldIndexCount;
            if (ic != 0) {
                DrawCalls.Add(new DrawCall {
                    IndexCount = (uint)ic,
                    IndexLocation = (uint)_oldIndexCount,

                    ScissorsRect = ImGuiLowLevel.CurrentScissorRect,
                });
            }

            _oldIndexCount = -1;
        }

        public void EndDrawComposite(DDTexture2D? texture, D3D11_PRIMITIVE_TOPOLOGY topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleList) {
            if (texture == null) {
                EndDrawComposite(null, null, topology);
            } else {
                if (texture.IsRenderable) {
                    EndDrawComposite(texture.DXSRV, texture.DXSampler, topology);
                } else {
                    Logger.Warn("EndDrawComposite: Texture is not renderable, fallback to default texture");
                    EndDrawComposite(null, null, topology);
                }
            }
        }

        /// <summary>
        /// Apply 1 draw call with texture to system
        /// </summary>
        public void EndDrawComposite(ShaderResourceView? pTexture, SamplerState? pSampler, D3D11_PRIMITIVE_TOPOLOGY topology = D3D11_PRIMITIVE_TOPOLOGY.TriangleList) {
            if (_oldIndexCount < 0) {
                Logger.Warn(nameof(EndDrawComposite) + " cannot be called because engine is not in drawing composite state. Operation cancelled.");
                return;
            }

            DrawCalls.Add(new DrawCall {
                IndexCount = (uint)(ImGuiContext.Indices.Count - _oldIndexCount),
                IndexLocation = (uint)_oldIndexCount,

                TexturePointer = pTexture.GetNativePtr(),
                SamplerPointer = pSampler.GetNativePtr(),
                ScissorsRect = ImGuiLowLevel.CurrentScissorRect,

                Topology = topology,
            });

            _oldIndexCount = -1;
        }

        public bool IsDrawingComposite {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => _oldIndexCount >= 0;
        }

        public void Move(Vector2 delta) {
            Position += delta;
        }

        protected bool hasRegistered = false;

        public virtual void ResetForNewFrame() {
            DrawCalls.Clear();
            hasRegistered = false;
            _oldIndexCount = -1;
        }

        public virtual void RegisterToGlobalDrawing() {
            if (hasRegistered) return;

            ImGuiEngine.GlobalDrawCalls.Add(DrawCalls);
            hasRegistered = true;
        }

        public bool EnableScrollY { get; internal set; }
    }

    public sealed class StandardGuiWindow : GuiWindow {
        public const float HorizontalScrollbarWidth = 12;

        public const float TitlebarHeight = 22;
        public const float ResizeHandleSize = 12;

        public static readonly Vector2 MinimumSize = new(60);

        public StandardWindowFlags Flags { get; internal set; }
        public override bool IsFocusable => (Flags & StandardWindowFlags.PreventFocus) != StandardWindowFlags.PreventFocus;
        public bool HasTitlebar => (Flags & StandardWindowFlags.DisableTitlebar) != StandardWindowFlags.DisableTitlebar;
        public bool RenderBackground => (Flags & StandardWindowFlags.DisableBackground) != StandardWindowFlags.DisableBackground;

        public StandardGuiWindow(string name) : base(name) {
            Type = WindowType.Standard;

            _windowRect.Size = new Vector2(400, 300);
        }

        public Rect TitlebarRect => new(_windowRect.Position, new Vector2(_windowRect.Size.X, TitlebarHeight));

        public override Vector2 Position {
            get => _windowRect.Position;
            set {
                _windowRect.Position = Vector2.Clamp(value, Vector2.Zero, EditorWindow.ClientSize - Vector2.One * TitlebarHeight);
            }
        }

        public override Vector2 Size {
            get => _windowRect.Size;
            set {
                _windowRect.Size = Vector2.Max(value, MinimumSize);
            }
        }

        public Rect ClientRect {
            get {
                var r = new Rect(Position, Size);
                if ((Flags & StandardWindowFlags.DisableTitlebar) != StandardWindowFlags.DisableTitlebar) {
                    r.Extrude(0, 0, 0, -TitlebarHeight);
                }

                return r;
            }
        }

        public override Rect DisplayRect {
            get {
                Rect r = ClientRect;
                r.Extrude(-Styling.Read<Vector4>(StylingID.WindowContentPadding));

                if (EnableScrollY) {
                    r.Extrude(0, -HorizontalScrollbarWidth, 0, 0);
                }

                return r;
            }
        }
    }

    public sealed class TooltipWindow : GuiWindow {
        public TooltipWindow(string name) : base(name) {
            Type = WindowType.Tooltip;
        }

        public override Rect DisplayRect => new(Position, Size);
        public override bool IsFocusable => false;

        public override Vector2 Position {
            get => _windowRect.Position;
            set => _windowRect.Position = value;
        }

        public override Vector2 Size {
            get => _windowRect.Size;
            set => _windowRect.Size = value;
        }
    }
}
