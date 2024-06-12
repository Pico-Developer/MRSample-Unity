/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UX;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Service;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PicoMRDemo.Runtime.UI
{
    public class DecorateRoomPage : MonoBehaviour
    {
        public DecorationPage Page;
        
        public PressableButton ThemeButton;
        public PressableButton CeilingButton;
        public PressableButton WallButton;
        public PressableButton FloorButton;
        public PressableButton DecorationButton;

        public Toggle OpenToggle;
        
        [Inject]
        public IDecorationDataLoader DecorationDataLoader;

        public void Open()
        {
            if (OpenToggle.isOn) return;

            Page.Close();
            OpenToggle.isOn = true;
        }

        private void OnEnable()
        {
            RegisterEvent();
            ShowTheme();
        }

        private void OnDisable()
        {
            UnregisterEvent();
        }


        public void RegisterEvent()
        {
            DecorationButton.OnClicked.AddListener(OnDecorationButton);
            WallButton.OnClicked.AddListener(OnWallButton);
            FloorButton.OnClicked.AddListener(OnFloorButton);
            CeilingButton.OnClicked.AddListener(OnCeilingButton);
            ThemeButton.OnClicked.AddListener(ShowTheme);
        }

        public void UnregisterEvent()
        {
            ThemeButton.OnClicked.RemoveListener(ShowTheme);
            CeilingButton.OnClicked.RemoveListener(OnCeilingButton);
            FloorButton.OnClicked.RemoveListener(OnFloorButton);
            WallButton.OnClicked.RemoveListener(OnWallButton);
            DecorationButton.OnClicked.RemoveListener(OnDecorationButton);
        }

        private void OnDecorationButton()
        {
            IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Item);
            Page.Close();
            Page.Show(data);
        }

        private void OnWallButton()
        {
            IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Wall);
            Page.Close();
            Page.Show(data);
        }

        private void OnFloorButton()
        {
            IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Floor);
            Page.Close();
            Page.Show(data);
        }

        private void OnCeilingButton()
        {
            IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Ceiling);
            Page.Close();
            Page.Show(data);
        }

        private void ShowTheme()
        {
            IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Theme);
            Page.Close();
            Page.Show(data);
        }

    }
}