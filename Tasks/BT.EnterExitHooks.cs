using System;

namespace ClosureBT {
    public static partial class BT {
        public static partial class D {
            public static BTDecorator OnEnter(string name, Action onEnter) => new BTDecorator(name, () => {
                var self = BT.current as BTDecorator;
                BT.OnBaseTick(() => self.child.Tick());
                BT.OnEnter(onEnter);
            });

            public static BTDecorator OnExit(string name, Action onExit) => new BTDecorator(name, () => {
                var self = BT.current as BTDecorator;
                BT.OnBaseTick(() => self.child.Tick());
                BT.OnExit(onExit);
            });

            public static BTDecorator OnTick(string name, Action onTick) => new BTDecorator(name, () => {
                var self = BT.current as BTDecorator;
                BT.OnBaseTick(() => { onTick(); return self.child.Tick(); });
            });

            public static BTDecorator OnEnter(Action onEnter) => OnEnter("On Enter", onEnter);
            public static BTDecorator OnExit(Action onExit) => OnExit("On Exit", onExit);
            public static BTDecorator OnTick(Action onTick) => OnTick("On Tick", onTick);
        }
    }
}