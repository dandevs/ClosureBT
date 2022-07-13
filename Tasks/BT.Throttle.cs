using System;
using UnityEngine;

namespace ClosureBT {
    public static partial class BT {
        public static partial class D {
            public static BTDecorator Throttle(string name, double seconds, bool leading = true) => new BTDecorator(name, () => {
                var self = BT.current as BTDecorator;
                var timeForNextExecute = 0d;

                BT.OnEnter(() => timeForNextExecute = leading ? 0d : Time.timeAsDouble + seconds);
                BT.OnBaseTick(() => {
                    if (Time.timeAsDouble > timeForNextExecute) {
                        timeForNextExecute = Time.timeAsDouble + seconds;
                        return self.child.Tick();
                    }

                    return BT.Status.Running;
                });
            });

            public static BTDecorator Throttle(double seconds, bool leading = true) => Throttle("Throttle", seconds, leading);
        }
    }
}