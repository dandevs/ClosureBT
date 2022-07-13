using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTComposite Selector(string name, Action nodes) => new BTComposite(name, () => {
            var self = BT.current as BTComposite;
            var lastRunningIndex = 0;
            var anyPass = false;

            BT.OnEnter(() => lastRunningIndex = 0);
            
            BT.OnBaseTick(() => {
                var startingIndex = self.dynamic ? 0 : lastRunningIndex;
                anyPass = false;

                for (var i = startingIndex; i < self.children.Count; i++) {
                    var status = self.children[i].Tick();

                    switch (status) {
                        case BT.Status.Running:
                        case BT.Status.Success:
                            if (self.dynamic)
                                for (var j = i + 1; j <= lastRunningIndex; j++)
                                    self.children[j].Reset();

                            lastRunningIndex = i;
                            anyPass = true;

                            if (status == BT.Status.Success && self.repeating)
                                return BT.Status.Running;

                            return status;
                    }
                }

                if (self.repeating && self.children.Count > 0)
                    return self.ResetAndTick();

                return BT.Status.Failure;
            });

            BT.OnValidate(() => {
                if (!self.dynamic && !self.repeating)
                    return true;

                var maxIndex = anyPass ? lastRunningIndex + 1 : self.children.Count;

                for (var i = 0; i < maxIndex; i++)
                    if (!self.children[i].CheckValidity())
                        return false;

                return true;
            });

            nodes();
        });

        public static BTComposite Selector(Action nodes) => Selector("Selector", nodes);
    }
}