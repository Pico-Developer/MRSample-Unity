/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Honeti;
using Microsoft.MixedReality.Toolkit.UX;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Config;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Service;
using TMPro;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.UI
{
    public class DebugPage : MonoBehaviour
    {
        
        public Transform Root;
        public GameObject Prefab;
        public Transform Pool;

        public IList<PressableButton> ShowButtons => _showButtons;

        private Queue<PressableButton> _buttonPool = new Queue<PressableButton>();
        private IList<PressableButton> _showButtons = new List<PressableButton>();

        private IAssetConfig _assetConfig;

        private readonly string TAG = nameof(DebugPage);

        [Inject]
        private IEntityManager _entityManager;
        [Inject]
        private IPersistentLoader _persistentLoader;
        [Inject]
        private IThemeManager _themeManager;


        public void Show()
        {
            Close();
            gameObject.SetActive(true);
            var commands = RegisterCommand();
            foreach (var command in commands)
            {
                PressableButton button = null;
                if (_buttonPool.Count > 0)
                {
                    button = _buttonPool.Dequeue();
                }
                else
                {
                    button = GameObject.Instantiate(Prefab).GetComponent<PressableButton>();
                }

                IList<TMP_Text> texts = button.transform.GetComponentsInChildren<TMP_Text>();

                if (texts.Count >= 2)
                {
                    texts[0].text = command.Title;
                    texts[1].text = command.Des;
                }
                
                button.OnClicked.AddListener(() =>
                {
                    command.Action.Invoke();
                });
                
                button.transform.SetParent(Root, false);
                button.gameObject.SetActive(true);
                _showButtons.Add(button);
            }
        }

        public void Close()
        {
            var buttons = Root.GetComponentsInChildren<PressableButton>();

            foreach (var pressableButton in buttons)
            {
                pressableButton.OnClicked.RemoveAllListeners();
                _buttonPool.Enqueue(pressableButton);
                pressableButton.transform.SetParent(Pool);
            }
            _showButtons.Clear();
            gameObject.SetActive(false);
        }

        class Command
        {
            public string Title;
            public string Des;
            public Action Action;
        }
        private IList<Command> RegisterCommand()
        {
            IList<Command> commands = new List<Command>();
            commands.Add(new Command()
            {
                Title = I18N.instance.getValue("^CLEAR_DATA"),
                Des = I18N.instance.getValue("^CLEAR_ANCHOR_DATA"),
                Action = async () =>
                {
                    await _entityManager.ClearGameEntities();
                    _persistentLoader.ClearAllThemeData();
                    await _persistentLoader.DeleteAllData();
                    _themeManager.SwitchToDefaultTheme();
                }
            });
            return commands;
        }
    }
}