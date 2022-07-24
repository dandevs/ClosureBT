using System;

namespace ClosureBT {
    public static partial class BT {
        /// <summary> Execute function </summary>
        public static BTLeaf Do(string name, Action action) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => {
                action();
                return BT.Status.Success;
            });
        });

        /// <summary> Execute function when condition is true </summary>
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

        /// <summary> Execute function every tick (never stops) </summary>
        public static BTLeaf DoAlwaysOnTick(string name, Action action) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => {
                action();
                return BT.Status.Running;
            });
        });

        public static BTLeaf DoAlwaysOnTick(Action action) => DoAlwaysOnTick("Do Always", action);

        //----------------------------------------------------------------------------------------------------------------------

        public static BTLeaf ReturnSuccess(string name, Action lifecycle = null) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => BT.Status.Success);
            lifecycle?.Invoke();
        });

        public static BTLeaf ReturnFailure(string name, Action lifecycle = null) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => BT.Status.Failure);
            lifecycle?.Invoke();
        });

        public static BTLeaf ReturnRunning(string name, Action lifecycle = null) => new BTLeaf(name, () => {
            BT.OnBaseTick(() => BT.Status.Running);
            lifecycle?.Invoke();
        });

        public static BTLeaf ReturnSuccess(Action lifecycle = null) => ReturnSuccess("Return Success", lifecycle);
        public static BTLeaf ReturnFailure(Action lifecycle = null) => ReturnFailure("Return Failure", lifecycle);
        public static BTLeaf ReturnRunning(Action lifecycle = null) => ReturnRunning("Return Running", lifecycle);

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