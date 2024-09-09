/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Linq;
using PicoMRDemo.Runtime.Entity;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.Service
{
    public interface ILocationService
    {
        bool TryGetTablePosition(out Vector3 position, Vector3 offset = default(Vector3));
        bool TryGetFloorPosition(out Vector3 position, Vector3 offset = default(Vector3));
        bool CheckPointInRoom(Vector3 point);
    }

    public class LocationService : ILocationService
    {
        [Inject]
        private IEntityManager _entityManager;

        public bool CheckPointInRoom(Vector3 point)
        {
            // todo 每次都要遍历，优化下
            var roomEntities = _entityManager.GetRoomEntities();
            
            IEntity ceilingEntity = null;
            IEntity floorEntity = null;
            foreach (var roomEntity in roomEntities)
            {
                if (roomEntity.GetRoomLabel() == PxrSemanticLabel.Ceiling)
                {
                    ceilingEntity = roomEntity;
                }
                if (roomEntity.GetRoomLabel() == PxrSemanticLabel.Floor)
                {
                    floorEntity = roomEntity;
                }

                if (ceilingEntity != null && floorEntity != null)
                {
                    break;
                }
            }

            if (ceilingEntity == null || floorEntity == null)
            {
                Debug.unityLogger.LogWarning($"{nameof(LocationService)}", "CheckPointInRoom: Ceiling or floor Entity not find");
                return false;
            }

            var local = floorEntity.GameObject.transform.InverseTransformPoint(point);
            var point2D = new Vector2(local.x, local.y);
            var polyPoints = floorEntity.AnchorData.ScenePolygonData.Vertices;

            return point.y < ceilingEntity.GameObject.transform.position.y 
                   && point.y > floorEntity.GameObject.transform.position.y
                   && CheckPointInPolygon(point2D, polyPoints.ToArray());
        }

        public bool CheckPointInPolygon(Vector2 point, Vector3[] polyPoints)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                var isLeft = (pi.y <= point.y && point.y < pj.y) || (pj.y <= point.y && point.y < pi.y);
                var isIntersect = (point.x < (pj.x - pi.x)  / (pj.y - pi.y) * (point.y - pi.y) + pi.x);
                if (isLeft && isIntersect)
                {
                    inside = !inside;
                }
            }
            return inside;
        }
        
        public bool TryGetTablePosition(out Vector3 position, Vector3 offset = default(Vector3))
        {
            IEntity tableEntity = null;
            position = offset;
            var tableEntities = _entityManager.GetRoomEntities(PxrSemanticLabel.Table);
            if (tableEntities.Count > 0)
                tableEntity = tableEntities[0];

            if (tableEntity != null)
            {
                var tablePosition = tableEntity.AnchorData.Position;
                position = tablePosition + offset;
            }

            return tableEntity != null;
        }
        
        public bool TryGetFloorPosition(out Vector3 position, Vector3 offset = default(Vector3))
        {
            IEntity floorEntity = null;
            position = offset;
            var floorEntities = _entityManager.GetRoomEntities(PxrSemanticLabel.Floor);
            if (floorEntities.Count > 0)
                floorEntity = floorEntities[0];

            if (floorEntity != null)
            {
                var tablePosition = floorEntity.AnchorData.Position;
                position = tablePosition + offset;
            }

            return floorEntity != null;
        }
    }
}