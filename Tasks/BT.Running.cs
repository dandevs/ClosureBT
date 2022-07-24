using System;

namespace ClosureBT {
    public static partial class BT {
        /// <summary> Node that just returns Status.Running </summary>
        public static BTLeaf JustRunningStatus(string name, Action lifecycle) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => BT.Status.Running);
            lifecycle();
        });

        public static BTLeaf JustRunningStatus(Action lifecycle) => JustRunningStatus("Running", lifecycle);
    }
}