/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Microsoft.MixedReality.Toolkit.UX;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Service;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.UI
{
    public class BottomPanel : MonoBehaviour
    {
        [Header("Button")] 
        public PressableButton QuitButton;

        [Inject]
        public DialogPool DialogPool;

        [Inject]
        private IEntityManager _entityManager;
        [Inject]
        private IPersistentLoader _persistentLoader;
        [Inject]
        private IThemeManager _themeManager;

        private void OnEnable()
        {
            QuitButton.OnClicked.AddListener(QuitApplication);
        }

        private void OnDisable()
        {
            QuitButton.OnClicked.RemoveListener(QuitApplication);
        }

        private void QuitApplication()
        {
            var dialog = DialogPool.Get()
                .SetHeader("退出应用")
                .SetBody("确认退出应用吗?")
                .SetPositive("确定", async args =>
                {
                    await _entityManager.SaveGameEntities();
                    _persistentLoader.StageAllThemeData(_themeManager.GetCurrentThemes());
                    await _persistentLoader.SaveAllData();
                    Application.Quit();
                })
                .SetNegative("取消");
            dialog.Show();
        }
    }
}