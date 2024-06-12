/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Honeti;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Mock;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Runtime.Pet;
using PicoMRDemo.Runtime.Runtime.PresetDecoration;
using PicoMRDemo.Runtime.Runtime.SDK;
using PicoMRDemo.Runtime.Runtime.ShootingGame;
using PicoMRDemo.Runtime.Runtime.Theme;
using PicoMRDemo.Runtime.Service;
using PicoMRDemo.Runtime.UI;
using PicoMRDemo.Runtime.Utils;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using LocationService = PicoMRDemo.Runtime.Service.LocationService;

namespace PicoMRDemo.Runtime.Game
{
    public class App : MonoBehaviour
    {
        private readonly string TAG = nameof(App);

        private LifetimeScope _rootLifetimeScope;
        private IObjectResolver Container => _rootLifetimeScope.Container;
        
        private ulong _onSpatialTrackingStateTask;

        public UIContext UIContext;
        public ResourceLoader ResourceLoader;

        async void Start()
        {
            _rootLifetimeScope = LifetimeScope.Create(ConfigureRootAppContext);
            UIContext.Init(_rootLifetimeScope);
            await InitAsync();
            Debug.unityLogger.Log(TAG, $"Current camera position: {Camera.main.transform.position}");
            UpdatePos().Forget();
            CheckUserPos().Forget();
        }
        
        void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                PXR_Boundary.EnableSeeThroughManual(true);
            }
        }

        private void ConfigureRootAppContext(IContainerBuilder builder)
        {
            Debug.unityLogger.Log(TAG, "Configure App Context...");
            
            builder.RegisterEntryPointExceptionHandler(OnEntryPointException);
#if UNITY_EDITOR
            builder.Register<IEntityManager, MockEntityManager>(Lifetime.Singleton);
#else
            builder.Register<IEntityManager, EntityManager>(Lifetime.Singleton);
#endif
            builder.Register<IRoomService, RoomService>(Lifetime.Singleton);
            builder.Register<IGuideService, GuideService>(Lifetime.Singleton);
            builder.Register<ILocationService, LocationService>(Lifetime.Singleton);
            builder.Register<IItemDataLoader, ItemDataLoader>(Lifetime.Singleton);
            builder.Register<IMaterialLoader, MaterialLoader>(Lifetime.Singleton);
            builder.Register<IPresetDecorationLoader, PresetDecorationLoader>(Lifetime.Singleton);
            builder.Register<IItemFactory, ItemFactory>(Lifetime.Singleton);
            builder.Register<IPersistentLoader, PersistentLoader>(Lifetime.Singleton);
            builder.Register<IPresetDecorationManager, PresetDecorationManager>(Lifetime.Singleton);
            builder.Register<IPetFactory, PetFactory>(Lifetime.Singleton);
            builder.Register<IThemeLoader, ThemeLoader>(Lifetime.Singleton);
            builder.Register<IThemeManager, ThemeManager>(Lifetime.Singleton);
            builder.Register<IDecorationDataLoader, DecorationDataLoader>(Lifetime.Singleton);
            builder.Register<ICatchableManager, CatchableManager>(Lifetime.Singleton);
            builder.Register<ILightManager, LightManager>(Lifetime.Singleton);
            builder.Register<IMRSDKManager, MRSDKManager>(Lifetime.Singleton);
            builder.Register<IShootingGameManager, ShootingGameManager>(Lifetime.Singleton);
            builder.Register<IVirtualWorldManager, VirtualWorldManager>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LogCapture>().As<ILogCapture>();
            builder.RegisterEntryPoint<BalloonInteractionManager>().As<IBalloonInteractionManager>();
            builder.RegisterComponentInNewPrefab(ResourceLoader, Lifetime.Singleton).As<IResourceLoader>();
            builder.RegisterComponent(UIContext);
        }
        private async UniTask<bool> CheckSpatialTrackingStateAsync()
        {
            await UniTask.CompletedTask;
            return true;
        }

        private async UniTask InitAsync()
        {
            // Turn on MR mode
            PXR_Boundary.EnableSeeThroughManual(true);
            
            Debug.unityLogger.Log(TAG, "Turn on MR mode");
            
            var spatialState = await CheckSpatialTrackingStateAsync();
            if (spatialState)
            {
                Debug.unityLogger.Log(TAG, $"Init Spatial Tracking State Success");
                var entityManager = Container.Resolve<IEntityManager>();
                Debug.unityLogger.Log(TAG, "Start Load Room Entities");
                await entityManager.LoadRoomEntities();
                Debug.unityLogger.Log(TAG, "Load Room Entities Finished");

                var guideService = Container.Resolve<IGuideService>();
                var persistentLoader = Container.Resolve<IPersistentLoader>();

#if UNITY_EDITOR
                var roomService = Container.Resolve<IRoomService>();
                roomService.EnterRoom();
#else
                await persistentLoader.LoadAllData();
                guideService.GenerateGuideGameObject();
#endif
            }
            else
            {
                Debug.unityLogger.LogError(TAG, $"Init Spatial Tracking State Error");
            }
        }

        private async UniTask UpdatePos()
        {
            var entityManager = Container.Resolve<IEntityManager>();
            while (true)
            {
                var allRoomEntities = entityManager.GetRoomEntities();
                foreach (var roomEntity in allRoomEntities)
                {
                    roomEntity.GameObject.transform.position = roomEntity.AnchorData.Position;
                    roomEntity.GameObject.transform.rotation = roomEntity.AnchorData.Rotation;
                }

                await UniTask.Delay(10000);
            }
        }

        private async UniTask CheckUserPos()
        {
            var mainCameraTransform = Camera.main.transform;
            var locationService = Container.Resolve<ILocationService>();
            var uiContext = Container.Resolve<UIContext>();
            var showToast = false;
            while (true)
            {
                var isInsideRoom = locationService.CheckPointInRoom(mainCameraTransform.position);
                
                if (showToast != !isInsideRoom)
                {
                    showToast = !isInsideRoom;
                    uiContext.SetToast(showToast, I18N.instance.getValue(ConstantProperty.OutsideRoomTip));
                }
                await UniTask.Delay(10000);
            }
        }

        private void OnEntryPointException(Exception ex)
        {
            Debug.unityLogger.LogException(ex);
        }
    }
}