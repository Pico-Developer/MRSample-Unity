/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
using System;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Mock;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Runtime.Pet;
using PicoMRDemo.Runtime.Runtime.PresetDecoration;
using PicoMRDemo.Runtime.Runtime.SDK;
using PicoMRDemo.Runtime.Runtime.ShootingGame;
using PicoMRDemo.Runtime.Runtime.BallDrop;
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
        public static App Instance;
        private readonly string _tag = nameof(App);

        private LifetimeScope _rootLifetimeScope;
        private IObjectResolver Container => _rootLifetimeScope.Container;
        
        private ulong _onSpatialTrackingStateTask;

        public UIContext UIContext;
        public ResourceLoader ResourceLoader;
        public ControllerManager ControllerManager;
        public GameObject spatialMeshManager;
        public GameObject deviceSimulator;
        public bool isShowEnterGuide = true;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        async void Start()
        {
            _rootLifetimeScope = LifetimeScope.Create(ConfigureRootAppContext);
            UIContext.Init(_rootLifetimeScope);
            ControllerManager.Instance.BindingMainMenuHotKey();
            await InitAsync();
            if (Camera.main != null)
                Debug.unityLogger.Log(_tag, $"Current camera position: {Camera.main.transform.position}");
            UpdatePos().Forget();
            CheckUserPos().Forget();
            UpdateAnchorData().Forget();
#if UNITY_EDITOR
            Instantiate(deviceSimulator);
#endif
            spatialMeshManager.SetActive(false);
            UIContext.Instance.ToggleMainMenu();
        }
        
        void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                PXR_Manager.EnableVideoSeeThrough = true;
            }
        }

        private void ConfigureRootAppContext(IContainerBuilder builder)
        {
            Debug.unityLogger.Log(_tag, "Configure App Context...");
            
            builder.RegisterEntryPointExceptionHandler(OnEntryPointException);
#if UNITY_EDITOR
            builder.Register<IEntityManager, MockEntityManager>(Lifetime.Singleton);
#else
            builder.Register<IEntityManager, EntityManager>(Lifetime.Singleton);
#endif
            //control room entities' create and destroy by room anchor data
            builder.Register<IRoomService, RoomService>(Lifetime.Singleton);
            //some location functions for checking the room entities' position in space
            builder.Register<ILocationService, LocationService>(Lifetime.Singleton);
            //item data loader
            builder.Register<IItemDataLoader, ItemDataLoader>(Lifetime.Singleton);
            //material data loader
            builder.Register<IMaterialLoader, MaterialLoader>(Lifetime.Singleton);
            //current decoration data loader
            builder.Register<IPresetDecorationLoader, PresetDecorationLoader>(Lifetime.Singleton);
            //item creator
            builder.Register<IItemFactory, ItemFactory>(Lifetime.Singleton);
            //decorations and items local-data save and load 
            builder.Register<IPersistentLoader, PersistentLoader>(Lifetime.Singleton);
            //decoration creator
            builder.Register<IPresetDecorationManager, PresetDecorationManager>(Lifetime.Singleton);
            //Pet Manager
            builder.Register<IPetFactory, PetFactory>(Lifetime.Singleton);
            //Theme data loader
            builder.Register<IThemeLoader, ThemeLoader>(Lifetime.Singleton);
            //Theme Control Manager
            builder.Register<IThemeManager, ThemeManager>(Lifetime.Singleton);
            //decoration data loader
            builder.Register<IDecorationDataLoader, DecorationDataLoader>(Lifetime.Singleton);
            //some catchable Object Manager
            builder.Register<ICatchableManager, CatchableManager>(Lifetime.Singleton);
            //Light Manager
            builder.Register<ILightManager, LightManager>(Lifetime.Singleton);
            //SDK Interface
            builder.Register<IMRSDKManager, MRSDKManager>(Lifetime.Singleton);
            //Shooting Game Manager
            builder.Register<IShootingGameManager, ShootingGameManager>(Lifetime.Singleton);
            //VirtualWorld Manager
            builder.Register<IVirtualWorldManager, VirtualWorldManager>(Lifetime.Singleton);
            //BallDrop Game Manager
            builder.Register<IBallDropGameManager, BallDropGameManager>(Lifetime.Singleton);
            //Log Panel Manager
            builder.RegisterEntryPoint<LogCapture>().As<ILogCapture>();
            //Balloons Control in Shooting Game
            builder.RegisterEntryPoint<BalloonInteractionManager>().As<IBalloonInteractionManager>();
            //PaintBall Game Manager
            builder.RegisterEntryPoint<PaintBallGameManager>().As<IPaintBallGameManager>();
            builder.RegisterComponentInNewPrefab(ResourceLoader, Lifetime.Singleton).As<IResourceLoader>();
            builder.RegisterComponent(UIContext);
            builder.RegisterComponent(ControllerManager);
        }
        private async UniTask<bool> CheckSpatialTrackingStateAsync()
        {
            await UniTask.CompletedTask;
            return true;
        }

        private async UniTask InitAsync()
        {
            // Turn on MR mode
            PXR_Manager.EnableVideoSeeThrough = true;
            Debug.unityLogger.Log(_tag, "Turn on MR mode");
            await StartSceneCaptureProvider();
            await StartSpatialAnchorProvider();
            Debug.unityLogger.Log(_tag, $"Init Provider Success");
            var result = await PXR_MixedReality.StartSceneCaptureAsync();
            Debug.unityLogger.Log($"StartSceneCaptureAsync: {result}");
            Debug.unityLogger.Log(_tag, $"StartSceneCaptureAsync Success");
            var spatialState = await CheckSpatialTrackingStateAsync();
            if (spatialState)
            {
                Debug.unityLogger.Log(_tag, $"Init Spatial Tracking State Success");
                var entityManager = Container.Resolve<IEntityManager>();
                Debug.unityLogger.Log(_tag, "Start Load Room Entities");
                await entityManager.LoadRoomEntities();
                Debug.unityLogger.Log(_tag, "Load Room Entities Finished");
                
#if !UNITY_EDITOR
                var persistentLoader = Container.Resolve<IPersistentLoader>();
                await persistentLoader.LoadAllData();
#endif
                var roomService = Container.Resolve<IRoomService>();
                roomService.EnterRoom();
            }
            else
            {
                Debug.unityLogger.LogError(_tag, $"Init Spatial Tracking State Error");
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
            if (Camera.main != null)
            {
                var mainCameraTransform = Camera.main.transform;
                var locationService = Container.Resolve<ILocationService>();
                var uiContext = Container.Resolve<UIContext>();
                while (true)
                {
                    var isInsideRoom = locationService.CheckPointInRoom(mainCameraTransform.position);
                    if (spatialMeshManager.activeSelf)
                    {
                        uiContext.SetToast(false, ConstantProperty.OutsideRoomTip);
                    }
                    else if (!isInsideRoom)
                    {
                        uiContext.SetToast(true, ConstantProperty.OutsideRoomTip);
                    }
                    else if (isShowEnterGuide)
                    {
                        uiContext.SetToast(true, ConstantProperty.EnterRoomTip);
                    }
                    else 
                    {
                        uiContext.SetToast(false, ConstantProperty.OutsideRoomTip);
                    }
                    await UniTask.Delay(1000);
                }
            }
        }

        public IRoomService GetRoomService()
        {
            return Container.Resolve<IRoomService>();
        }
        
        private async UniTask UpdateAnchorData()
        {
            var entityManager = Container.Resolve<IEntityManager>();
            while (true)
            {
                entityManager.CheckSceneAnchorUpdate();
                entityManager.CheckSpatialAnchorUpdate();
                entityManager.UpdateSpatialAnchorPosition();
                await UniTask.Delay(1000);
            }
        }
        
        
        
        private void OnEntryPointException(Exception ex)
        {
            Debug.unityLogger.LogException(ex);
        }
        private async UniTask StartSceneCaptureProvider()
        {
            var result0 = await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SceneCapture);
            Debug.unityLogger.Log($"StartSceneCaptureProvider:SceneCapture: {result0}");
        }
        private async UniTask StartSpatialAnchorProvider()
        {
            var result0 = await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);
            Debug.unityLogger.Log($"StartSenseDataProvider: {result0}");
        }
    }
}