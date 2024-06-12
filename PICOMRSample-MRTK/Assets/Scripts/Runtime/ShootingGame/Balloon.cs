/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PicoMRDemo.Runtime.Runtime.ShootingGame
{
    public class Balloon : MonoBehaviour, IBalloon
    {
        private Rigidbody _rigidbody;
        public IList<Collider> Colliders { get; private set; }
        public GameObject GameObject => gameObject;

        public GameObject[] BallonEntities;
        public ParticleSystem Hit;

        public Rigidbody Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }

                return _rigidbody;
            }
        }

        private void Awake()
        {
            var randomValue = Random.Range(0, BallonEntities.Length);
            for (int i = 0; i < BallonEntities.Length; i++)
            {
                BallonEntities[i].SetActive(i == randomValue);
            }
            Colliders = GetComponentsInChildren<Collider>().ToArray();
        }

        public async UniTask Boom()
        {
            Debug.unityLogger.Log("Balloom", "Boom");
            Hit.gameObject.SetActive(true);
            Hit.Play();
            for (int i = 0; i < BallonEntities.Length; i++)
            {
                BallonEntities[i].SetActive(false);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            Hit.Stop();
            Hit.gameObject.SetActive(false);
            GameObject.Destroy(gameObject);
        }
    }
}