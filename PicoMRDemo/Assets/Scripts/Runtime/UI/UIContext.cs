/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using VContainer;
using VContainer.Unity;
using PicoMRDemo.Runtime.Game;
namespace PicoMRDemo.Runtime.UI
{
    public class UIContext : MonoBehaviour
    {
        public static UIContext Instance;
        public Dialog DialogPool;
        public MainMenu MainMenu;
        public HomePage HomePage;
        public DecorateRoomPage DecorateRoomPage;
        public ShootGamePage ShootGamePage;
        public VirtualWorldPage VirtualWorldPage;
        public BallDropPage BallDropPage;
        public PaintShootPage PaintShootPage;
        public DoodlePage DoodlePage;
        public DecorationPage DecorationPage;
        public PetGamePage PetGamePage;
        public DebugPage DebugPage;
        public BottomPanel BottomPanel;
        public ConsoleVirtualizeList ConsoleVirtualizeList;
        public ToastComponent ToastComponent;
        private LifetimeScope _lifetimeScope;

        private readonly string TAG = nameof(UIContext);
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
        public void Init(LifetimeScope rootLifetimeScope)
        {
            _lifetimeScope = rootLifetimeScope.CreateChild(ConfigureUIContext);
        }

        private void OnEnable()
        {
            RegisterEvent();
        }

        private void OnDisable()
        {
            UnregisterEvent();
        }


        private void ConfigureUIContext(IContainerBuilder builder)
        {
            builder.RegisterComponent(DialogPool);
            builder.RegisterComponent(MainMenu);
            builder.RegisterComponent(HomePage);
            builder.RegisterComponent(DecorateRoomPage);
            builder.RegisterComponent(BallDropPage);
            builder.RegisterComponent(PaintShootPage);
            builder.RegisterComponent(VirtualWorldPage);
            builder.RegisterComponent(ShootGamePage);
            builder.RegisterComponent(DecorationPage);
            builder.RegisterComponent(PetGamePage);
            builder.RegisterComponent(BottomPanel);
            builder.RegisterComponent(ConsoleVirtualizeList);
            builder.RegisterComponent(DoodlePage);
            builder.RegisterComponent(DebugPage);
        }
        
        private void RegisterEvent()
        {
            MainMenu.LogWindowButton.onClick.AddListener(ToggleConsoleMenu);
            MainMenu.OnClose += CloseConsoleMenu;
        }

        private void UnregisterEvent()
        {
            MainMenu.OnClose -= CloseConsoleMenu;
            MainMenu.LogWindowButton.onClick.RemoveListener(ToggleConsoleMenu);
        }

        private void CloseConsoleMenu()
        {
            if (ConsoleVirtualizeList.gameObject.activeSelf)
            {
                ToggleConsoleMenu();
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("ShortCut/ToggleMainMenu")]
        public static void OpenMainMenu()
        {
            var uiContext = FindObjectOfType<UIContext>();
            uiContext.ToggleMainMenu();
        }
#endif
        public void ToggleMainMenu()
        {
            var active = !MainMenu.gameObject.activeSelf;
            if (active)
            {
                MainMenu.Open();
                App.Instance.isShowEnterGuide = false;
            }
            else
            {
                MainMenu.Close();
            }
        }
        
        private void ToggleConsoleMenu()
        {
            var active = !ConsoleVirtualizeList.gameObject.activeSelf;
            if (active)
            {
                var localPos = new Vector3(0.7960f, 0.0f, 0.0f);
                
                var mainMenuTransform = MainMenu.gameObject.transform;
                var deltaV = new Vector3(Mathf.Cos(Mathf.PI/9.0f), 0, -Mathf.Sin(Mathf.PI/9.0f)) * 0.245f - new Vector3(0.245f, 0f, 0f);
                localPos += deltaV;
                var pos = mainMenuTransform.TransformPoint(localPos);
                ConsoleVirtualizeList.gameObject.transform.position = pos;
                var worldDirectionToTarget =
                    mainMenuTransform.TransformDirection(new Vector3(-Mathf.Sin(-Mathf.PI / 9.0f), 0,
                        Mathf.Cos(-Mathf.PI / 9.0f)));
                var worldUp = mainMenuTransform.TransformDirection(Vector3.up);
                ConsoleVirtualizeList.gameObject.transform.rotation = Quaternion.LookRotation(worldDirectionToTarget, worldUp);
                ConsoleVirtualizeList.Open();
            }
            else
            {
                ConsoleVirtualizeList.Close();
            }
        }

        public void SetToast(bool active, string context)
        {
            if (!active)
            {
                ToastComponent.HideToast();
                return;
            }
            
            ToastComponent.ShowToast(context);
        }
    }
}