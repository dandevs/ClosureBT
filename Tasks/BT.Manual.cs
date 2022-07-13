using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf Manual(string name, Action<Action<bool>> nodes) => new BTLeaf(name, () => {
            var doneStatus = false;
            var called = false;
            
            Action<bool> doneFunc = successful => {
                called = true;
                doneStatus = successful;
            };

            BT.OnEnter(() => {
                called = false;
                doneStatus = false;
            });

            BT.OnBaseTick(() => {
                if (called)
                    return doneStatus ? BT.Status.Success : BT.Status.Failure;

                return BT.Status.Running;
            });

            nodes(doneFunc);
        });

        public static BTLeaf Manual(Action<Action<bool>> nodes) => Manual("Manual", nodes);
    }
}