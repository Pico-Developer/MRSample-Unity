using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace PXR.Benchmark.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleAnimation : MonoBehaviour
    {
        [SerializeField]
        private RectTransform uiHandleRectTransform;
        
        [SerializeField]
        private Color backgroundActiveColor;

        [SerializeField]
        private Color handleActiveColor;
        
        [SerializeField]
        private Toggle toggle;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Image handleImage;

        private Color backgroundDefaultColor, handleDefaultColor;

        private Vector2 handlePosition;

        private void Awake()
        {
            if (toggle == null)
            {
                toggle = GetComponent<Toggle>();
            }

            handlePosition = uiHandleRectTransform.anchoredPosition;

            backgroundDefaultColor = backgroundImage.color;
            handleDefaultColor = handleImage.color;

            toggle.onValueChanged.AddListener(OnSwitch);

            if (toggle.isOn)
            {
                OnSwitch(true);
            }
        }

        private void OnSwitch(bool on)
        {
            uiHandleRectTransform.DOAnchorPos(on ? handlePosition * -1.0f : handlePosition, 0.4f).SetEase(Ease.InOutBack);
            backgroundImage.DOColor(on ? backgroundActiveColor : backgroundDefaultColor, 0.6f);
            handleImage.DOColor(on ? handleActiveColor : handleDefaultColor, 0.4f);
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnSwitch);
        }
    }
}
