using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseKinematic : MonoBehaviour
{
    public Transform target, effector;
    public int maxIterations;
    public float minError;
    public Transform[] bones;
    public Vector3[] clampAxes;

    // Assume x-z axis only for now
    // Rotate towards target over the axis

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
            MoveToTarget();
    }

    public void MoveToTarget()
    {
        int iteration = maxIterations;

        while (iteration > 0)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                Vector3 localPrev = ClampRotation(bones[i].transform.localEulerAngles);
                Vector3 boneToEffector = effector.position - bones[i].position;
                Vector3 boneToTarget = target.position - bones[i].position;
                Quaternion toRotate = Quaternion.FromToRotation(boneToEffector, boneToTarget);
                Vector3 eulerRotation = toRotate.eulerAngles;
                Vector3 localEnd = ClampRotation((bones[i].localRotation * toRotate).eulerAngles);
                Vector3 angleChange = eulerRotation;
                angleChange = new Vector3(angleChange.x * clampAxes[i].x, angleChange.y * clampAxes[i].y, angleChange.z * clampAxes[i].z);
                bones[i].localEulerAngles += angleChange;
            }

            if((target.position - effector.position).magnitude <= minError)
            {
                break;
            }

            iteration--;
        }
    }
    IEnumerator DebugMove()
    {
        int iteration = maxIterations;

        while (iteration > 0)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                Vector3 localPrev = ClampRotation(bones[i].transform.localEulerAngles);
                Vector3 boneToEffector = effector.position - bones[i].position;
                Vector3 boneToTarget = target.position - bones[i].position;
                Quaternion toRotate = Quaternion.FromToRotation(boneToEffector, boneToTarget);
                Vector3 localEnd = ClampRotation((bones[i].localRotation * toRotate).eulerAngles);
                Vector3 angleChange = localEnd - localPrev;
                angleChange = new Vector3(angleChange.x * clampAxes[i].x, angleChange.y * clampAxes[i].y, angleChange.z * clampAxes[i].z);
                bones[i].localEulerAngles += angleChange;
                yield return new WaitForSeconds(1);
            }

            if ((target.position - effector.position).magnitude <= minError)
            {
                break;
            }

            iteration--;
        }
    }
    public Vector3 ClampRotation(Vector3 rotation)
    {
        Vector3 ret = new Vector3(ClampAngle(rotation.x), ClampAngle(rotation.y), ClampAngle(rotation.z));
        return ret;
    }
    public float ClampAngle(float angle)
    {
        while(angle > 360)
        {
            angle -= 360;
        }
        while(angle < 0)
        {
            angle += 360;
        }
        return angle;
    }
}
