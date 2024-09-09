/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
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
        
        public Toggle ThemeButton;
        public Toggle CeilingButton;
        public Toggle WallButton;
        public Toggle FloorButton;
        public Toggle DecorationButton;

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
            ShowTheme(true);
        }

        private void OnDisable()
        {
            UnregisterEvent();
        }


        private void RegisterEvent()
        {
            DecorationButton.onValueChanged.AddListener(OnDecorationButton);
            WallButton.onValueChanged.AddListener(OnWallButton);
            FloorButton.onValueChanged.AddListener(OnFloorButton);
            CeilingButton.onValueChanged.AddListener(OnCeilingButton);
            ThemeButton.onValueChanged.AddListener(ShowTheme);
        }

        private void UnregisterEvent()
        {
            ThemeButton.onValueChanged.RemoveListener(ShowTheme);
            CeilingButton.onValueChanged.RemoveListener(OnCeilingButton);
            FloorButton.onValueChanged.RemoveListener(OnFloorButton);
            WallButton.onValueChanged.RemoveListener(OnWallButton);
            DecorationButton.onValueChanged.RemoveListener(OnDecorationButton);
        }

        private void OnDecorationButton(bool isOn)
        {
            if (isOn)
            {
                IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Item);
                Page.Close();
                Page.Show(data);
            }
            
        }

        private void OnWallButton(bool isOn)
        {
            if (isOn)
            {
                IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Wall);
                Page.Close();
                Page.Show(data);
            }
            
        }

        private void OnFloorButton(bool isOn)
        {
            if (isOn)
            {
                IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Floor);
                Page.Close();
                Page.Show(data);
            }
            
        }

        private void OnCeilingButton(bool isOn)
        {
            if (isOn)
            {
                IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Ceiling);
                Page.Close();
                Page.Show(data);
            }
        }

        private void ShowTheme(bool isOn)
        {
            if (isOn)
            {
                IList<IDecorationData> data = DecorationDataLoader.LoadData(DecorationType.Theme);
                Page.Close();
                Page.Show(data);
            }
            
        }

    }
}