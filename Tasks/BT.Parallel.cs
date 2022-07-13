using System;

namespace ClosureBT {
    public static partial class BT {
        public static BTComposite Parallel(string name, Action nodes) => new BTComposite(name, () => {
            var self = BT.current as BTComposite;
            var children = self.children;

            BT.OnBaseTick(() => {
                if (children.Count == 0)
                    return BT.Status.Success;

                var successes = 0;
                var fails = 0;

                for (var i = 0; i < children.Count; i++) {
                    var status = children[i].Tick(false);

                    switch (status) {
                        case Status.Success: successes++; break;
                        case Status.Failure: fails++; break;
                    }
                }

                if (self.repeating && (successes == children.Count || fails == children.Count))
                    return self.ResetAndTick();

                if (successes == children.Count)
                    return Status.Success;

                if (fails == children.Count)
                    return Status.Failure;

                return BT.Status.Running;
            });

            BT.OnValidate(() => {
                for (var i = 0; i < self.children.Count; i++)
                    if (self.children[i].CheckValidity() == false)
                        return false;

                return true;
            });
        });

        public static BTComposite Parallel(Action nodes) => Parallel("Parallel", nodes);
    }
}