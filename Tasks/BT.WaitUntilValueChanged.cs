using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf WaitUntilValueChanged<T>(string name, Func<T> value) where T : IEquatable<T> => new BTLeaf(name, () => {
            var changed   = false;
            T   prevValue = value();

            BT.OnBaseTick(() => {
                if (changed) {
                    changed = false;
                    return BT.Status.Success;
                }

                var curValue = value();
                changed = !curValue.Equals(prevValue);

                if (changed) {
                    changed = false;
                    prevValue = curValue;
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

        public static BTLeaf WaitUntilValueChanged<T>(Func<T> value) where T : IEquatable<T> => WaitUntilValueChanged("Wait Until Value Changed", value);
    }
}