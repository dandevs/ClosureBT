using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf Defer(BTNode bt) => new BTLeaf(bt.name, () => {
            BT.OnBaseTick(bt.onTickBase);
            BT.OnValidate(bt.onValidate);
        });

        public static BTLeaf Defer(Func<BTNode> getBT) => new BTLeaf("Defer", () => {
            BTNode bt = null;

            BT.OnEnter(() => bt ??= getBT());
            BT.OnBaseTick(bt.onTickBase);
            BT.OnValidate(bt.onValidate);
        });
    }
}