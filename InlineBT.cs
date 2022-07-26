using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClosureBT {
    public static partial class BT {
        private static Dictionary<Func<BTNode>, BTInlineInfo> inlineBTInfo = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ClearInlineBTCache() {
            inlineBTInfo.Clear();
            GameObject.DontDestroyOnLoad(new GameObject("BT Inline Manager").AddComponent<BTInlineManager>());
        }

        public static bool InlineBT(this Component source, Func<BTNode> func) {
            // TryGetValue causes 24b allocation :(
            if (!inlineBTInfo.ContainsKey(func)) {
                inlineBTInfo[func] = new BTInlineInfo(func());
                
                if (!source.gameObject.TryGetComponent<BTInlineOnDestroy>(out var inlineOnDestroy))
                    inlineOnDestroy = source.gameObject.AddComponent<BTInlineOnDestroy>();

                inlineOnDestroy.onDestroyDelegate += () => inlineBTInfo.Remove(func);
            }

            var info          = inlineBTInfo[func];
                info.lastTick = Time.frameCount;
                info.running  = true;

            return info.bt.Tick() == BT.Status.Success ? true : false;
        }

        private class BTInlineManager : MonoBehaviour {
            private void LateUpdate() {
                foreach (var (_, info) in inlineBTInfo) {
                    if (!info.running)
                        continue;

                    var d = Time.frameCount - info.lastTick;

                    if (d > 0 || d < 0) {
                        info.running = false;
                        info.bt.Reset();
                    }
                }
            }
        }

        private class BTInlineOnDestroy : MonoBehaviour {
            public Action onDestroyDelegate = delegate { };

            private void OnDestroy() {
                onDestroyDelegate?.Invoke();
            }
        }

        private class BTInlineInfo {
            public BTNode bt;
            public int lastTick;
            public bool running = false;

            public BTInlineInfo(BTNode bt) {
                this.bt = bt;
            }
        }
    }
}