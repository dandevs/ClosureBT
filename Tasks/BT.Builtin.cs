using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf Do(string name, Action action) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => {
                action();
                return BT.Status.Success;
            });
        });


        public static BTLeaf Do(string name, Func<bool> condition, Action action) => new BTLeaf(name, () => {
            var lastStatus = false;

            BT.OnBaseTick(() => {
                if (lastStatus = condition()) {
                    action();
                    return BT.Status.Success;
                }
                else
                    return BT.Status.Running;
            });

            BT.OnValidate(() => condition() == lastStatus);
        });

        public static BTLeaf Do(Action action) => Do("Do", action);
        public static BTLeaf Do(Func<bool> condition, Action action) => Do("Do", condition, action);
        
        //----------------------------------------------------------------------------------------------------------------------

        public static BTLeaf DoAlways(string name, Action action) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => {
                action();
                return BT.Status.Running;
            });
        });

        public static BTLeaf DoAlways(Action action) => DoAlways("Do Always", action);

        //----------------------------------------------------------------------------------------------------------------------

        public static BTLeaf ReturnSuccess(string name) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => BT.Status.Success);
        });

        public static BTLeaf ReturnFailure(string name) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => BT.Status.Failure);
        });

        public static BTLeaf ReturnRunning(string name) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => BT.Status.Running);
        });

        public static BTLeaf ReturnSuccess() => ReturnSuccess("Return Success");
        public static BTLeaf ReturnFailure() => ReturnFailure("Return Failure");
        public static BTLeaf ReturnRunning() => ReturnRunning("Return Running");

        //----------------------------------------------------------------------------------------------------------------------
        
        public static BTLeaf Action(string name, Action action) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => {
                action();
                return BT.Status.Running;
            });
        });

        public static BTLeaf Action(Action action) => Action("Action", action);

        //----------------------------------------------------------------------------------------------------------------------

        public static partial class D {
            public static BTDecorator Do(string name, Action action) => new BTDecorator(name, () => {
                var self = BT.current as BTDecorator;
                var executed = false;

                BT.OnEnter(() => executed = false);

                BT.OnBaseTick(() => {
                    if (!executed) {
                        executed = true;
                        action();
                    }

                    return self.child.Tick();
                });

                BT.OnValidate(() => self.child.CheckValidity());
            });
        }
    }
}