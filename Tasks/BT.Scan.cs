using System;

namespace ClosureBT {
    public static partial class BT {
        /// <summary>
        /// Sequentially go through all nodes ignoring success or failure.
        /// Returns success once all nodes have been executed.
        /// </summary>
        public static BTComposite Scan(string name, Action nodes) => new BTComposite(name, () => {
            var self = BT.current as BTComposite;
            var children = self.children;
            var lastRunningIndex = 0;

            BT.OnEnter(() => lastRunningIndex = 0);

            BT.OnBaseTick(() => {
                var startingIndex = self.dynamic ? 0 : lastRunningIndex;

                for (var i = startingIndex; i < children.Count; i++) {
                    var status = children[i].Tick();

                    if (status == BT.Status.Running) {
                        lastRunningIndex = i;
                        return BT.Status.Running;
                    }
                }

                // if (self.repeating && self.children.Count > 0)
                //     return self.ResetAndTick();

                return BT.Status.Success;
            });

            BT.OnValidate(() => {
                if (!self.dynamic && !self.repeating)
                    return true;

                for (var i = 0; i < children.Count; i++)
                    if (!self.children[i].CheckValidity())
                        return false;

                return true;
            });

            nodes();
        });

        public static BTComposite Scan(Action nodes) => Scan("Scan", nodes);
    }
}