using System;

namespace ClosureBT {
    public static partial class BT {
        public static partial class D {
            private static BTDecorator FailIfStatus(BT.Status status, BTNode bt) => new BTDecorator($"Fail If {status}", () => {
                var self = BT.current as BTDecorator;
                BT.OnExit(() => bt.Reset());

                BT.OnBaseTick(() => {
                    var btStatus = bt.Tick();

                    if (btStatus == status) {
                        bt.Reset();
                        self.child.Reset();
                        return BT.Status.Failure;
                    }

                    return btStatus == BT.Status.Running ? BT.Status.Running : BT.Status.Success;
                });
            });

            public static BTDecorator FailIfSuccess(BTNode bt) => FailIfStatus(BT.Status.Success, bt);
            public static BTDecorator FailIfFailure(BTNode bt) => FailIfStatus(BT.Status.Failure, bt);
        }
    }
}