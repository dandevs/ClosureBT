using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf Condition(string name, Func<bool> condition) => BT.Leaf(name, () => {
            var lastStatus = false;
            BT.OnBaseTick(() => (lastStatus = condition()) ? BT.Status.Success : BT.Status.Failure);
            BT.OnValidate(() => condition() == lastStatus);
        });

        public static BTLeaf Condition(Func<bool> condition) => Condition("Condition", condition);

        //----------------------------------------------------------------------------------------------------------------------

        public static partial class D {
            public static BTDecorator Condition(string name, Func<bool> condition) => BT.Decorator(name, () => {
                var self = BT.current as BTDecorator;
                var lastStatus = false;

                BT.OnBaseTick(() => {
                    lastStatus = condition();

                    if (lastStatus)
                        return self.child.Tick();
                    else {
                        self.child.Reset();
                        return BT.Status.Failure;
                    }
                });

                BT.OnValidate(() => {
                    var ok = condition();

                    if (ok && !self.child.CheckValidity())
                        return false;

                    return ok == lastStatus;
                });
            });
        
            public static BTDecorator Condition(Func<bool> condition) => D.Condition("Condition", condition);

            //----------------------------------------------------------------------------------------------------------------------
            
            public static BTDecorator FailIf(string name, Func<bool> condition) => BT.Decorator(name, () => {
                var self = BT.current as BTDecorator;
                var lastStatus = false;

                BT.OnBaseTick(() => {
                    lastStatus = condition();

                    if (lastStatus) {
                        self.child.Reset();
                        return BT.Status.Failure;
                    }
                    else
                        return self.child.Tick();
                });

                BT.OnValidate(() => {
                    var ok = condition();

                    if (!ok && !self.child.CheckValidity())
                        return false;

                    return ok == lastStatus;
                });
            });

            public static BTDecorator FailIf(Func<bool> condition) => D.FailIf("Fail If", condition);

            //----------------------------------------------------------------------------------------------------------------------
            
            public static void ConditionSuccess(string name, Func<bool> condition) {
                BT.D.AlwaysSucceed();
                BT.D.Condition(name, condition);
            }

            public static void ConditionFail(string name, Func<bool> condition) {
                BT.D.FailOnFinish();
                BT.D.Condition(name, condition);
            }

            public static void ConditionSuccess(Func<bool> condition) => ConditionSuccess("Condition (Succeed Always)", condition);
            public static void ConditionFail(Func<bool> condition) => ConditionFail("Condition (Fail Always)", condition);

            //----------------------------------------------------------------------------------------------------------------------

            public static BTDecorator ConditionLocking(string name, Func<bool> condition) => BT.Decorator(name, () => {
                var self = BT.current as BTDecorator;
                var lastStatus = false;
                var locked = false;

                BT.OnEnter(() => locked = false);

                BT.OnBaseTick(() => {
                    if (!locked && (lastStatus = condition()))
                        locked = true;

                    if (!lastStatus && !locked)
                        self.child.Reset();
                    
                    if (locked)
                        return self.child.Tick();

                    return BT.Status.Failure;
                });

                BT.OnValidate(() => {
                    var ok = condition();

                    if (ok && !self.child.CheckValidity())
                        return false;
                    
                    return ok == lastStatus;
                });
            });

            public static BTDecorator ConditionLocking(Func<bool> condition) => D.ConditionLocking("Condition (Locking)", condition);

            //----------------------------------------------------------------------------------------------------------------------

            public static BTDecorator TakeWhile(string name, Func<bool> condition) => BT.Decorator(name, () => {
                var self = BT.current as BTDecorator;
                var lastStatus = false;

                BT.OnBaseTick(() => {
                    if (lastStatus = condition())
                        return self.child.Tick();

                    return BT.Status.Failure;
                });

                BT.OnValidate(() => {
                    var ok = condition();

                    if (ok && !self.child.CheckValidity())
                        return false;

                    return ok == lastStatus;
                });
            });

            public static BTDecorator TakeWhile(Func<bool> condition) => D.TakeWhile("Take While", condition);
            
            //----------------------------------------------------------------------------------------------------------------------

            public static BTDecorator TakeUntil(string name, Func<bool> condition) => BT.Decorator(name, () => {
                var self = BT.current as BTDecorator;
                var lastStatus = false;

                BT.OnBaseTick(() => {
                    if (lastStatus = condition())
                        return BT.Status.Failure;

                    return self.child.Tick();
                });

                BT.OnValidate(() => {
                    var ok = condition();

                    if (ok && !self.child.CheckValidity())
                        return false;

                    return ok == lastStatus;
                });
            });

            public static BTDecorator TakeUntil(Func<bool> condition) => D.TakeUntil("Take Until", condition);

            //----------------------------------------------------------------------------------------------------------------------

            public static BTDecorator AlwaysSucceed() => BT.Decorator("Always Succeed", () => {
                var self = BT.current as BTDecorator;

                BT.OnBaseTick(() => {
                    if (self.child.Tick() == BT.Status.Running)
                        return BT.Status.Running;
                    else
                        return BT.Status.Success;
                });

                BT.OnValidate(() => self.child.CheckValidity());
            });

            public static BTDecorator FailOnFinish() => BT.Decorator("Always Fail", () => {
                var self = BT.current as BTDecorator;

                BT.OnBaseTick(() => {
                    if (self.child.Tick() == BT.Status.Running)
                        return BT.Status.Running;
                    else
                        return BT.Status.Failure;
                });

                BT.OnValidate(() => self.child.CheckValidity());
            });
        }
    }
}