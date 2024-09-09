using UnityEngine;

namespace PXR.Benchmark.UI
{
    public class UserMenuUIController : MonoBehaviour
    {
        [SerializeField]
        private Camera targetCamera;

        [SerializeField]
        private bool lazyFollow = false; // Checkbox to enable/disable lazy follow

        [SerializeField]
        [Range(0.01f, 1f)]
        private float followSpeed = 0.05f; // Speed at which the billboard follows the camera

        [SerializeField]
        private float distanceOffset = 3.0f; // Desired distance from the camera in local Z axis of the canvas

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            // Calculate target world position
            Vector3 targetWorldPosition = targetCamera.transform.position + targetCamera.transform.forward * distanceOffset;

            // Convert target world position to local position relative to the parent canvas
            Vector3 targetLocalPosition = transform.parent.InverseTransformPoint(targetWorldPosition);
            Vector3 currentLocalPosition = transform.localPosition;
            targetLocalPosition.z = currentLocalPosition.z; // Maintain current local Z

            // Only apply lazy follow to X and Y, maintaining Z
            Vector3 newPosition = new Vector3(
                lazyFollow ? Mathf.Lerp(currentLocalPosition.x, targetLocalPosition.x, followSpeed * Time.deltaTime) : targetLocalPosition.x,
                lazyFollow ? Mathf.Lerp(currentLocalPosition.y, targetLocalPosition.y, followSpeed * Time.deltaTime) : targetLocalPosition.y,
                currentLocalPosition.z
            );

            // Convert the target local position back to world space for movement
            transform.localPosition = newPosition;

            // Set rotation to face forward relative to the camera, maintaining upright orientation
            Vector3 forwardDirection = new Vector3(targetCamera.transform.forward.x, 0, targetCamera.transform.forward.z).normalized;
            transform.rotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
        }
    }
}
