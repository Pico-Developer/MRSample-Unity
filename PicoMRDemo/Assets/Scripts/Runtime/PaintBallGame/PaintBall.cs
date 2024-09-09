/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using PicoMRDemo.Runtime.Game;
using PicoMRDemo.Runtime.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace PicoMRDemo.Runtime.Runtime.ShootingGame
{
    public class PaintBall : MonoBehaviour
    {
        public IPaintBallGameManager PaintBallGameManager { get; set; }

        public GameObject GameObject => gameObject;

        [FormerlySerializedAs("HitEffect")] public ParticleSystem hitEffect;
        
        public Color ballColor;
        
        private Rigidbody RigidBody
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

        private void PlayHit(Collision collision)
        {
            ContactPoint contact = collision.contacts[0];
            var hitEffectPrefab = UnityEngine.Object.Instantiate(hitEffect,contact.point,collision.transform.rotation);
            hitEffectPrefab.transform.LookAt(GameObject.transform.position - contact.normal);
            hitEffectPrefab.transform.Translate(Vector3.back * 0.001f);
            hitEffectPrefab.Play();
            var particleeffect = hitEffectPrefab.GetComponent<ParticleSystem>();
            Material copiedMaterial = new Material(particleeffect.GetComponent<Renderer>().sharedMaterial);
            copiedMaterial.color = ballColor;
            particleeffect.GetComponent<Renderer>().sharedMaterial = copiedMaterial;
            PaintBallGameManager.AddPaintEffect(hitEffectPrefab);
            RigidBody.velocity /= 2f;
        }

        private void Update()
        {
            if (LifeTime <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                LifeTime -= Time.deltaTime;
            }
        }
        
        private void OnCollisionEnterEvent(Collision collision)
        {
            if (collision.gameObject.layer == 7||
                collision.gameObject.layer == 8||
                collision.gameObject.layer == 9||
                collision.gameObject.layer == 10|
                App.Instance.spatialMeshManager.activeSelf)
            {
                PlayHit(collision);
            }
        }

        private void OnCollisionStayEvent(Collision collision)
        {
        }

        private void OnCollisionExitEvent(Collision collision)
        {
            
        }
    }
}