using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public static class Identifier {
        private static int _cacheIdentifierHash;

        public static int ActiveID { get; private set; }
        public static int HoveringID { get; private set; }

        public static int CurrentIdentifierHash => _cacheIdentifierHash;

        private static readonly Stack<string> _identifierStack;

        static Identifier() {
            _identifierStack = new(16);
        }

        public static void PushIdentifier(string id) {
            _identifierStack.Push(id);

            RecalculateHashCache();
        }

        public static void PopIdentifier() {
            if (_identifierStack.Count == 0) {
                _cacheIdentifierHash = 0;

                return;
            }

            _identifierStack.Pop();
            RecalculateHashCache();
        }

        public static void PopIdentifier(int count) {
            if (_identifierStack.Count == 0) {
                _cacheIdentifierHash = 0;

                return;
            }

            if (count < 0) {
                _identifierStack.Clear();
                return;
            }

            while (count > 0 && _identifierStack.Count > 0) {
                _identifierStack.Pop();
                count--;
            }

            RecalculateHashCache();
        }

        public static void SetActiveID() {
            ActiveID = CurrentIdentifierHash;
        }

        public static void ClearActiveID() {
            ActiveID = 0;
        }

        public static bool IsIDActived() {
            return ActiveID != 0 && ActiveID == CurrentIdentifierHash;
        }

        public static bool SetHoveringID() {
            if (HoveringID == _cacheIdentifierHash) return false;

            HoveringID = _cacheIdentifierHash;

            return true;
        }

        public static bool ClearHoveringID() {
            if (HoveringID == 0) return false;

            HoveringID = 0;

            return true;
        }

        public static bool ClearCurrentHoveringID() {
            if (HoveringID != CurrentIdentifierHash || HoveringID == 0) return false;

            HoveringID = 0;

            return true;
        }

        public static bool IsIDHovered() {
            return HoveringID != 0 && HoveringID == CurrentIdentifierHash;
        }

        private static void RecalculateHashCache() {
            HashCode hashCombiner = new();
            foreach (var id in _identifierStack) {
                hashCombiner.Add(id);
            }

            _cacheIdentifierHash = hashCombiner.ToHashCode();
        }

        internal static void ValidateHashToPrepareFrame() {
            if (_identifierStack.Count > 0) {
                Logger.Warn("Editor: Identifier leak. Make sure you call " + nameof(Identifier.PushIdentifier) + " follow by the same number of " + nameof(Identifier.PopIdentifier) + ".");
                _identifierStack.Clear();
            }
        }
    }
}
