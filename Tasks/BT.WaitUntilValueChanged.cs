using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf WaitUntilValueChanged<T>(string name, bool leading, Func<T> value) where T : IEquatable<T> => new BTLeaf(name, () => {
            var leadingCalled = false;
            var changed       = false;
            T   prevValue     = default;

            BT.OnBaseTick(() => {
                if (changed) {
                    changed = false;
                    return BT.Status.Success;
                }

                var curValue = value();
                changed = !curValue.Equals(prevValue);

                if (changed || (!leadingCalled && leading)) {
                    changed = false;
                    prevValue = curValue;

                    if (leading)
                        leadingCalled = true;

                    return BT.Status.Success;
                }
                else
                    return BT.Status.Running;
            });

            BT.OnValidate(() => {
                if (!value().Equals(prevValue)) {
                    changed = true;
                    return false;
                }
                else
                    return true;
            });
        });

        public static BTLeaf WaitUntilValueChanged<T>(bool leading, Func<T> value) where T : IEquatable<T> => WaitUntilValueChanged("Wait Until Value Changed", leading, value);
        public static BTLeaf WaitUntilValueChanged<T>(Func<T> value) where T : IEquatable<T> => WaitUntilValueChanged("Wait Until Value Changed", false, value);
    }
}