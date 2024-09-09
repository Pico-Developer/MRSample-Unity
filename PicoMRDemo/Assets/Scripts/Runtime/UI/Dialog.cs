/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace PicoMRDemo.Runtime.UI
{
    public class Dialog : MonoBehaviour ,IDialog
    {
        public TMP_Text titleText;
        
        public TMP_Text bodyText;
        
        public TMP_Text firstText;
        
        public TMP_Text secondText;
        
        public Button firstButton;
        
        public Button secondButton;

        private string title = null;

        private string body = null;
        
        private UnityAction firstButtonAction = null;

        private UnityAction secondButtonAction = null;
        
        private bool hasDismissed = false;
        
        
        public IDialog SetTitle(string title)
        {
            this.title = title;
            return this;
        }
        
        public IDialog SetBody(string body)
        {
            this.body = body;
            return this;
        }
        
        public IDialog SetFirstButton(string label, UnityAction action)
        {
            if (label == null) { return this; }
            firstText.text = label;
            firstButtonAction = action;
            return this;
        }
        
        public IDialog SetSecondButton(string label,UnityAction action)
        {
            if (label == null) { return this; }
            secondText.text = label;
            secondButtonAction = action;
            return this;
        }
        
        public virtual void Reset()
        {
            title = null;
            body = null;
            firstButtonAction = null;
            secondButtonAction = null;
            firstButton.onClick.RemoveAllListeners();
            secondButton.onClick.RemoveAllListeners();
            hasDismissed = false;
        }
        
        /// <inheritdoc />
        public virtual IDialog Show()
        {
            titleText.gameObject.SetActive(title != null);
            titleText.text = title;
            bodyText.gameObject.SetActive(body != null);
            bodyText.text = body;
            
            firstButton.onClick.AddListener(firstButtonAction);
            secondButton.onClick.AddListener(secondButtonAction);
            firstButton.gameObject.SetActive(firstButtonAction != null);
            secondButton.gameObject.SetActive(secondButtonAction != null);

            gameObject.SetActive(true);

            return this;
        }

        /// <inheritdoc />
        public virtual void Dismiss()
        {
            firstButtonAction = null;
            secondButtonAction = null;
            
            if (hasDismissed) { return; }
            hasDismissed = true;
            gameObject.SetActive(false);
        }

        /// <inheritdoc />
        public GameObject VisibleRoot => gameObject;
    }
    
}