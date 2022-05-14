using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public static class Identifier {
        public static int HoveringID { get; private set; }
        public static int ActiveID { get; private set; }

        public static bool Hovering => HoveringID != 0 && HoveringID == _current;
        public static bool Activating => ActiveID != 0 && ActiveID == _current;

        private static int _current;
        public static int Current => _current;

        private static readonly Stack<int> _identifierStack;

        static Identifier() {
            _identifierStack = new(16);
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

        public static void Push(string id) {
            _identifierStack.Push(id.GetHashCode());
            RecalculateHashCache();
        }

        public static void Push(ReadOnlySpan<char> id) {
            _identifierStack.Push(id.ToString().GetHashCode());
            RecalculateHashCache();
        }

        public static void Push(int id) {
            _identifierStack.Push(id);

            RecalculateHashCache();
        }

        public static void Pop() {
            if (_identifierStack.Count == 0) {
                _current = 0;

                return;
            }

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
            HashCode hashCombiner = new();
            foreach (var id in _identifierStack) {
                hashCombiner.Add(id);
            }

            _current = hashCombiner.ToHashCode();
        }

        internal static void ResetForNewFrame() {
            _identifierStack.Clear();
            _current = 0;
            HoveringID = 0;
        }
    }
}
