/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using PicoMRDemo.Runtime.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PicoMRDemo.Runtime.UI
{
    public class ConsoleItem : MonoBehaviour
    {
        public TMP_Text MsgText;
        public TMP_Text RightText1;
        public TMP_Text RightText2;
        //public FontIconSelector IconSelector;
        public Button Button;

        private readonly Color DebugColor = Color.white;
        private readonly Color WarningColor = Color.yellow;
        private readonly Color ErrorColor = Color.red;
        private readonly string DebugIcon = "Icon 97";
        private readonly string WarningIcon = "Icon 87";
        private readonly string ErrorIcon = "Icon 80";

        public void SetUIData(ConsoleListItemData data)
        {
            data.UpdateTexts();
            MsgText.text = data.msg;
            var color = DebugColor;
            var icon = DebugIcon;
            switch (data.level)
            {
                case LogType.Warning:
                    color = WarningColor;
                    icon = WarningIcon;
                    break;
                case LogType.Error:
                    color = ErrorColor;
                    icon = ErrorIcon;
                    break;
                default:
                    break;
            }

            MsgText.color = color;
            //IconSelector.CurrentIconName = icon;
            RightText1.text = $"[{data.LocalDateTimeText}]";
            RightText2.text = data.GameTimeFrameText;
        }
    }
}