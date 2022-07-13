using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf Running(string name, Action lifecycle) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => BT.Status.Running);
            lifecycle();
        });

        public static BTLeaf Running(Action lifecycle) => Running("Running", lifecycle);
    }
}