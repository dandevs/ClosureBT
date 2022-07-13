using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf Leaf(string name, Action lifecycle) => new BTLeaf(name, lifecycle);
        public static BTLeaf Leaf(Action lifecycle) => new BTLeaf("Leaf", lifecycle);

        public static BTDecorator Decorator(string name, Action lifecycle) => new BTDecorator(name, lifecycle);
        public static BTDecorator Decorator(Action lifecycle) => new BTDecorator("Decorator", lifecycle);

        public static BTComposite Composite(string name, Action nodes) => new BTComposite(name, nodes);
        public static BTComposite Composite(Action nodes) => new BTComposite("Composite", nodes);

        //----------------------------------------------------------------------------------------------------------------------

        public static void OnBaseTick(Func<BT.Status> onTick) {
            if (BT.current != null)
                BT.current.onTickBase = onTick;
        }

        public static void OnTick(Action onTick) {
            if (BT.current != null)
                BT.current.onTickDelegates += onTick;
        }

        public static void OnValidate(Func<bool> onValidate) {
            if (BT.current != null)
                BT.current.onValidate = onValidate;
        }

        public static void OnEnter(Action onEnter) {
            if (BT.current != null)
                BT.current.onEnterDelegates += onEnter;
        }

        public static void OnExit(Action onExit) {
            if (BT.current != null)
                BT.current.onExitDelegates += onExit;
        }

        public static void OnSuccess(Action onSuccess) {
            if (BT.current != null)
                BT.current.onSuccessDelegates += onSuccess;
        }

        public static void OnFailure(Action onFailure) {
            if (BT.current != null)
                BT.current.onFailureDelegates += onFailure;
        }

        public static void Repeating(bool repeating = true) {
            if (BT.current is BTComposite composite)
                composite.repeating = repeating;
        }

        public static void Dynamic(bool dynamic = true) {
            if (BT.current != null)
                BT.current.dynamic = dynamic;
        }

        public static void StubAsLeafNode(bool stubAsLeafNode = true) {
            if (BT.current != null)
                BT.current.stubbedAsLeafNode = stubAsLeafNode;
        }
    }
}