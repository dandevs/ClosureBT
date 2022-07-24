using System;

namespace ClosureBT {
    public static partial class BT {
        /// <summary> Switch to node whose status just turned to Status.Running </summary>
        public static BTComposite RunningSwitch(string name, Action nodes) => new BTComposite(name, () => {
            var self = BT.current as BTComposite;
            var lastRunningIndex = 0;

            BT.OnEnter(() => lastRunningIndex = 0);

            BT.OnBaseTick(() => {
                for (var i = 0; i < self.children.Count; i++) {
                    var status = self.children[i].Tick();

                    if (status == BT.Status.Running) {
                        if (lastRunningIndex != i)
                            self.children[lastRunningIndex].Reset();

                        lastRunningIndex = i;
                    }
                }

                return BT.Status.Running;
            });

            BT.OnValidate(() => {
                for (var i = 0; i < self.children.Count; i++)
                    if (!self.children[i].CheckValidity())
                        return false;

                return true;
            });

            nodes();
        });

        public static BTComposite RunningSwitch(Action nodes) => RunningSwitch("Running Switch", nodes);
    }
}