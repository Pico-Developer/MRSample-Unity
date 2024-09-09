/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Data;
using UnityEngine;
using VContainer.Unity;
using VContainer;
namespace PicoMRDemo.Runtime.Runtime.ShootingGame
{
    public class PaintBallGameManager :ITickable, IPaintBallGameManager
    {

        private Gun _gun;
        private bool _isStart;

        private HashSet<PaintBall> PaintBalls { get; } = new HashSet<PaintBall>();
        private HashSet<ParticleSystem> PaintEffects { get; } = new HashSet<ParticleSystem>();
        public bool IsStart => _isStart;
        [Inject]
        private IResourceLoader _resourceLoader;
        public void StartGame(Gun gun)
        {
            if (_gun != null && _gun == gun)
                return;
            _gun = gun;
            _isStart = true;
        }

        public void EndGame()
        {
            if (_gun != null)
            {
                
                Object.Destroy(_gun.GameObject);
                _gun = null;
                RemoveAllPaintBalls();
                RemoveAllPaintBallEffects();
                _isStart = false;
            }
        }

        public void Shoot(Gun gun)
        {
            if (_gun == gun)
            {
                var gunTransform = _gun.transform;
                var position = _gun.FirePoint.position;
                var forward = gunTransform.forward;
                var paintball = AddBall(position, forward, forward * 10);
                PaintBalls.Add(paintball);
                _gun.Shoot();
            }
        }

        private PaintBall AddBall(Vector3 position, Vector3 direction, Vector3 velocity)
        {
            var ballPrefab = _resourceLoader.AssetSetting.PaintBall;
            var obj = Object.Instantiate(ballPrefab, position, Quaternion.LookRotation(direction));
            // obj.transform.position = position;
            var ballMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            ballMaterial.color = new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f));
            obj.GetComponent<Renderer>().material = ballMaterial;
            obj.GetComponent<Rigidbody>().velocity = velocity;
            var paintball = obj.GetComponent<PaintBall>();
            paintball.ballColor = ballMaterial.color;
            paintball.PaintBallGameManager = this;
            return paintball;
        }
        public void AddPaintEffect(ParticleSystem ps)
        {
            PaintEffects.Add(ps);
        }

        private void RemoveAllPaintBalls()
        {
            var copiedList = PaintBalls.ToList();
            foreach (var paintBall in copiedList)
            {
                if (paintBall)
                {
                    RemoveBall(paintBall);
                }
            }
        }

        private void RemoveBall(PaintBall paintBall)
        {
            PaintBalls.Remove(paintBall);
            if (paintBall.gameObject)
            {
                Object.DestroyImmediate(paintBall.gameObject,true);
            }
        }

        private void RemoveAllPaintBallEffects()
        {
            var copiedList = PaintEffects.ToList();
            foreach (var paintBallEffect in copiedList)
            {
                if (paintBallEffect)
                {
                    RemovePaintBallEffect(paintBallEffect);
                }
            }
        }

        private void RemovePaintBallEffect(ParticleSystem paintBallEffect)
        {
            PaintEffects.Remove(paintBallEffect);
            if (paintBallEffect.gameObject)
            {
                Object.DestroyImmediate(paintBallEffect.gameObject, true);
            }
        }
        public void Tick()
        {
            var copiedList = PaintEffects.ToList();
            foreach (ParticleSystem ps in copiedList)
            {
                if (ps.isStopped)
                {
                    RemovePaintBallEffect(ps);
                }
            }
        }
    }
}