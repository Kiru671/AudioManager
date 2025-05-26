using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CurveFollower : MonoBehaviour
{
    public float moveSpeed;
    public float offset;
    public float t;
    public Vector3 prevPos;
    public TrainCurveTool curve;

    void Start()
    {
        t = 0;
        transform.position = curve.GetPointAtLength(offset);
        prevPos = transform.position;
    }

    void Update()
    {
        t += Time.deltaTime;
        Debug.Log($"Requesting point at {(t * moveSpeed) + offset}");
        transform.position = curve.GetPointAtLength((t * moveSpeed) + offset);
        Vector3 posChange = transform.position - prevPos;
	if(posChange.magnitude > 0.01f)
            transform.forward = posChange.normalized;
        prevPos = transform.position;
    }

}
