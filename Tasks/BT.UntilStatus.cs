using System;

namespace ClosureBT {
    public static partial class BT {
        private static BTComposite WaitUntilStatus(string name, BT.Status finalStatus, Action nodes) => new BTComposite("Wait Until Status", () => {
            var self = BT.current as BTComposite;
            var runningIndex = 0;

            BT.OnEnter(() => runningIndex = -1);

            BT.OnBaseTick(() => {
                if (runningIndex != -1)  {
                    var status = self.children[runningIndex].Tick();

                    if (status == finalStatus)
                        return finalStatus;
                    else if (status != BT.Status.Running)
                        runningIndex = -1;
                }

                if (runningIndex == -1) {
                    for (var i = 0; i < self.children.Count; i++) {
                        var status = self.children[i].Tick();

                        if (status == finalStatus)
                            return finalStatus;
                        else if (status == BT.Status.Running) {
                            runningIndex = i;
                            break;
                        }
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

        public static BTComposite WaitUntilAnySucceed(string name, Action nodes) => WaitUntilStatus(name, BT.Status.Success, nodes);
        public static BTComposite WaitUntilAnySucceed(Action nodes) => WaitUntilStatus("Scan Until Success", BT.Status.Success, nodes);

        public static BTComposite WaitUntilAnyFail(string name, Action nodes) => WaitUntilStatus(name, BT.Status.Failure, nodes);
        public static BTComposite WaitUntilAnyFail(Action nodes) => WaitUntilStatus("Scan Until Failure", BT.Status.Failure, nodes);
    }
}