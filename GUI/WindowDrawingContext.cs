using System.Numerics;
using DirectDimensional.Core;
using System.Runtime.CompilerServices;

namespace DirectDimensional.Editor.GUI {
    public sealed class WindowDrawingContext {
        private readonly GuiWindow _wnd;
        private Vector2 _position, _contentSize;
        public Vector2 CurrentPosition {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _position = value;
        }
        public Vector2 ContentSize => _contentSize;

        public Rect CurrentRect { get; private set; }

        // Horizontal layout handling
        private bool _inHorizontalMode = false;
        private float _highestItemHorizontal = 0;

        private Vector2 _horizontalJump;

        public WindowDrawingContext(GuiWindow wnd) {
            _wnd = wnd;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rect GetRect(float sx, float sy) {
            return GetRect(new Vector2(sx, sy));
        }
        public Rect GetRect(Vector2 size) {
            size = Vector2.Max(Vector2.Zero, size);

            var elemSpace = ElementSpacing;
            var oldPos = _position;

            _contentSize = Vector2.Max(_position + size, _contentSize);

            if (_inHorizontalMode) {
                _position.X += size.X + elemSpace;
                _highestItemHorizontal = MathF.Max(_highestItemHorizontal, size.Y);
            } else {
                _horizontalJump = _position + new Vector2(size.X + elemSpace, 0);
                _position = new(0, _position.Y + size.Y + elemSpace);
            }

            CurrentRect = new(oldPos, size);
            return CurrentRect;
        }

        public void BeginHorizontalLayout() {
            if (_inHorizontalMode) return;

            _position = _horizontalJump;
            _inHorizontalMode = true;
        }

        public void EndHorizontalLayout() {
            if (!_inHorizontalMode) return;
            _inHorizontalMode = false;

            _position = new(0, _position.Y + _highestItemHorizontal + ElementSpacing);
            _highestItemHorizontal = 0;
        }

        internal void Reset() {
            _position = default;
            _contentSize = default;

            CurrentRect = default;

            _inHorizontalMode = false;
            _highestItemHorizontal = 0;
            _horizontalJump = default;
        }

        private static int ElementSpacing {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Styling.Read<int>(StylingID.LayoutElementSpacing);
        }
    }
}
