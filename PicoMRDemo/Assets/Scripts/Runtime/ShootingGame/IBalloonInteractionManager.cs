/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace PicoMRDemo.Runtime.Runtime.ShootingGame
{
    public interface IBalloonInteractionManager
    {
        HashSet<IBullet> Bullets { get; }
        HashSet<IBalloon> Balloons { get; }
        IBalloon AddBalloon(Vector3 position = default(Vector3));
        IList<IBalloon> AddBalloons(int count);
        void RemoveBalloon(IBalloon balloon);
        void RemoveBalloons(IList<IBalloon> balloons);
        void RemoveAllBalloons();
        IBullet AddBullet(Vector3 position, Vector3 direction, Vector3 velocity);
        void RemoveBullet(IBullet bullet);
        void RemoveAllBullets();

        bool TryGetBalloonForCollider(Collider balloonCollider, out IBalloon balloon);
    }
}