using DirectDimensional.Core;
using System.Runtime.CompilerServices;

namespace DirectDimensional.Editor.GUI {
    public static class Identifier {
        private static int _hashcodeDefault;

        public static int HoveringID { get; private set; }
        public static int ActiveID { get; private set; }

        public static bool Hovering => HoveringID != 0 && HoveringID == _current;
        public static bool Activating => ActiveID != 0 && ActiveID == _current;

        private static int _current;
        public static int Current => _current;

        private static readonly Stack<int> _identifierStack;
        public static int StackCount => _identifierStack.Count;

        static Identifier() {
            _identifierStack = new(16);

            _hashcodeDefault = new HashCode().ToHashCode();
        }

        public static int Calculate(int id) {
            HashCode hashCombiner = new();
            hashCombiner.Add(id);

            foreach (var _id in _identifierStack) {
                hashCombiner.Add(_id);
            }

            return hashCombiner.ToHashCode();
        }

        public static int Calculate(string id) {
            HashCode hashCombiner = new();
            hashCombiner.Add(id.GetHashCode());

            foreach (var _id in _identifierStack) {
                hashCombiner.Add(_id);
            }

            return hashCombiner.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(string id) {
            Push(id.GetHashCode());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(ReadOnlySpan<char> id) {
            Push(id.ToString().GetHashCode());
        }

        public static void Push(int id) {
            _identifierStack.Push(id);
            RecalculateHashCache();
        }

        public static void Pop() {
            _identifierStack.Pop();
            RecalculateHashCache();
        }

        public static void SetActiveID() {
            ActiveID = _current;
        }

        public static void SetActiveID(int value) {
            ActiveID = value;
        }

        public static void SetHoveringID() {
            HoveringID = _current;
        }

        public static void SetHoveringID(int id) {
            HoveringID = id;
        }

        public static void ClearHoveringID() {
            HoveringID = 0;
        }

        public static void ClearActiveID() {
            ActiveID = 0;
        }

        public static bool ClearCurrentHoveringID() {
            if (HoveringID != 0 && HoveringID == _current) {
                HoveringID = 0;
                return true;
            }

            return false;
        }

        public static bool ClearCurrentActiveID() {
            if (ActiveID != 0 && ActiveID == _current) {
                ActiveID = 0;
                return true;
            }

            return false;
        }

        private static void RecalculateHashCache() {
            if (_identifierStack.Count == 0) {
                _current = _hashcodeDefault;
                return;
            }

            HashCode hashCombiner = new();
            foreach (var id in _identifierStack) {
                hashCombiner.Add(id);
            }

            _current = hashCombiner.ToHashCode();
        }

        internal static void ResetForNewFrame() {
            _identifierStack.Clear();
            _current = _hashcodeDefault;
            HoveringID = 0;
        }

        public readonly struct Laziness : IDisposable {
            public void Dispose() {
                Pop();
            }
        }
        public static Laziness Lazy(int id) {
            Push(id);
            return default;
        }
        public static Laziness Lazy(string id) {
            Push(id);
            return default;
        }
        public static Laziness Lazy(ReadOnlySpan<char> id) {
            Push(id);
            return default;
        }
    }
}
