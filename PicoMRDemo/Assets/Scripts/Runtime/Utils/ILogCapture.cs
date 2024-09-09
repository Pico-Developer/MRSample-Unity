/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace PicoMRDemo.Runtime.Utils
{
    public interface ILogCapture
    {
         IList<ConsoleListItemData> AllLogsDataList { get; }
         event Action<int> SetDataCount;
    }


    public class LogCapture : ILogCapture, ILateTickable, IDisposable
    {
        private List<ConsoleListItemData> _receivingDataList = new List<ConsoleListItemData>(256);
        private List<ConsoleListItemData> _allLogsDataList = new List<ConsoleListItemData>(4096);
        public IList<ConsoleListItemData> AllLogsDataList => _allLogsDataList;
        public event Action<int> SetDataCount;

        public LogCapture()
        {
            Application.logMessageReceivedThreaded += OnLogReceived;
        }
        
        public void LateTick()
        {
            ReceiveToDataList();
        }
        
        private void OnLogReceived(string logMsg, string stackTrace, LogType type)
        {

            if (logMsg == null)
                logMsg = string.Empty;
            if (stackTrace == null)
                stackTrace = string.Empty;

            var itemLevel = type;
            var itemMsg = logMsg;
            var itemStack = stackTrace;

            var dateTime = System.DateTime.UtcNow;
            var item = new ConsoleListItemData()
            {
                level = itemLevel,
                msg = itemMsg,
                stack = itemStack,
                dateTime = dateTime,
            };

            lock (_receivingDataList)
            {
                _receivingDataList.Add(item);
            }
        }
        
        private void ReceiveToDataList()
        {
            var addCount = 0;
            lock (_receivingDataList)
            {
                var sourceList = _receivingDataList;
                if (sourceList.Count == 0)
                {
                    return;
                }

                var targetList = _allLogsDataList;
                
                // CalcLevelsCounts(out countD, out countI, out countW, out countE);
                // CommonUtil.LimitFromEnd(ref targetList, ref sourceList, MaxLogsCount);
                
                targetList.AddRange(sourceList);
                addCount = sourceList.Count;
                sourceList.Clear();
            }

            if (addCount > 0)
            {
                var len = AllLogsDataList.Count;
                var gameTime = UnityEngine.Time.time;
                var gameFrame = UnityEngine.Time.frameCount;
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                for (int i = len - addCount; i >= 0 && i < len; i++)
                {
                    var item = AllLogsDataList[i];
                    item.gameTime = gameTime;
                    item.gameFrame = gameFrame;
                }
            }
            SetDataCount?.Invoke(AllLogsDataList.Count);
        }


        public void Dispose()
        {
            Application.logMessageReceivedThreaded -= OnLogReceived;
        }
    }

    public class ConsoleListItemData
    {
        public LogType level;
        public string msg;
        public string stack;
        public string tag;
        
        public System.DateTime dateTime;
        
        public float gameTime;
        public int gameFrame;


        public string LocalDateTimeText { get; private set; }

        public string GameTimeFrameText { get; private set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(msg);
        }

        public bool HasStack()
        {
            return !string.IsNullOrEmpty(stack);
        }

        public bool HasTag()
        {
            return !string.IsNullOrEmpty(tag);
        }

        public void UpdateTexts(bool force = false)
        {
            if (!IsValid())
            {
                LocalDateTimeText = string.Empty;
                GameTimeFrameText = string.Empty;
                return;
            }

            if (force || string.IsNullOrEmpty(LocalDateTimeText))
                LocalDateTimeText = GetTimeString();
            if (force || string.IsNullOrEmpty(GameTimeFrameText))
            {
                GameTimeFrameText = GetGameTimeFrame();
            }
        }

        public string GetTimeString(string format = "HH:mm:ss")
        {
            return dateTime.ToLocalTime().ToString(format);
        }

        public string GetGameTimeFrame()
        {
            int mm = Mathf.FloorToInt(gameTime / 60f);
            float ss = gameTime - mm * 60f;
            return $"{mm.ToString("00")}:{ss.ToString("00.###")}s {gameFrame:D0}f";
        }
    }
}