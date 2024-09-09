/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Entity;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace PicoMRDemo.Runtime.Runtime.ShootingGame
{
    public class BalloonInteractionManager : ITickable, IBalloonInteractionManager
    {
        public HashSet<IBullet> Bullets { get; } = new HashSet<IBullet>();
        public HashSet<IBalloon> Balloons { get; } = new HashSet<IBalloon>();
        
        readonly Dictionary<Collider, IBalloon> _colliderToBalloonMap = new Dictionary<Collider, IBalloon>();
        
        
        [Inject]
        private IResourceLoader _resourceLoader;

        [Inject] 
        private IEntityManager _entityManager;

        private AstarPath _astarPath;

        private AstarPath AstarPath
        {
            get
            {
                if (_astarPath == null)
                {
                    _astarPath = Object.FindObjectOfType<AstarPath>();
                }
                return _astarPath;
            }
        }
        
        public IBalloon AddBalloon(Vector3 position)
        {
            var balloonPrefab = _resourceLoader.AssetSetting.Balloon;
            var obj = Object.Instantiate(balloonPrefab,_entityManager.GetGameEntityRoot());
            obj.transform.position = position;
            var balloon = obj.GetComponent<IBalloon>();
            foreach (var balloonCollider in balloon.Colliders)
            {
                if (!_colliderToBalloonMap.TryGetValue(balloonCollider, out _))
                {
                    _colliderToBalloonMap.TryAdd(balloonCollider, balloon);
                }
            }
            balloon.RigidBody.velocity = Vector3.up * 0.2f;
            Balloons.Add(balloon);
            return balloon;
        }

        public IList<IBalloon> AddBalloons(int count)
        {
            var list = new List<IBalloon>();
            var randomPositions = GetWalkablePositions(count);
            for (int i = 0; i < count; i++)
            {
                Vector3 position = Vector3.zero;
                if (randomPositions.Count > i)
                {
                    position = randomPositions[i];
                }
                list.Add(AddBalloon(position + new Vector3(0f, 0.5f, 0f)));
            }
            return list;
        }

        public void RemoveBalloon(IBalloon balloon)
        {
            var colliders = balloon.Colliders;
            foreach (var collider in colliders)
            {
                if (TryGetBalloonForCollider(collider, out _))
                {
                    _colliderToBalloonMap.Remove(collider);
                }
            }
            Balloons.Remove(balloon);
        }

        public void RemoveBalloons(IList<IBalloon> balloons)
        {
            foreach (var balloon in balloons)
            {
                RemoveBalloon(balloon);
            }
        }

        public void RemoveAllBalloons()
        {
            var copiedList = Balloons.ToList();
            RemoveBalloons(copiedList);
            foreach (var balloon in copiedList)
            {
                Object.Destroy(balloon.GameObject);
            }
        }

        public IBullet AddBullet(Vector3 position, Vector3 direction, Vector3 velocity)
        {
            var bulletPrefab = _resourceLoader.AssetSetting.Bullet;
            var obj = Object.Instantiate(bulletPrefab, position, Quaternion.LookRotation(direction),_entityManager.GetGameEntityRoot());
            // obj.transform.position = position;
            obj.GetComponent<Rigidbody>().velocity = velocity;
            var bullet = obj.GetComponent<IBullet>();
            bullet.BalloonInteractionManager = this;
            Bullets.Add(bullet);
            return bullet;
        }

        public void RemoveBullet(IBullet bullet)
        {
            Bullets.Remove(bullet);
        }

        public void RemoveAllBullets()
        {
            var copiedList = Bullets.ToList();
            foreach (var bullet in copiedList)
            {
                RemoveBullet(bullet);
            }
        }

        public bool TryGetBalloonForCollider(Collider balloonCollider, out IBalloon balloon)
        {
            balloon = null;
            if (balloonCollider == null)
                return false;
            
            _colliderToBalloonMap.TryGetValue(balloonCollider, out balloon);
            return balloon != null && (!(balloon is Object unityObject) || unityObject != null);
        }

        private readonly IList<IBullet> _removedBullet = new List<IBullet>();
        public void Tick()
        {
            if (_removedBullet.Count > 0)
                _removedBullet.Clear();
            foreach (var bullet in Bullets)
            {
                bullet.GetValidTargets(out var balloons);
                if (balloons is { Length: > 0 })
                {
                    foreach (var balloon in balloons)
                    { 
                        RemoveBalloon(balloon);
                        balloon.Boom();
                    }
                    _removedBullet.Add(bullet);
                    bullet.PlayHit();
                }
            }

            if (_removedBullet.Count > 0)
            {
                foreach (var bullet in _removedBullet)
                {
                    RemoveBullet(bullet);
                }
                _removedBullet.Clear();
            }
        }
        
        private IList<Vector3> GetWalkablePositions(int count)
        {
            var result = GetRandomItems(OriginalPositions, count);
            return result;
        }

        private IList<Vector3> _originalPositions;
        private IList<Vector3> OriginalPositions
        {
            get
            {
                if (_originalPositions == null || _originalPositions.Count <= 0)
                {
                    _originalPositions = new List<Vector3>();
                    if (AstarPath != null)
                    {
                        AstarPath.data.GetNodes((node) =>
                        {
                            if (node.Walkable)
                            {
                                _originalPositions.Add((Vector3)node.position);
                            }
                        });
                    }
                }
                return _originalPositions;
            }
        }

        private IList<Vector3> GetRandomItems(IList<Vector3> originList, int count)
        {
            IList<Vector3> result;
            if (originList.Count <= count)
            {
                result = originList.ToList();
                return result;
            }
            var randomList = originList.OrderBy(_ => Random.value).ToList();
            result = randomList.GetRange(0, count);
            return result;
        }

        
    }
}