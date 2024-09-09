/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Threading;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Runtime.Item;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.Runtime.ShootingGame
{
    public class ShootingGameManager : IShootingGameManager
    {
        
        private Gun _gun;
        private bool _isStart;
        [Inject]
        private IBalloonInteractionManager _balloonManager;
        
        public bool IsStart => _isStart;
        public void StartGame(Gun gun)
        {
            if (_gun != null && _gun == gun)
                return;
            _gun = gun;
            InitGame();
            _isStart = true;
            StartBalloonChecker().Forget();
        }

        public void EndGame()
        {
            if (_gun != null)
            {
                StopBalloonChecker();
                _balloonManager.RemoveAllBalloons();
                _balloonManager.RemoveAllBullets();
                Object.Destroy(_gun.GameObject);
                _gun = null;
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
                _balloonManager.AddBullet(position, forward, forward * 10);
                _gun.Shoot();
            }
        }


        private const int MaxBalloonCount = 10;

        private void InitGame()
        {
            _balloonManager.AddBalloons(MaxBalloonCount);
        }
        
        private CancellationTokenSource _cancellationTokenSource;
        
        private async UniTask StartBalloonChecker()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                if (_balloonManager.Balloons.Count < MaxBalloonCount)
                {
                    var res = MaxBalloonCount - _balloonManager.Balloons.Count;
                    _balloonManager.AddBalloons(res);
                }
                await UniTask.Delay(1000, cancellationToken: cancellationToken);
            }
        }

        private void StopBalloonChecker()
        {
            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();
        }
    }
}