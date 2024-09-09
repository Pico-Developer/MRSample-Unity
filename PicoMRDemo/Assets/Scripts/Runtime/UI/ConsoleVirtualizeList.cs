/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Threading;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Utils;
using UnityEngine;
using VContainer;
using PicoDemoUI;
namespace PicoMRDemo.Runtime.UI
{
    public class ConsoleVirtualizeList : MonoBehaviour
    {
        [Inject]
        public ILogCapture LogCapture;
        public ScrollRectListVertical scrollRectListVertical;
        public LogDetailWindow LogDetailWindow;
        public GameObject Content;
       
        private CancellationTokenSource _disableCancellation = new CancellationTokenSource();
        private void OnEnable()
        {
            if (_disableCancellation != null)
            {
                _disableCancellation.Dispose();
            }
            _disableCancellation = new CancellationTokenSource();
             OpenVirtualizedListAsync().Forget();
        }
        
        public void OnDisable()
        {
            _disableCancellation.Cancel();
            CloseVirtualizedList();
        }

        private void OnDestroy()
        {
            _disableCancellation.Cancel();
            _disableCancellation.Dispose();
        }

        public void Open()
        {
            LogDetailWindow.HideWindow();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            LogDetailWindow.HideWindow();
            gameObject.SetActive(false);
        }

        private void OnLogDetailWindowClose()
        {
            LogDetailWindow.HideWindow();
            Content.SetActive(true);
        }
        private void DoList(GameObject go, int idx)
        {
            var consoleItem = go.GetComponent<ConsoleItem>();
            if (idx >= LogCapture.AllLogsDataList.Count)
            {
                return;
            }
            var data = LogCapture.AllLogsDataList[idx];
            if (data != null)
            {
                consoleItem.SetUIData(data);
                consoleItem.Button.onClick.AddListener(() =>
                {
                    LogDetailWindow.ShowWindow(data.msg, data.stack);
                    Content.SetActive(false);
                });
            }
        }

        private void HideInList(GameObject go, int idx)
        {
            var consoleItem = go.GetComponent<ConsoleItem>();
            consoleItem.Button.onClick.RemoveAllListeners();
        }

        private void SetDataCount(int count)
        {
            scrollRectListVertical.SetItemCount(count);
        }
        
        private async UniTask OpenVirtualizedListAsync()
        {
            scrollRectListVertical.OnVisible += DoList;
            scrollRectListVertical.OnInvisible += HideInList;
            LogDetailWindow.CloseButton.onClick.AddListener(OnLogDetailWindowClose);
            await UniTask.DelayFrame(3, PlayerLoopTiming.Update, _disableCancellation.Token);
            Content.SetActive(true);
            LogCapture.SetDataCount += SetDataCount;
            scrollRectListVertical.SetItemCount(LogCapture.AllLogsDataList.Count);
        }

        private void CloseVirtualizedList()
        {
            LogCapture.SetDataCount -= SetDataCount;
            Content.SetActive(false);
            LogDetailWindow.CloseButton.onClick.RemoveListener(OnLogDetailWindowClose);
            scrollRectListVertical.OnInvisible -= HideInList;
            scrollRectListVertical.OnVisible -= DoList;
        }
    }
}
