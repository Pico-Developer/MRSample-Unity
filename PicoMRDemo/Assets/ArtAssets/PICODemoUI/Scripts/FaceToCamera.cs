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

public class FaceToCamera : MonoBehaviour
{
	private Transform m_Camera;
	// Start is called before the first frame update
	private void Start()
	{
		if (Camera.main != null) 
			m_Camera = Camera.main.transform;
	}

	private void LateUpdate()
	{
		if (m_Camera == null)
		{
			return;
		}
		transform.rotation = Quaternion.LookRotation (transform.position - m_Camera.position);
	}
}
