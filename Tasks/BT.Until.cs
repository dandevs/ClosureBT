using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf WaitUntil(string name, Func<bool> condition) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => condition() ? BT.Status.Success : BT.Status.Running);
            BT.OnValidate(condition);
        });

        public static BTLeaf WaitUntil(Func<bool> condition) => WaitUntil("Wait Until", condition);

        //----------------------------------------------------------------------------------------------------------------------
        
        public static BTLeaf WaitWhile(string name, Func<bool> condition) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => condition() ? BT.Status.Running : BT.Status.Success);
            BT.OnValidate(() => !condition());
        });

        public static BTLeaf WaitWhile(Func<bool> condition) => WaitWhile("Wait While", condition);
    }
}