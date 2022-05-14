using System.Numerics;
using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public sealed class WindowDrawingContext {
        private readonly GuiWindow wnd;

        private Vector2 _position, _contentSize;
        public Vector2 CurrentPosition => _position;
        public Vector2 ContentSize => _contentSize;

        private bool _horizontalLayout;
        public bool InHorizontalMode => _horizontalLayout;

        private float _horizontalBaseLine = 0;

        public float RightWidthTillMax => _contentSize.X - _position.X;

        public WindowDrawingContext(GuiWindow window) {
            wnd = window;
        }

        public float HorizontalBaseLine {
            get => _horizontalBaseLine;
            set => _horizontalBaseLine = MathF.Max(0, value);
        }

        public void ForcePosition(Vector2 pos) {
            _position = Vector2.Max(Vector2.Zero, pos);
        }

        public Rect GetRect(float sx, float sy) {
            return GetRect(new Vector2(sx, sy));
        }

        public Rect GetRect(Vector2 size) {
            var position = _position;

            if (_horizontalLayout) {
                _contentSize = Vector2.Max(_contentSize, _position + size);
                _position.X += size.X + 3;
            } else {
                _contentSize = Vector2.Max(_contentSize, new(_horizontalBaseLine + size.X, _position.Y + size.Y));
                _position = new(_horizontalBaseLine, _position.Y + size.Y + 3);
            }

            return new(position, size);
        }

        /// <summary>
        /// Linebreak, advance and return the rect advanced.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public Rect GetRect(float height) {
            var position = _position;

            _contentSize = new(MathF.Max(_contentSize.X, wnd.DisplayRect.Width), MathF.Max(_contentSize.Y, _position.Y + height));
            _position = new(_horizontalBaseLine, _position.Y + height + 3);

            return new(position.X, position.Y, _contentSize.X, height);
        }

        public void Space(float size) {
            if (size <= 0) return;

            if (_horizontalLayout) {
                _contentSize.X = MathF.Max(_contentSize.X, _position.X + size);
                _position.X += size;
            } else {
                _contentSize.Y = MathF.Max(_contentSize.Y, _position.Y + size);
                _position.Y += size;
            }
        }

        public void ToVerticalLayoutMode() {
            _horizontalLayout = false;
        }

        public void ToHorizontalLayoutMode() {
            _horizontalLayout = true;
        }

        internal void Reset() {
            _horizontalLayout = false;
            _position = default;
            _contentSize = new(wnd.DisplayRect.Width, 0);
        }
    }
}
