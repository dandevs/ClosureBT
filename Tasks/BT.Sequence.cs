using System;

namespace ClosureBT {
    public static partial class BT {
        /// <summary> Same as predicate() && predicate() && predicate() </summary>
        public static BTComposite Sequence(string name, Action nodes) => new BTComposite(name, () => {
            var self = BT.current as BTComposite;
            var lastRunningIndex = 0;

            BT.OnEnter(() => lastRunningIndex = 0);
            
            BT.OnBaseTick(() => {
                var startingIndex = self.dynamic ? 0 : lastRunningIndex;

                for (var i = startingIndex; i < self.children.Count; i++) {
                    var status = self.children[i].Tick();

                    switch (status) {
                        case BT.Status.Running:
                        case BT.Status.Failure:
                            if (self.dynamic)
                                for (var j = i + 1; j <= lastRunningIndex; j++)
                                    self.children[j].Reset();

                            lastRunningIndex = i;

                            // if (status == BT.Status.Failure && self.repeating)
                            //     return BT.Status.Running;

                            return status;
                    }
                }

                if (self.repeating && self.children.Count > 0)
                    return self.ResetAndTick();
                
                return BT.Status.Success;
            });
            
            BT.OnValidate(() => {
                if (!self.dynamic && !self.repeating)
                    return true;

                for (var i = 0; i <= lastRunningIndex; i++)
                    if (!self.children[i].CheckValidity())
                        return false;

                return true;
            });

            nodes();
        });

        public static BTComposite Sequence(Action nodes) => Sequence("Sequence", nodes);
    }
}