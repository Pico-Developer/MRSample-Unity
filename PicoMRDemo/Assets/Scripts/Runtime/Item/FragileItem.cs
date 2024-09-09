/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using UnityEngine;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public class FragileItem : Item
    {
        [Tooltip("破碎后是否自动消失")]
        public bool AutoDisappear = true;

        [Range(1, 60)]
        [Tooltip("AutoDisappear开启下，破碎后持续多久消失")]
        public float DisappearTime = 3f;

        public GameObject FractureMesh;
        public GameObject FullGameObject;
        public GameObject FragmentsObject;
        
        private UnfreezeFragment _fracture;
        
        private void Start()
        {
            _fracture = FullGameObject.GetComponent<UnfreezeFragment>();
            _fracture.onFractureCompleted.AddListener(FractureCompleted);
        }

        private void OnDestroy()
        {
            _fracture.onFractureCompleted.RemoveListener(FractureCompleted);
        }

        public void FractureCompleted()
        {
            FullGameObject.SetActive(false);
            FragmentsObject.SetActive(true);
            if (AutoDisappear)
            {            
                StopCoroutine(nameof(Disappear));
                StartCoroutine(nameof(Disappear));
            }
        }
        
        /// <summary>
        /// 破碎后消失
        /// </summary>
        /// <returns></returns>
        private IEnumerator Disappear()
        {
            yield return new WaitForSeconds(DisappearTime);

            if (EntityManager != null && Entity != null)
            {
                EntityManager.DeleteEntity(Entity);
            }
            else
            {
                Debug.unityLogger.Log($"EntityManager: {EntityManager}, Entity: {Entity}");
                Destroy(this.gameObject);
            }
        }
        
        void OnCollisionEnter(Collision collision)
        {
            _fracture.TryFractureByCollision(collision);
        }
    }
}