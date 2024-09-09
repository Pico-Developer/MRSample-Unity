/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using VContainer;
using PicoMRDemo.Runtime.Runtime.ShootingGame;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System;

namespace PicoMRDemo.Runtime.UI
{
    public enum ControllerState
    {
        Normal,
        Doodle,
        ShootGame,
        VirtualWorld,
        BallDrop,
        AnchorCreate
    }
    public class ControllerManager : MonoBehaviour
    {
        public static ControllerManager Instance;
        public GameObject LeftControllerRoot;
        public GameObject RightControllerRoot;
        public Transform LeftControllerPreviewPoint;
        public Transform RightControllerPreviewPoint;
        public InputActionReference menuBtnLeft;
        public InputActionReference primaryBtnLeft;
        public InputActionReference primaryBtnPressedLeft;
        public InputActionReference primaryBtnUpLeft;
        public InputActionReference primaryBtnRight;
        public InputActionReference primaryBtnPressedRight;
        public InputActionReference primaryBtnUpRight;
        public InputActionReference gripBtnLeft;
        public InputActionReference gripBtnPressedLeft;
        public InputActionReference gripBtnUpLeft;
        public InputActionReference gripBtnRight;
        public InputActionReference gripBtnPressedRight;
        public InputActionReference gripBtnUpRight;
        public InputActionReference triggerBtnLeft;
        public InputActionReference triggerBtnRight;
        public InputActionReference secondaryBtnLeft;
        public InputActionReference secondaryBtnPressedLeft;
        public InputActionReference secondaryBtnUpLeft;
        public InputActionReference secondaryBtnRight;
        public InputActionReference secondaryBtnPressedRight;
        public InputActionReference secondaryBtnUpRight;
        public InputActionReference rotateAnchorHorizontalActionLeft;
        public InputActionReference rotateAnchorHorizontalActionRight;
        public InputActionReference rotateAnchorVerticalActionLeft;
        public InputActionReference rotateAnchorVerticalActionRight;
        private Action<InputAction.CallbackContext> triggerInputLeft;
        private Action<InputAction.CallbackContext> triggerInputRight;
        private Action<InputAction.CallbackContext> gripInputLeft;
        private Action<InputAction.CallbackContext> gripInputPressedLeft;
        private Action<InputAction.CallbackContext> gripInputUpLeft;
        private Action<InputAction.CallbackContext> gripInputRight;
        private Action<InputAction.CallbackContext> gripInputPressedRight;
        private Action<InputAction.CallbackContext> gripInputUpRight;
        private Action<InputAction.CallbackContext> primaryInputLeft;
        private Action<InputAction.CallbackContext> primaryInputPressedLeft;
        private Action<InputAction.CallbackContext> primaryInputUpLeft;
        private Action<InputAction.CallbackContext> primaryInputRight;
        private Action<InputAction.CallbackContext> primaryInputPressedRight;
        private Action<InputAction.CallbackContext> primaryInputUpRight;
        private Action<InputAction.CallbackContext> secondaryInputLeft;
        private Action<InputAction.CallbackContext> secondaryInputPressedLeft;
        private Action<InputAction.CallbackContext> secondaryInputUpLeft;
        private Action<InputAction.CallbackContext> secondaryInputRight;
        private Action<InputAction.CallbackContext> secondaryInputPressedRight;
        private Action<InputAction.CallbackContext> secondaryInputUpRight;
        private Action<InputAction.CallbackContext> rotateAnchorHorizontalInputActionLeft;
        private Action<InputAction.CallbackContext> rotateAnchorHorizontalInputActionRight;
        private Action<InputAction.CallbackContext> rotateAnchorVerticalInputActionLeft;
        private Action<InputAction.CallbackContext> rotateAnchorVerticalInputActionRight;
        private readonly string _tag = nameof(ControllerManager);
        [HideInInspector]
        private ControllerState leftControllerState = ControllerState.Normal;
        [HideInInspector]
        private ControllerState rightControllerState = ControllerState.Normal;
        [Inject]
        private IShootingGameManager _shootingGameManager;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
        }

        private void OnEnable()
        {
            RegisterEvent();
        }

        private void OnDisable()
        {
            UnregisterEvent();
        }
        
        private void RegisterEvent()
        {
           
        }

        private void UnregisterEvent()
        {
            
        }
        public void BindingMainMenuHotKey()
        {
            if (primaryBtnLeft != null)
            {
                primaryBtnLeft.action.Enable();
                primaryBtnLeft.action.performed += OpenMenu;
                menuBtnLeft.action.Enable();
                menuBtnLeft.action.performed += OpenMenu;
            }

            if (primaryBtnRight != null)
            {
                primaryBtnRight.action.Enable();
                primaryBtnRight.action.performed += OpenMenu;
            }
        }

        public void SetControllerState(bool isLeftController, ControllerState state)
        {
            if (isLeftController)
            {
                leftControllerState = state;
            }
            else
            {
                rightControllerState = state;
            }
        }
        
        public ControllerState GetControllerState(bool isLeftController)
        {
            if (isLeftController)
            {
                return leftControllerState;
            }
            else
            {
                return rightControllerState;
            }
        }
        public GameObject GetController(bool isLeftController)
        {
            if (isLeftController)
            {
                return LeftControllerRoot;
            }
            else
            {
                return RightControllerRoot;
            }
        }
        public void BingingTriggerHotKey(bool isLeftController,Action<InputAction.CallbackContext>  downAction)
        {
            if (isLeftController)
            {
                if (triggerInputLeft != null)
                {
                    UnBingingTriggerInputActionLeft();
                }
                triggerInputLeft = downAction;
                if(triggerBtnLeft != null && triggerInputLeft != null)
                {
                    triggerBtnLeft.action.Enable();
                    triggerBtnLeft.action.performed += triggerInputLeft;
                }
            }
            else
            {
                if (triggerInputRight != null)
                {
                    UnBingingTriggerInputActionRight();
                }
                triggerInputRight = downAction;
                if(triggerBtnRight != null && triggerInputRight != null)
                {
                    triggerBtnRight.action.Enable();
                    triggerBtnRight.action.performed += triggerInputRight;
                }
            }
        }
        public void BingingGripHotKey(bool isLeftController,Action<InputAction.CallbackContext>  downAction,Action<InputAction.CallbackContext>  pressedAction,Action<InputAction.CallbackContext>  upAction)
        {
            if (isLeftController)
            {
                if (gripInputLeft != null||
                    gripInputPressedLeft != null||
                    gripInputUpLeft != null)
                {
                    UnBingingGripInputActionLeft();
                }
                gripInputLeft = downAction;
                gripInputPressedLeft = pressedAction;
                gripInputUpLeft = upAction;
                if(gripBtnLeft != null && gripInputLeft != null)
                {
                    gripBtnLeft.action.Enable();
                    gripBtnLeft.action.performed += gripInputLeft;
                }
                if (gripBtnPressedLeft != null && gripInputPressedLeft!= null)
                {
                    gripBtnPressedLeft.action.Enable();
                    gripBtnPressedLeft.action.performed += gripInputPressedLeft;
                }
                if (gripBtnUpLeft != null && gripInputUpLeft!= null)
                {
                    gripBtnUpLeft.action.Enable();
                    gripBtnUpLeft.action.performed += gripInputUpLeft;
                }
            }
            else
            {
                if (gripInputRight != null||
                    gripInputPressedRight != null||
                    gripInputUpRight != null)
                {
                    UnBingingGripInputActionRight();
                }
                gripInputRight = downAction;
                gripInputPressedRight = pressedAction;
                gripInputUpRight = upAction;
                if(gripBtnRight != null && gripInputRight != null)
                {
                    gripBtnRight.action.Enable();
                    gripBtnRight.action.performed += gripInputRight;
                }
                if (gripBtnPressedRight != null && gripInputPressedRight!= null)
                {
                    gripBtnPressedRight.action.Enable();
                    gripBtnPressedRight.action.performed += gripInputPressedRight;
                }
                if (gripBtnUpRight != null && gripInputUpRight!= null)
                {
                    gripBtnUpRight.action.Enable();
                    gripBtnUpRight.action.performed += gripInputUpRight;
                }
            }
        }
        public void BingingPrimaryHotKey(bool isLeftController,Action<InputAction.CallbackContext>  downAction,Action<InputAction.CallbackContext>  pressedAction,Action<InputAction.CallbackContext>  upAction)
        {
            if (isLeftController)
            {
                if (primaryInputLeft != null||
                    primaryInputPressedLeft!= null||
                    primaryInputUpLeft!= null)
                {
                    UnBingingPrimaryInputActionLeft();
                }
                primaryInputLeft = downAction;
                primaryInputPressedLeft = pressedAction;
                primaryInputUpLeft = upAction;
                if (primaryBtnLeft != null && primaryInputLeft!= null)
                {
                    primaryBtnLeft.action.Enable();
                    primaryBtnLeft.action.performed += primaryInputLeft;
                }
                if (primaryBtnPressedLeft != null && primaryInputPressedLeft!= null)
                {
                    primaryBtnPressedLeft.action.Enable();
                    primaryBtnPressedLeft.action.performed += primaryInputPressedLeft;
                }
                if (primaryBtnUpLeft != null && primaryInputUpLeft!= null)
                {
                    primaryBtnUpLeft.action.Enable();
                    primaryBtnUpLeft.action.performed += primaryInputUpLeft;
                }
            }
            else
            {
                if (primaryInputRight != null||
                    primaryInputPressedRight!= null||
                    primaryInputUpRight!= null)
                {
                    UnBingingPrimaryInputActionRight();
                }
                primaryInputRight = downAction;
                primaryInputPressedRight = pressedAction;
                primaryInputUpRight = upAction;
                if (primaryBtnRight != null && primaryInputRight != null)
                {
                    primaryBtnRight.action.Enable();
                    primaryBtnRight.action.performed += primaryInputRight;
                }
                if (primaryBtnPressedRight != null && primaryInputPressedRight!= null)
                {
                    primaryBtnPressedRight.action.Enable();
                    primaryBtnPressedRight.action.performed += primaryInputPressedRight;
                }
                if (primaryBtnUpRight != null && primaryInputUpRight!= null)
                {
                    primaryBtnUpRight.action.Enable();
                    primaryBtnUpRight.action.performed += primaryInputUpRight;
                }
            }
        }
        public void BingingSecondaryHotKey(bool isLeftController,Action<InputAction.CallbackContext>  downAction,Action<InputAction.CallbackContext>  pressedAction,Action<InputAction.CallbackContext>  upAction)
        {
            if (isLeftController)
            {
                if (secondaryInputLeft != null||
                    secondaryInputPressedLeft!= null||
                    secondaryInputUpLeft!= null)
                {
                    UnBingingSecondaryInputActionLeft();
                }
                secondaryInputLeft = downAction;
                secondaryInputPressedLeft = pressedAction;
                secondaryInputUpLeft = upAction;
                if (secondaryBtnLeft != null && secondaryInputLeft!= null)
                {
                    secondaryBtnLeft.action.Enable();
                    secondaryBtnLeft.action.performed += secondaryInputLeft;
                }
                if (secondaryBtnPressedLeft != null && secondaryInputPressedLeft!= null)
                {
                    secondaryBtnPressedLeft.action.Enable();
                    secondaryBtnPressedLeft.action.performed += secondaryInputPressedLeft;
                }
                if (secondaryBtnUpLeft != null && secondaryInputUpLeft!= null)
                {
                    secondaryBtnUpLeft.action.Enable();
                    secondaryBtnUpLeft.action.performed += secondaryInputUpLeft;
                }
            }
            else
            {
                if (secondaryInputRight != null||
                    secondaryInputPressedRight!= null||
                    secondaryInputUpRight!= null)
                {
                    UnBingingSecondaryInputActionRight();
                }
                secondaryInputRight = downAction;
                secondaryInputPressedRight = pressedAction;
                secondaryInputUpRight = upAction;
                if (secondaryBtnRight != null && secondaryInputRight != null)
                {
                    secondaryBtnRight.action.Enable();
                    secondaryBtnRight.action.performed += secondaryInputRight;
                }
                if (secondaryBtnPressedRight != null && secondaryInputPressedRight!= null)
                {
                    secondaryBtnPressedRight.action.Enable();
                    secondaryBtnPressedRight.action.performed += secondaryInputPressedRight;
                }
                if (secondaryBtnUpRight != null && secondaryInputUpRight!= null)
                {
                    secondaryBtnUpRight.action.Enable();
                    secondaryBtnUpRight.action.performed += secondaryInputUpRight;
                }
            }
        }
        
        public void BingingRotateAnchorHorizontalActionHotKey(bool isLeftController,Action<InputAction.CallbackContext>  rotateAnchorHorizontalAction)
        {
            if (isLeftController)
            {
                if (rotateAnchorHorizontalInputActionLeft != null)
                {
                    UnBingingRotateAnchorHorizontalInputActionLeft();
                }
                rotateAnchorHorizontalInputActionLeft = rotateAnchorHorizontalAction;
                if(rotateAnchorHorizontalActionLeft != null && rotateAnchorHorizontalInputActionLeft != null)
                {
                    rotateAnchorHorizontalActionLeft.action.Enable();
                    rotateAnchorHorizontalActionLeft.action.performed += rotateAnchorHorizontalInputActionLeft;
                }
            }
            else
            {
                if (rotateAnchorHorizontalInputActionRight != null)
                {
                    UnBingingRotateAnchorHorizontalInputActionRight();
                }
                rotateAnchorHorizontalInputActionRight = rotateAnchorHorizontalAction;
                if(rotateAnchorHorizontalActionRight != null && rotateAnchorHorizontalInputActionRight != null) 
                {
                    rotateAnchorHorizontalActionRight.action.Enable();
                    rotateAnchorHorizontalActionRight.action.performed += rotateAnchorHorizontalInputActionRight;
                }
            }
        }
        
        public void BingingRotateAnchorVerticalActionHotKey(bool isLeftController,Action<InputAction.CallbackContext>  rotateAnchorVerticalAction)
        {
            if (isLeftController)
            {
                if (rotateAnchorVerticalInputActionLeft != null)
                {
                    UnBingingRotateAnchorVerticalInputActionLeft();
                }
                rotateAnchorVerticalInputActionLeft = rotateAnchorVerticalAction;
                if(rotateAnchorVerticalActionLeft != null && rotateAnchorVerticalInputActionLeft != null)
                {
                    rotateAnchorVerticalActionLeft.action.Enable();
                    rotateAnchorVerticalActionLeft.action.performed += rotateAnchorVerticalInputActionLeft;
                }
            }
            else
            {
                if (rotateAnchorVerticalInputActionRight != null)
                {
                    UnBingingRotateAnchorVerticalInputActionRight();
                }
                rotateAnchorVerticalInputActionRight = rotateAnchorVerticalAction;
                if(rotateAnchorVerticalActionRight != null && rotateAnchorVerticalInputActionRight != null) 
                {
                    rotateAnchorVerticalActionRight.action.Enable();
                    rotateAnchorVerticalActionRight.action.performed += rotateAnchorVerticalInputActionRight;
                }
            }
        }

        public void SetControllerShow(bool isLeftController, bool isShowModel, bool isShowLine)
        {
            if (isLeftController)
            {
                SetLeftControllerStatus(isShowModel, isShowLine);
            }
            else
            {
                SetRightControllerStatus(isShowModel, isShowLine);
            }
        }

        private void SetLeftControllerStatus(bool isShowModel, bool isShowLine)
        {
            if(LeftControllerRoot.GetComponent<ActionBasedController>().model != null)
                LeftControllerRoot.GetComponent<ActionBasedController>().model.gameObject.SetActive(isShowModel);
            if(LeftControllerRoot.GetComponent<XRRayInteractor>() != null)
                LeftControllerRoot.GetComponent<XRRayInteractor>().enabled = isShowLine;
        }

        private void SetRightControllerStatus(bool isShowModel, bool isShowLine)
        {
            if(RightControllerRoot.GetComponent<ActionBasedController>().model != null)
                RightControllerRoot.GetComponent<ActionBasedController>().model.gameObject.SetActive(isShowModel);
            if(RightControllerRoot.GetComponent<XRRayInteractor>() != null)
                RightControllerRoot.GetComponent<XRRayInteractor>().enabled = isShowLine;
        }
        public void UnBingingGameHotKey(bool isLeftController)
        {
            if (isLeftController)
            {
                UnBingingTriggerInputActionLeft();
                UnBingingGripInputActionLeft();
                UnBingingRotateAnchorHorizontalInputActionLeft();
                UnBingingRotateAnchorVerticalInputActionLeft();
                UnBingingSecondaryInputActionLeft();
                SetLeftControllerStatus(true,true);
            }
            else
            {
                UnBingingTriggerInputActionRight();
                UnBingingGripInputActionRight();
                UnBingingRotateAnchorHorizontalInputActionLeft();
                UnBingingRotateAnchorVerticalInputActionRight();
                UnBingingSecondaryInputActionRight();
                SetRightControllerStatus(true,true);
            }
        }

        private void UnBingingTriggerInputActionLeft()
        {
            if (triggerBtnLeft != null && triggerInputLeft != null)
            {
                triggerBtnLeft.action.Disable();
                triggerBtnLeft.action.performed -= triggerInputLeft;
                triggerInputLeft = null;
            }
        }

        private void UnBingingTriggerInputActionRight()
        {
            if (triggerBtnRight != null && triggerInputRight != null)
            {
                triggerBtnRight.action.Disable();
                triggerBtnRight.action.performed -= triggerInputRight;
                triggerInputRight = null;
            }
        }

        private void UnBingingGripInputActionLeft()
        {
            if (gripBtnLeft != null && gripInputLeft != null)
            {
                gripBtnLeft.action.Disable();
                gripBtnLeft.action.performed -= gripInputLeft;
                gripInputLeft = null;
            }
            if (gripBtnPressedLeft != null && gripInputPressedLeft!= null)
            {
                gripBtnPressedLeft.action.Disable();
                gripBtnPressedLeft.action.performed -= gripInputPressedLeft;
                gripInputPressedLeft = null;
            }
            if (gripBtnUpLeft != null && gripInputUpLeft!= null)
            {
                gripBtnUpLeft.action.Disable();
                gripBtnUpLeft.action.performed -= gripInputUpLeft;
                gripInputUpLeft = null;
            }
        }

        private void UnBingingGripInputActionRight()
        {
            if (gripBtnRight != null && gripInputRight != null)
            {
                gripBtnRight.action.Disable();
                gripBtnRight.action.performed -= gripInputRight;
                gripInputRight = null;
            }
            if (gripBtnPressedRight != null && gripInputPressedRight!= null)
            {
                gripBtnPressedRight.action.Disable();
                gripBtnPressedRight.action.performed -= gripInputPressedRight;
                gripInputPressedRight = null;
            }
            if (gripBtnUpRight != null && gripInputUpRight!= null)
            {
                gripBtnUpRight.action.Disable();
                gripBtnUpRight.action.performed -= gripInputUpRight;
                gripInputUpRight = null;
            }
        }

        private void UnBingingPrimaryInputActionLeft()
        {
            if (primaryBtnLeft != null && primaryInputLeft!= null)
            {
                primaryBtnLeft.action.Disable();
                primaryBtnLeft.action.performed -= primaryInputLeft;
                primaryInputLeft = null;
            }
            if (primaryBtnPressedLeft != null && primaryInputPressedLeft!= null)
            {
                primaryBtnPressedLeft.action.Disable();
                primaryBtnPressedLeft.action.performed -= primaryInputPressedLeft;
                primaryInputPressedLeft = null;
            }
            if (primaryBtnUpLeft != null && primaryInputUpLeft!= null)
            {
                primaryBtnUpLeft.action.Disable();
                primaryBtnUpLeft.action.performed -= primaryInputUpLeft;
                primaryInputUpLeft = null;
            }
        }
        public void UnBingingPrimaryInputActionRight()
        {
            if (primaryBtnRight != null && primaryInputRight!= null)
            {
                primaryBtnRight.action.Disable();
                primaryBtnRight.action.performed -= primaryInputRight;
                primaryInputRight = null;
            }
            if (primaryBtnPressedRight != null && primaryInputPressedRight!= null)
            {
                primaryBtnPressedRight.action.Disable();
                primaryBtnPressedRight.action.performed -= primaryInputPressedRight;
                primaryInputPressedRight = null;
            }
            if (primaryBtnUpRight != null && primaryInputUpRight!= null)
            {
                primaryBtnUpRight.action.Disable();
                primaryBtnUpRight.action.performed -= primaryInputUpRight;
                primaryInputUpRight = null;
            }
        }
        public void UnBingingSecondaryInputActionLeft()
        {
            if (secondaryBtnLeft != null && secondaryInputLeft!= null)
            {
                secondaryBtnLeft.action.Disable();
                secondaryBtnLeft.action.performed -= secondaryInputLeft;
                secondaryInputLeft = null;
            }
            if (secondaryBtnPressedLeft != null && secondaryInputPressedLeft!= null)
            {
                secondaryBtnPressedLeft.action.Disable();
                secondaryBtnPressedLeft.action.performed -= secondaryInputPressedLeft;
                secondaryInputPressedLeft = null;
            }
            if (secondaryBtnUpLeft != null && secondaryInputUpLeft!= null)
            {
                secondaryBtnUpLeft.action.Disable();
                secondaryBtnUpLeft.action.performed -= secondaryInputUpLeft;
                secondaryInputUpLeft = null;
            }
        }
        public void UnBingingSecondaryInputActionRight()
        {
            if (secondaryBtnRight != null && secondaryInputRight!= null)
            {
                secondaryBtnRight.action.Disable();
                secondaryBtnRight.action.performed -= secondaryInputRight;
                secondaryInputRight = null;
            }
            if (secondaryBtnPressedRight != null && secondaryInputPressedRight!= null)
            {
                secondaryBtnPressedRight.action.Disable();
                secondaryBtnPressedRight.action.performed -= secondaryInputPressedRight;
                secondaryInputPressedRight = null;
            }
            if (secondaryBtnUpRight != null && secondaryInputUpRight!= null)
            {
                secondaryBtnUpRight.action.Disable();
                secondaryBtnUpRight.action.performed -= secondaryInputUpRight;
                secondaryInputUpRight = null;
            }
        }

        private void UnBingingRotateAnchorHorizontalInputActionLeft()
        {
            if (rotateAnchorHorizontalActionLeft != null && rotateAnchorHorizontalInputActionLeft != null)
            {
                rotateAnchorHorizontalActionLeft.action.Disable();
                rotateAnchorHorizontalActionLeft.action.performed -= rotateAnchorHorizontalInputActionLeft;
                rotateAnchorHorizontalInputActionLeft = null;
            }
        }

        private void UnBingingRotateAnchorHorizontalInputActionRight()
        {
            if (rotateAnchorHorizontalActionRight != null && rotateAnchorHorizontalInputActionRight != null)
            {
                rotateAnchorHorizontalActionRight.action.Disable();
                rotateAnchorHorizontalActionRight.action.performed -= rotateAnchorHorizontalInputActionRight;
                rotateAnchorHorizontalInputActionRight = null;
            }
        }

        private void UnBingingRotateAnchorVerticalInputActionLeft()
        {
            if (rotateAnchorVerticalActionLeft != null && rotateAnchorVerticalInputActionLeft != null)
            {
                rotateAnchorVerticalActionLeft.action.Disable();
                rotateAnchorVerticalActionLeft.action.performed -= rotateAnchorVerticalInputActionLeft;
                rotateAnchorVerticalInputActionLeft = null;
            }
        }

        private void UnBingingRotateAnchorVerticalInputActionRight()
        {
            if (rotateAnchorVerticalActionRight != null && rotateAnchorVerticalInputActionRight != null)
            {
                rotateAnchorVerticalActionRight.action.Disable();
                rotateAnchorVerticalActionRight.action.performed -= rotateAnchorVerticalInputActionRight;
                rotateAnchorVerticalInputActionRight = null;
            }
        }

        public void UnBingingRotateAnchorVerticalInputAction()
        {
            UnBingingRotateAnchorVerticalInputActionLeft();
            UnBingingRotateAnchorVerticalInputActionRight();
        }
        
        public void UnBingingRotateAnchorHorizontalInputAction()
        {
            UnBingingRotateAnchorHorizontalInputActionLeft();
            UnBingingRotateAnchorHorizontalInputActionRight();
        }
        
        private void OpenMenu(InputAction.CallbackContext content)
        { 
            UIContext.Instance.ToggleMainMenu();
        }
        
        /// <summary>
        /// Display a preview object in a virtual reality scene
        /// </summary>
        /// <param name="previewObj">The game object of the preview object to be displayed</param>
        /// <param name="isLeft">A Boolean value indicating whether the object is displayed on the left (true) or right (false)</param>
        public void ShowAnchorPreview(GameObject previewObj,bool isLeft)
        {
            GameObject newObj = Instantiate(previewObj, (isLeft?LeftControllerPreviewPoint:RightControllerPreviewPoint).transform.position, (isLeft?LeftControllerPreviewPoint:RightControllerPreviewPoint).rotation);
            newObj.transform.SetParent(isLeft?LeftControllerPreviewPoint:RightControllerPreviewPoint);
            newObj.SetActive(true);
        }

        public void HideAnchorPreview(bool isLeft)
        {
            foreach (Transform child in (isLeft?LeftControllerPreviewPoint:RightControllerPreviewPoint).transform)
            {
                Destroy(child.gameObject);  
            }
        }
    }

    
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> //注意此约束为T必须为其本身或子类
    {
        private static T _instance; //创建私有对象记录取值，可只赋值一次避免多次赋值

        public static T Instance
        {
            //实现按需加载
            get
            {
                //当已经赋值，则直接返回即可
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<T>();

                //为了防止脚本还未挂到物体上，找不到的异常情况，可以自行创建空物体挂上去
                if (_instance == null)
                {
                    //如果创建对象，则会在创建时调用其身上脚本的Awake即调用T的Awake(T的Awake实际上是继承的父类的）
                    //所以此时无需为instance赋值，其会在Awake中赋值，自然也会初始化所以无需init()
                    /*instance = */
                    new GameObject("Singleton of " + typeof(T)).AddComponent<T>();
                }
                else _instance.Init(); //保证Init只执行一次

                return _instance;

            }
        }

        private void Awake()
        {
            //若无其它脚本在Awake中调用此实例，则可在Awake中自行初始化instance
            _instance = this as T;
            //初始化
            Init();
        }

        //子类对成员进行初始化如果放在Awake里仍会出现Null问题所以自行制作一个init函数解决（可用可不用）
        protected virtual void Init()
        {

        }
    }
}