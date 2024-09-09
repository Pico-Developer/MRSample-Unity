/*******************************************************************************
Copyright © 2015-2024 PICO Technology Co., Ltd.All rights reserved.

NOTICE：All information contained herein is, and remains the property of
PICO Technology Co., Ltd. The intellectual and technical concepts
contained herein are proprietary to PICO Technology Co., Ltd. and may be
covered by patents, patents in process, and are protected by trade secret or
copyright law. Dissemination of this information or reproduction of this
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd.
*******************************************************************************/
using UnityEngine;

public class FollowEyeTracker : MonoBehaviour
{

    public enum ELayout
    {
        Center,
        Left,
        Right,
    }

    public ELayout followLayout;
    public float layoutFollowAngle = 30; //angle of crosswise
    public float maxDistance = 1; //distance aHead camera
    public float followDistance = 0.3f; //follow distance
    public float followSpeed = 3; //follow speed

    private bool _follow;
    private bool _smooth;
    private RectTransform _cacheRectTransform;
    private Transform _cameraTransform;
    private Vector3 _boxSize = new(1, 1, 0.01f);
    private RaycastHit _hitRaycast;
    private bool _hitDetect;

    private Quaternion _layoutRotation;
    private Vector3 _goalPosition;
    private Quaternion _goalRotation;

    private void Awake()
    {
        _cacheRectTransform = transform as RectTransform;
        _cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        var pos = RefreshUIPos();

        _goalPosition = pos;
        var rotation = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0);
        _goalRotation = followLayout == ELayout.Center ? rotation : rotation * _layoutRotation;

        transform.position = _goalPosition;
        transform.rotation = _goalRotation;

        _follow = true;
        _smooth = true;
    }

    private void OnDisable()
    {
        _follow = false;
    }

    Vector3 RefreshUIPos()
    {
        if (_cacheRectTransform == null)
        {
            return Vector3.zero;
        }
        var sizeDelta = _cacheRectTransform.sizeDelta;
        _boxSize.x = sizeDelta.x / 1000;
        _boxSize.y = sizeDelta.y / 1000;
        var direction = _cameraTransform.forward;
        if (followLayout == ELayout.Left)
        {
            _layoutRotation = Quaternion.Euler(0, -layoutFollowAngle, 0);
            direction = _layoutRotation * _cameraTransform.forward;
        }
        else if (followLayout == ELayout.Right)
        {
            _layoutRotation = Quaternion.Euler(0, layoutFollowAngle, 0);
            direction = _layoutRotation * _cameraTransform.forward;
        }

        var layer = 1 << LayerMask.NameToLayer("UIBox");
        _hitDetect = Physics.BoxCast(_cameraTransform.position, _boxSize * 0.5f, direction, out _hitRaycast,
            _cameraTransform.rotation, maxDistance, layer, QueryTriggerInteraction.Collide);
        Vector3 pos;
        if (_hitDetect)
        {
            pos = _cameraTransform.position + direction * _hitRaycast.distance;
        }
        else
        {
            pos = _cameraTransform.position + direction * maxDistance;
        }
        
        pos.y = _cameraTransform.position.y;
        return pos;
    }


    private void Update()
    {
        if (_cameraTransform && _follow)
        {
            var pos = RefreshUIPos();
            if (_smooth)
            {
                _goalPosition = pos;
                var rotation = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0);
                _goalRotation = followLayout == ELayout.Center ? rotation : rotation * _layoutRotation;
            }

            var offset = transform.position - pos;
            if (offset.sqrMagnitude > followDistance)
            {
                _smooth = true;
            }
            else if (offset.sqrMagnitude < 0.01f)
            {
                _smooth = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (_cameraTransform && _follow)
        {
            transform.position = Vector3.Lerp(transform.position, _goalPosition, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, _goalRotation, Time.deltaTime * followSpeed);
        }
    }

}
