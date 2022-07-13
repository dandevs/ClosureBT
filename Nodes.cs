using System;
using System.Collections.Generic;

namespace ClosureBT {
    public class BTNode {
        public BTNode parent;
        private BTNode root;

        protected BTNode _lastActiveLeafNode;
        protected BTNode lastActiveLeafNode {
            get => root._lastActiveLeafNode;
            set => root._lastActiveLeafNode = value;
        }

        public BT.Status status = BT.Status.Resting;
        public string name;
        public bool done = false;
        public bool dynamic = false;
        public bool stubbedAsLeafNode = false;

        public Func<BT.Status> onTickBase;
        public Func<bool> onValidate = () => true;

        public Action onEnterDelegates = () => {};
        public Action onExitDelegates = () => {};
        public Action onTickDelegates = () => {};
        public Action onDisableDelegates = () => {};
        public Action onSuccessDelegates = () => {};
        public Action onFailureDelegates = () => {};

        public BTNode(string name) {
            this.name = name;
            parent = BTStack.current;
            root = parent?.root ?? this;
        }

        public virtual void AddChild(BTNode child) {}

        public void Reset() {
            if (status == BT.Status.Resting)
                return;

            var leaf = FindActiveLeafNode();

            if (leaf == this)
                ResetDownwards();
            else {
                if (leaf != null) {
                    leaf.ResetUpwards(this);
                    ResetDownwards();
                }
                else
                    ResetDownwards();
            }
        }

        public void ResetDownwards() {
            if (status == BT.Status.Resting)
                return;

            if (status == BT.Status.Running) {
                onExitDelegates.Invoke();
                onFailureDelegates.Invoke();
            }

            onDisableDelegates.Invoke();

            status = BT.Status.Resting;
            done = false;
            OnReset();
        }

        public void ResetUpwards(BTNode stoppingNode) {
            if (status == BT.Status.Resting)
                return;

            for (var node = this; node != null; node = node.parent) {
                if (node == stoppingNode)
                    break;

                node.ResetDownwards();
            }
        }

        public BT.Status ResetAndTick() {
            Reset();
            return Tick();
        }

        public BT.Status Tick(bool autoResetLastActiveLeafNode = true) {
            if (done) {
                if (!onValidate()) {
                    if (autoResetLastActiveLeafNode)
                        lastActiveLeafNode?.ResetUpwards(this.parent);

                    return FindRootNodeForReset().ResetAndTick();
                }
                
                return status;
            }

            if (status == BT.Status.Resting) 
                onEnterDelegates.Invoke();

            if (this is BTLeaf || stubbedAsLeafNode)
                lastActiveLeafNode = this;

            status = onTickBase();
            // onTickDelegates.Invoke();

            if (status == BT.Status.Running)
                onTickDelegates.Invoke();

            if (status != BT.Status.Running) {
                done = true;
                onExitDelegates.Invoke();

                if (status == BT.Status.Success)
                    onSuccessDelegates.Invoke();
                else
                    onFailureDelegates.Invoke();
            }
            
            return status;
        }

        protected virtual void OnReset() {}

        protected BTNode FindRootNodeForReset() {
            var node = this;

            if (node.parent?.done == false)
                return node;

            for (node = this; node.parent != null; node = node.parent)
                if (!node.parent.done)
                    return node;

            return node;
        }

        public virtual BTNode FindActiveLeafNode() => null;

        public bool CheckValidity() {
            if (status == BT.Status.Resting)
                return true;
            else
                return onValidate();
        }
    }

    //----------------------------------------------------------------------------------
    
    public class BTComposite : BTNode {
        public List<BTNode> children = new();
        public bool repeating = false;

        public BTComposite(string name, Action nodes) : base(name) {
            BTStack.current?.AddChild(this);

            BTStack.Push(this);
            nodes();
            Internal.Compile(children);
            BTStack.Pop();

            onTickBase ??= () => BT.Status.Failure;
        }

        override public void AddChild(BTNode child) {
            children.Add(child);
        }

        override protected void OnReset() {
            for (var i = 0; i < children.Count; i++)
                children[i].Reset();
        }

        override public BTNode FindActiveLeafNode() {
            if (done)
                return null;

            if (stubbedAsLeafNode)
                return this;

            for (var i = 0; i < children.Count; i++) {
                var node = children[i].FindActiveLeafNode();

                if (node != null)
                    return node;
            }

            return null;
        }
    }

    //----------------------------------------------------------------------------------
    
    public class BTLeaf : BTNode {
        public BTLeaf(string name, Action lifecycle) : base(name) {
            BTStack.current?.AddChild(this);
            BTStack.Push(this);
            lifecycle();
            BTStack.Pop();

            onTickBase ??= () => BT.Status.Failure;
        }

        override public BTNode FindActiveLeafNode() {
            return !done ? this : null;
        }
    }

    //----------------------------------------------------------------------------------

    public class BTDecorator : BTNode {
        public BTNode child;

        public BTDecorator(string name, Action lifecycle) : base(name) {
            onValidate = null;

            BTStack.current?.AddChild(this);
            BTStack.Push(this);
            lifecycle();
            BTStack.Pop();

            onTickBase ??= () => child.Tick();
            onValidate ??= () => child.CheckValidity();
        }

        protected override void OnReset() {
            child.Reset();
        }

        public override BTNode FindActiveLeafNode() {
            if (!done && stubbedAsLeafNode)
                return this;
            
            return !done ? child.FindActiveLeafNode() : null;
        }
    }

    //----------------------------------------------------------------------------------

    public static class BTStack {
        public static List<BTNode> stack = new List<BTNode>();
        public static BTNode current => stack.Count > 0 ? stack[stack.Count - 1] : null;

        public static void Push(BTNode node) => stack.Add(node);
        public static void Pop() => stack.RemoveAt(stack.Count - 1);
    }

    //----------------------------------------------------------------------------------

    public static partial class BT {
        public static BTNode current => BTStack.current;
        public enum Status { Resting, Success, Failure, Running }
    }

    public partial class Internal {
        private static List<int> indicesToRemove = new();

        public static void Compile(List<BTNode> nodes) {
            indicesToRemove.Clear();

            for (int i = 0; i < nodes.Count - 1; i++) {
                var (node, nextNode) = (nodes[i], nodes[i + 1]);

                if (node is BTDecorator decorator) {
                    decorator.child = nextNode;
                    decorator.child.parent = decorator;
                    indicesToRemove.Add(i + 1);
                }
            }

            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
                nodes.RemoveAt(indicesToRemove[i]);
        }
    }
}