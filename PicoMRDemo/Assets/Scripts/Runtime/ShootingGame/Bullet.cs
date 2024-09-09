/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace PicoMRDemo.Runtime.Runtime.ShootingGame
{
    public class Bullet : MonoBehaviour, IBullet
    {
        public IBalloonInteractionManager BalloonInteractionManager { get; set; }
        public GameObject GameObject => gameObject;

        [FormerlySerializedAs("HitEffect")] public ParticleSystem hitEffect;
        [FormerlySerializedAs("MeshRenderer")] public MeshRenderer meshRenderer;

        public Rigidbody RigidBody
        {
            get
            {
                if (_rigidBody == null)
                {
                    _rigidBody = GameObject.GetComponent<Rigidbody>();
                }
                return _rigidBody;
            }
        }

        public float LifeTime
        {
            get => lifeTime;
            set => lifeTime = value;
        }
        
        private PhysicalCollisionDelegate _physicalCollisionDelegate;

        private PhysicalCollisionDelegate PhysicalCollisionDelegate
        {
            get
            {
                if (_physicalCollisionDelegate == null)
                {
                    _physicalCollisionDelegate = GetComponent<PhysicalCollisionDelegate>();
                }
                return _physicalCollisionDelegate;
            }
        }
        
        private Rigidbody _rigidBody;

        private readonly HashSet<IBalloon> _enterBalloons = new HashSet<IBalloon>();
        
        [FormerlySerializedAs("_lifeTime")] [SerializeField]
        private float lifeTime = 10;

        private void OnEnable()
        {
            PhysicalCollisionDelegate.AddCollisionEvent(OnCollisionEnterEvent, OnCollisionStayEvent, OnCollisionExitEvent);
        }

        private void OnDisable()
        {
            PhysicalCollisionDelegate.RemoveCollisionEvent(OnCollisionEnterEvent, OnCollisionStayEvent, OnCollisionExitEvent);
        }

        public async void PlayHit()
        {
            hitEffect.gameObject.SetActive(true);
            hitEffect.Play();
            meshRenderer.enabled = false;
            RigidBody.isKinematic = true;
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            Destroy(gameObject);
        }

        public void GetValidTargets(out IBalloon[] validTargets)
        {
            validTargets = new IBalloon[_enterBalloons.Count];
            int idx = 0;
            foreach (var enterBalloon in _enterBalloons)
            {
                validTargets[idx] = enterBalloon;
                idx++;
            }
        }

        private void Update()
        {
            if (LifeTime <= 0)
            {
                BalloonInteractionManager.RemoveBullet(this);
                Destroy(gameObject);
            }
            else
            {
                LifeTime -= Time.deltaTime;
            }
        }
        
        private void OnCollisionEnterEvent(Collision collision)
        {
            if (BalloonInteractionManager.TryGetBalloonForCollider(collision.collider, out var balloon))
            {
                _enterBalloons.Add(balloon);
            }
        }

        private void OnCollisionStayEvent(Collision collision)
        {
        }

        private void OnCollisionExitEvent(Collision collision)
        {
            if (BalloonInteractionManager.TryGetBalloonForCollider(collision.collider, out var balloon))
            {
                _enterBalloons.Remove(balloon);
            }
        }
    }
}