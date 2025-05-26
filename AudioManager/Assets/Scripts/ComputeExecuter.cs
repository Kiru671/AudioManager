using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeExecuter : MonoBehaviour
{
    ComputeBuffer heightBuffer;
    public ComputeShader shader;
    public int resolution;

    static readonly int
        heightsId = Shader.PropertyToID("_Heights"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time");

    private void OnEnable()
    {
        heightBuffer = new ComputeBuffer(resolution * resolution, 1 * 4);
    }

    private void OnDisable()
    {
        heightBuffer.Release();
        heightBuffer = null;
    }

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        shader.SetInt(resolutionId, resolution);
        shader.SetFloat(stepId, step);
        shader.SetFloat(timeId, Time.time);

        shader.SetBuffer(0, heightsId, heightBuffer);

        int groups = Mathf.CeilToInt(resolution / 16f);

        shader.Dispatch(0, groups, groups, 1);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFunctionOnGPU();
    }
}
