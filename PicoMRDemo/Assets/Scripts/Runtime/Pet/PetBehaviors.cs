/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Pathfinding;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Service;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PicoMRDemo.Runtime.Runtime.Pet
{
    public enum PetAnimationType
    {
        Idle = 0,
        Move = 1,
        PickBall = 2,
        MoveWithBall = 3,
        Shame = 4
    }
    public class PetBehaviors : MonoBehaviour
    {
        AIPath Agent;
        AstarPath AstarPath;

        private readonly float MainTargetEndDis = 1.0f;
        private readonly float ItemEndDis = 0.3f;
        private readonly float PatrolEndDis = 0.2f;

        public Animator Animator;

        [HideInInspector]
        public bool IsVirtualWorld;

        [HideInInspector]
        public bool HasInVirtualWorld;

        public Transform MainTarget { get; private set; }
        public Transform TempTarget { get; set; }
        public Vector3 PatrolTargetPosition { get; set; }
        public ICatchableManager CatchableManager { get; set; }
        public IVirtualWorldManager VirtualWorldManager { get; set; }
        
        public HashSet<GraphNode> OriginWalkableNodes { get; } = new HashSet<GraphNode>();
        public HashSet<GraphNode> ExtendWalkableNodes { get; } = new HashSet<GraphNode>();

        private List<GraphNode> _originWalkableNodeList;
        private List<GraphNode> OriginWalkableNodeList
        {
            get
            {
                if (_originWalkableNodeList == null)
                {
                    _originWalkableNodeList = OriginWalkableNodes.ToList();
                }

                return _originWalkableNodeList;
            }
        }

        private List<GraphNode> _extendwalkableNodeList;

        private List<GraphNode> ExtendWalkableNodeList
        {
            get
            {
                if (_extendwalkableNodeList == null)
                {
                    _extendwalkableNodeList = ExtendWalkableNodes.ToList();
                }

                return _extendwalkableNodeList;
            }
        }

        private readonly string TAG = nameof(PetBehaviors);

        public event Action<Transform> ReachedTempTarget;
        public event Action ReachedMainTarget;
        public event Action<Transform> ReachedTempTargetEnd;
        public event Action ReachedMainTargetEnd;

        public event Action ReachedPatrolTarget;

        public event Action ReachedVritualWorld;
        public event Action ReachedRealWorld;

        private void Awake()
        {
            Agent = GetComponent<AIPath>();
            AstarPath = FindObjectOfType<AstarPath>();
            MainTarget = Camera.main.transform;
            InitWalkableNodes();
        }

        public void Register()
        {
            CatchableManager.OnSetCatchable += (catchable =>
            {
                if (TempTarget == null)
                {
                    TempTarget = catchable.GameObject.transform;
                }
            });
            VirtualWorldManager.OnOpenWorldFinished += (List<GraphNode> changedNodes) =>
            {
                // 进入virtual world
                InitExtendWalkableNodes(changedNodes);
                Debug.unityLogger.Log(TAG, $"进入virtual world");
                IsVirtualWorld = true;
                return UniTask.CompletedTask;
            };
            VirtualWorldManager.OnCloseWorldStart += async () =>
            {
                // 离开virtual world
                Debug.unityLogger.Log(TAG, $"离开virtual world");
                IsVirtualWorld = false;
                while (HasInVirtualWorld)
                {
                    await UniTask.Yield();
                }
            };
        }

        public async UniTask TrackTempTarget()
        {
            Agent.endReachedDistance = ItemEndDis;
            Agent.destination = TempTarget.position;
            Agent.SearchPath();
            var destinationSetter = GetComponent<AIDestinationSetter>();
            if (destinationSetter == null)
            {
                destinationSetter = gameObject.AddComponent<AIDestinationSetter>();
            }

            destinationSetter.target = TempTarget;
            destinationSetter.enabled = true;

            while (Vector3.Distance(Agent.transform.position, TempTarget.position) > ItemEndDis)
            {
                await UniTask.Yield();
            }
            
            destinationSetter.enabled = false;
            ReachedTempTarget?.Invoke(TempTarget);

            TempTarget = null;
        }

        public async UniTask TrackVirtualWorld()
        {
            Agent.enabled = false;
            Agent.enabled = true;
            Agent.endReachedDistance = PatrolEndDis;
            Agent.destination = GetExtendWalkablePosition();
            Agent.SearchPath();

            while (!Agent.reachedEndOfPath)
            {
                await UniTask.Yield();
            }
            ReachedVritualWorld?.Invoke();
        }
        
        
        
        public async UniTask TrackRealWorld()
        {
            Agent.enabled = false;
            Agent.enabled = true;
            Agent.endReachedDistance = PatrolEndDis;
            Agent.destination = GetOriginWalkablePosition();
            Agent.SearchPath();

            while (!Agent.reachedEndOfPath)
            {
                await UniTask.Yield();
            }
            ReachedRealWorld?.Invoke();
        }

        public async UniTask TrackMainTarget()
        {
            Agent.enabled = false;
            Agent.enabled = true;
            Agent.endReachedDistance = MainTargetEndDis;
            Agent.destination = MainTarget.position;
            Agent.SearchPath();

            while (!Agent.reachedEndOfPath)
            {
                await UniTask.Yield();
            }

            if (Agent.reachedDestination)
            {
                ReachedMainTarget?.Invoke();
            }
            else
            {
                ReachedMainTargetEnd?.Invoke();
            }
        }

        public async UniTask TrackPatrolTarget()
        {
            Agent.endReachedDistance = PatrolEndDis;
            Agent.destination = GetWalkablePosition();
            Agent.SearchPath();

            while (!Agent.reachedEndOfPath)
            {
                await UniTask.Yield();
            }
            
            ReachedPatrolTarget?.Invoke();
        }
        public bool HasTempTarget => TempTarget != null;

        public bool IsWalkFinished()
        {
            return !Agent.hasPath || Agent.reachedEndOfPath;
        }

        public async UniTask StartIdle()
        {
            _isIdle = true;
            await UniTask.Delay(3000);
            _isIdle = false;
        }

        private bool _isIdle = false;
        public bool IsIdleFinished()
        {
            return !_isIdle;
        }

        private void InitWalkableNodes()
        {
            AstarPath.active.data.GetNodes((node) =>
            {
                if (node.Walkable)
                    OriginWalkableNodes.Add(node);
            });
        }

        public void InitExtendWalkableNodes(List<GraphNode> changedNodes)
        {
            if (changedNodes.Count > 0)
            {
                foreach (var tempNode in changedNodes)
                {
                    if (!OriginWalkableNodes.Contains(tempNode))
                    {
                        ExtendWalkableNodes.Add(tempNode);
                    }
                }
            }
        }

        private Vector3 GetWalkablePosition()
        {
            if (IsVirtualWorld)
            {
                return GetExtendWalkablePosition();
            }

            return GetOriginWalkablePosition();
        }
        private Vector3 GetOriginWalkablePosition()
        {
            if (AstarPath == null)
            {
                return Vector3.zero;
            }
            // List<GraphNode> walkableNodes = new List<GraphNode>();
            // AstarPath.data.GetNodes((node) =>
            // {
            //     if (node.Walkable)
            //     {
            //         walkableNodes.Add(node);
            //     }
            // });
            Vector3 result = Vector3.zero;
            if (OriginWalkableNodeList.Count > 0)
            {
                var node = OriginWalkableNodeList[Random.Range(0, OriginWalkableNodeList.Count)];
                result = (Vector3)node.position;
            }
            return result;
        }

        private Vector3 GetExtendWalkablePosition()
        {
            if (AstarPath == null)
            {
                return Vector3.zero;
            }
            Vector3 result = Vector3.zero;
            if (ExtendWalkableNodeList.Count > 0)
            {
                var node = ExtendWalkableNodeList[Random.Range(0, ExtendWalkableNodeList.Count)];
                result = (Vector3)node.position;
            }
            return result;
        }
        
        public void PlayAnimation(PetAnimationType animationType)
        {
            var animId = (int)animationType;
            Animator?.SetInteger("state", animId);
        }
        
        public async UniTask PlayAnimationAwait(PetAnimationType animationType, CancellationToken cancellationToken)
        {
            if (!Animator) return;
            var animId = (int)animationType;
            Animator.SetInteger("state", animId);
            var animatorStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            var animName = animatorStateInfo.fullPathHash;
            var initLoop = (int)animatorStateInfo.normalizedTime;
            await UniTask.WaitUntil(() =>
            {
                animatorStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                var loop = (int)animatorStateInfo.normalizedTime;
                return loop > initLoop || animName != animatorStateInfo.fullPathHash;
            }, cancellationToken: cancellationToken);
        }
    }
}