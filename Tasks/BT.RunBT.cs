using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf RunBT(BTNode bt) => new BTLeaf(bt.name, () => {
            BT.OnBaseTick(bt.onTickBase);
            BT.OnValidate(bt.onValidate);
        });

        public static BTLeaf RunBT(Func<BTNode> getBT) => new BTLeaf("Run BT", () => {
            BTNode bt = null;

            BT.OnEnter(() => bt ??= getBT());
            BT.OnBaseTick(bt.onTickBase);
            BT.OnValidate(bt.onValidate);
        });
    }
}