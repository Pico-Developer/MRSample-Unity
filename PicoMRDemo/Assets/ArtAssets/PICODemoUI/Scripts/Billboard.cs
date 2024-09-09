using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        transform.LookAt(2.0f * transform.position - targetCamera.transform.position, Vector3.up);
    }
}