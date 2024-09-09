using TMPro;
using UnityEngine;

namespace PXR.Benchmark.UI
{
    public class TeleportationAnchorVisual : MonoBehaviour
    {
        [SerializeField] private TMP_Text _indexText;

        public void SetIndexText(int index)
        {
            _indexText.text = index.ToString();
        }

        public void ToggleText(bool enable)
        {
            _indexText.enabled = enable;
        }
    }
}
