/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using PicoMRDemo.Runtime.UI;
using UnityEngine;
using System.Linq;

public class SpatialMesh : MonoBehaviour
{
    private static SpatialMesh _instance = null;
    public ParticleSystem shootEffect;
    public GameObject gun;
    public GameObject ballPrefab;
    public Transform firePoint;
    private HashSet<PaintBallDemo> PaintBalls { get; } = new HashSet<PaintBallDemo>();
    private HashSet<ParticleSystem> PaintEffects { get; } = new HashSet<ParticleSystem>();
    public static SpatialMesh Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SpatialMesh>();
            }
            return _instance;
        }
    }

    private void Start()
    {
        ControllerManager.Instance.BingingTriggerHotKey(false, (args) =>
        {
            Shoot();
        });
    }

    private void Shoot()
    {
        var gunTransform = gun.transform;
        var position = firePoint.position;
        var forward = gunTransform.forward;
        var paintball = AddBall(position, forward, forward * 10);
        PaintBalls.Add(paintball);
        shootEffect.Play();
    }
    
    private PaintBallDemo AddBall(Vector3 position, Vector3 direction, Vector3 velocity)
    {
        var obj = Instantiate(ballPrefab, position, Quaternion.LookRotation(direction));
        // obj.transform.position = position;
        obj.SetActive(true);
        obj.GetComponent<Rigidbody>().velocity = velocity;
        var ballMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        ballMaterial.color = new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f));
        obj.GetComponent<Renderer>().material = ballMaterial;
        var paintball = obj.GetComponent<PaintBallDemo>();
        paintball.ballColor = ballMaterial.color;
        return paintball;
    }
    
    public void AddPaintEffect(ParticleSystem ps)
    {
        PaintEffects.Add(ps);
    }

    private void RemoveAllPaintBalls()
    {
        var copiedList = PaintBalls.ToList();
        foreach (var paintBall in copiedList)
        {
            if (paintBall)
            {
                RemoveBall(paintBall);
            }
        }
    }

    private void RemoveBall(PaintBallDemo paintBall)
    {
        PaintBalls.Remove(paintBall);
        if (paintBall.gameObject)
        {
            DestroyImmediate(paintBall.gameObject,true);
        }
    }


    private void RemovePaintBallEffect(ParticleSystem paintBallEffect)
    {
        PaintEffects.Remove(paintBallEffect);
        if (paintBallEffect.gameObject)
        {
            DestroyImmediate(paintBallEffect.gameObject, true);
        }
    }
    public void Update()
    {
        var copiedList = PaintEffects.ToList();
        foreach (ParticleSystem ps in copiedList)
        {
            if (ps.isStopped)
            {
                RemovePaintBallEffect(ps);
            }
        }
    }
}

