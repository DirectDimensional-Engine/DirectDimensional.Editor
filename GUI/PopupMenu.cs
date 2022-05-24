using System.Collections;
using System.Numerics;
using DirectDimensional.Core;

namespace DirectDimensional.Editor.GUI {
    public sealed class PopupMenu {
        internal enum ItemType {
            Item, Tree,
        }
        internal interface IPopupMenuItem {
            ItemType Type { get; }
        }

        internal class PopupMenuItem : IPopupMenuItem {
            public string Name { get; private set; }
            public Action Callback { get; private set; }

            public PopupMenuItem(string name, Action callback) {
                Name = name;
                Callback = callback;
            }

            public ItemType Type => ItemType.Item;
        }
        internal class PopupMenuTree : IPopupMenuItem, IEnumerable<IPopupMenuItem> {
            public string Name { get; private set; }
            public List<IPopupMenuItem> Items { get; private set; } = new(4);
            public ItemType Type => ItemType.Tree;

            public PopupMenuTree(string name) {
                Name = name;
            }

            public IEnumerator<IPopupMenuItem> GetEnumerator() {
                return Items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return Items.GetEnumerator();
            }
        }

        public static PopupMenu? DisplayInstance { get; private set; }
        public static Vector2 CursorLocation { get; private set; }

        internal List<IPopupMenuItem> Items { get; private set; }

        private readonly Stack<PopupMenuTree> _treeStack;
        public PopupMenu() {
            Items = new();
            _treeStack = new();
        }

        public void AddItem(string name, Action clickCallback) {
            var item = new PopupMenuItem(name, clickCallback);

            if (_treeStack.TryPeek(out var tree)) {
                tree.Items.Add(item);
            } else {
                Items.Add(item);
            }
        }

        public void BeginTree(string name) {
            _treeStack.Push(new(name));
        }

        public void EndTree() {
            if (_treeStack.TryPop(out var pop)) {
                if (_treeStack.TryPeek(out var receiver)) {
                    receiver.Items.Add(pop);
                    return;
                } else {
                    Items.Add(pop);
                }
            } else {
                Logger.Warn("Cannot request " + nameof(EndTree) + " operator as the tree stack is empty");
            }
        }

        public void MarkDisplayInstance() {
            DisplayInstance = this;
            CursorLocation = Mouse.Position;
        }
    }
}
