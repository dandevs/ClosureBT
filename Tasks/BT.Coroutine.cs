using System;
using System.Collections;
using UnityEngine;

namespace ClosureBT {
    public static partial class BT {
        public static BTLeaf Coroutine(string name, MonoBehaviour author, Func<IEnumerator> coroutineFunc) => new BTLeaf(name, () => {
            Coroutine coroutine = null;
            var done = false;

            BT.OnEnter(() => done = false);
            BT.OnExit(() => {
                if (coroutine != null) {
                    author.StopCoroutine(coroutine);
                    coroutine = null;
                }
            });

            IEnumerator WrapperRoutine() {
                yield return author.StartCoroutine(coroutineFunc());
                done = true;
            }

            BT.OnBaseTick(() => {
                if (coroutine == null)
                    coroutine = author.StartCoroutine(WrapperRoutine());

                return done ? BT.Status.Success : BT.Status.Running;
            });
        });

        public static BTLeaf Coroutine(MonoBehaviour author, Func<IEnumerator> coroutineFunc) => Coroutine("Coroutine", author, coroutineFunc);
    }
}