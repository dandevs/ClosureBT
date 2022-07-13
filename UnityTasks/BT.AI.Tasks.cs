using System;
using UnityEngine;
using UnityEngine.AI;

namespace ClosureBT {
    public static partial class BT {
        public static partial class AI {
            public static BTLeaf MoveTo(NavMeshAgent navagent, float stoppingDistance, Func<Vector3> destination) => new BTLeaf("NavMeshAgent Move To", () => {
                BT.OnBaseTick(() => {
                    navagent.stoppingDistance = stoppingDistance;
                    navagent.destination = destination();

                    if (!navagent.hasPath)
                        return BT.Status.Running;
                    else
                        return navagent.remainingDistance <= navagent.stoppingDistance ? BT.Status.Success : BT.Status.Running;
                });

                BT.OnValidate(() => {
                    navagent.destination = destination();
                    return navagent.hasPath && navagent.remainingDistance > navagent.stoppingDistance ? false : true;
                });
            });

            public static BTLeaf MoveTo(NavMeshAgent navagent, Func<Vector3> destination) => MoveTo(navagent, 0.03f, destination);
        }
    }
}