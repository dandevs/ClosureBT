using System;
using UnityEngine;
using UnityEngine.AI;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf Wait(string name, double seconds) => new BTLeaf(name, () => {
            var start = double.MinValue;
            BT.OnEnter(() => start = Time.timeAsDouble);
            BT.OnBaseTick(() => {
                return Time.timeAsDouble > start + seconds 
                    ? BT.Status.Success 
                    : BT.Status.Running;
            });
        });

        public static BTLeaf Wait(double seconds) => Wait("Wait", seconds);

        //----------------------------------------------------------------------------------------------------------------------

        public static BTLeaf WaitTicks(string name, int count) => new BTLeaf(name, () => {
            var ticks = 0;
            BT.OnEnter(() => ticks = 0);
            BT.OnBaseTick(() => ticks++ < count ? BT.Status.Running : BT.Status.Success);
        });

        public static BTLeaf WaitTicks(int count) => WaitTicks("Wait Ticks", count);

        //----------------------------------------------------------------------------------------------------------------------
        

        public static BTLeaf WaitUntilFrameEvent<T>(string channel, UnityEngine.Object objectChannel, out Func<T> readEvent, Action lifecycle = null) {
            T value = default;
            readEvent = () => value;

            return new BTLeaf("Wait Until Frame Event", () => {
                var force = false;
                
                BT.OnBaseTick(() => {
                    if (force) {
                        force = false;
                        return BT.Status.Success;
                    }

                    if (FrameEvent.When<T>(channel, objectChannel, out var e)) {
                        value = e;
                        return BT.Status.Success;
                    }
                    else
                        return BT.Status.Running;
                });

                BT.OnValidate(() => {
                    if (FrameEvent.When<T>(channel, objectChannel, out var e)) {
                        force = true;
                        value = e;
                        return false;
                    }
                    else
                        return true;
                });

                lifecycle?.Invoke();
            });
        }
        
        public static BTLeaf WaitUntilFrameEvent<T>(string channel, out Func<T> readEvent, Action lifecycle = null)
            => WaitUntilFrameEvent<T>(channel, null, out readEvent, lifecycle);

        public static BTLeaf WaitUntilFrameEvent<T>(UnityEngine.Object objectChannel, out Func<T> readEvent, Action lifecycle = null)
            => WaitUntilFrameEvent<T>(null, objectChannel, out readEvent, lifecycle);

        public static BTLeaf WaitUntilFrameEvent<T>(string channel, UnityEngine.Object objectChannel, Action lifecycle = null)
            => WaitUntilFrameEvent<T>(channel, objectChannel, out var _, lifecycle);

        public static BTLeaf WaitUntilFrameEvent<T>(UnityEngine.Object objectChannel, Action lifecycle = null)
            => WaitUntilFrameEvent<T>(null, objectChannel, out var _, lifecycle);

        public static BTLeaf WaitUntilFrameEvent<T>(string channel, Action lifecycle = null)
            => WaitUntilFrameEvent<T>(channel, null, out var _, lifecycle);

        public static BTLeaf WaitUntilFrameEvent(string channel, UnityEngine.Object objectChannel, Action lifecycle = null)
            => WaitUntilFrameEvent<FrameEvent.DefaultType>(channel, objectChannel, out var _, lifecycle);

        public static BTLeaf WaitUntilFrameEvent(UnityEngine.Object objectChannel, Action lifecycle = null)
            => WaitUntilFrameEvent<FrameEvent.DefaultType>(null, objectChannel, out var _, lifecycle);

        //----------------------------------------------------------------------------------------------------------------------

        public static partial class D {
            public static BTDecorator OnFrameEvent<T>(string channel, UnityEngine.Object objectChannel, out Func<T> readEvent) {
                T value = default;
                var name = channel ?? objectChannel?.name ?? "On Frame Event";
                readEvent = () => value;
                
                return new BTDecorator($"On Frame Event {name}", () => {
                    var self = BT.current as BTDecorator;
                    var running = false;
                    var force = false;
                    var wasFrameEvent = false;

                    BT.OnEnter(() => running = false);

                    BT.OnBaseTick(() => {
                        if (!force && (wasFrameEvent = FrameEvent.When<T>(channel, objectChannel, out var e)))
                            value = e;

                        if (force || wasFrameEvent) {
                            force = false;
                            running = true;
                            self.child.Reset();
                        }

                        if (running) 
                            return self.child.Tick();
                        else
                            return BT.Status.Failure;
                    });

                    BT.OnValidate(() => {
                        if (FrameEvent.When<T>(channel, objectChannel, out var e)) {
                            value = e;
                            force = true;
                            return false;
                        }
                        else
                            return true;
                    });
                });
            }

            public static BTDecorator OnFrameEvent<T>(string channel, out Func<T> readEvent) => OnFrameEvent<T>(channel, null, out readEvent);
            public static BTDecorator OnFrameEvent<T>(UnityEngine.Object objectChannel, out Func<T> readEvent) => OnFrameEvent<T>(null, objectChannel, out readEvent);
            public static BTDecorator OnFrameEvent<T>(out Func<T> readEvent) => OnFrameEvent<T>(null, null, out readEvent);
        }
    }
}