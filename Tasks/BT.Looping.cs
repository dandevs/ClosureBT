using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTComposite LoopingSequence(string name, Action nodes) => new BTComposite("Looping", () => {
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
                            return status;
                    }
                }
                
                for (var i = 0; i < self.children.Count; i++)
                    self.children[i].Reset();

                lastRunningIndex = 0;
                return BT.Status.Running;
            });

            nodes();
        });

        public static BTComposite LoopingSequence(Action nodes) => LoopingSequence("Looping Sequence", nodes);
    }
}