/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Honeti;
using Microsoft.MixedReality.Toolkit;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Runtime.Item;
using TMPro;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.Service
{
    public interface IGuideService
    {
        void GenerateGuideGameObject();
        void OpenGuide(Transform gameObjectTransform);
        void CloseGuide(Transform gameObjectTransform);
    }

    public class GuideService : IGuideService
    {
        private IRoomService _roomService;
        private ILocationService _locationService;
        private IResourceLoader _resourceLoader;
        [Inject]
        private IEntityManager _entityManager;
        [Inject] 
        private IPersistentLoader _persistentLoader;
        [Inject]
        private IThemeManager _themeManager;
        private TweenerCore<Vector3, Vector3, VectorOptions> _guidTween;

        private readonly string TAG = nameof(GuideService);

        [Inject]
        public GuideService( IRoomService roomService, ILocationService locationService, IResourceLoader resourceLoader)
        {
            _roomService = roomService;
            _locationService = locationService;
            _resourceLoader = resourceLoader;
        }

        public void GenerateGuideGameObject()
        {
            if (!_locationService.TryGetTablePosition(out var targetPosition, Vector3.up * 0.5f))
            {
                Debug.unityLogger.LogWarning(TAG, "Can't find table");
                var mainCameraTransform = Camera.main.transform;
                targetPosition = mainCameraTransform.forward * 0.8f;
            }
            var picoModel = _resourceLoader.AssetSetting.PicoModel;
            var pico = GameObject.Instantiate(picoModel);
            pico.transform.position = targetPosition;
            var picoInteractable = pico.GetComponentInChildren<GrabableComponent>();
            
            var guideTextPrefab = _resourceLoader.AssetSetting.GuideText;
            var guideTextObject = GameObject.Instantiate(guideTextPrefab);
            var guideText = guideTextObject.GetComponent<TMP_Text>();
            guideText.text = I18N.instance.getValue("^ENTER_TIP_INFO");
            guideTextObject.SetActive(false);
            
            
            // loop
            OpenGuide(pico.transform);
            
            picoInteractable.OnGrab += () =>
            {
                Debug.unityLogger.Log(TAG, "SelectedEntered pico model");
                CloseGuide(pico.transform);
                guideTextObject.SetActive(true);
            };
            
            picoInteractable.OnDrop += async () =>
            {
                var dis = Vector3.Distance(pico.transform.position, Camera.main.transform.position);
                Debug.unityLogger.Log(TAG, $"dis: {dis}");
                if (dis < 0.2f)
                {
                    Debug.unityLogger.Log(TAG, "Enter MR World"); 
                    // Tips?.gameObject.SetActive(true);
                    picoInteractable = null;
                    _roomService.EnterRoom();
                    guideText.text =  I18N.instance.getValue("^WELCOME_INFO");
                    var renderers = pico.GetComponentsInChildren<MeshRenderer>();
                    foreach (var meshRenderer in renderers)
                    {
                        meshRenderer.enabled = false;
                    }
                    await UniTask.Delay(3000);
                    GameObject.Destroy(pico);
                    pico = null;
                    GameObject.Destroy(guideTextObject);
                }
                else
                {
                    Debug.unityLogger.Log(TAG, "SelectedExited pico model");
                    OpenGuide(pico.transform);
                    guideTextObject.SetActive(false);
                }
            };
            
            Debug.unityLogger.Log(TAG, $"Create Pico Model in {pico.transform.position}");
        }

        public void OpenGuide(Transform gameTransform)
        {
            // if (_guidTween != null)
            // {
            //     _guidTween.Kill();
            //     _guidTween = null;
            //     gameTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            // }
            // var endValue = new Vector3(1f, 1f, 1f);
            // _guidTween = gameTransform.DOScale(endValue, 0.2f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }

        public void CloseGuide(Transform gameTransform)
        {
            // if (_guidTween != null)
            // {
            //     _guidTween.Kill();
            //     _guidTween = null;
            // }
            //
            // gameTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }
}