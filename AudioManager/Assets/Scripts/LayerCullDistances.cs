using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LayerCullDistances : MonoBehaviour
{

    [System.Serializable]
    public struct LayerCullSetting
    {
        public string layerName;
        public float cullDistance;
    }

    public List<LayerCullSetting> layerCullSettings = new List<LayerCullSetting>();

    private Camera mainCamera;
    private float[] layerCullDistances;

    void Start()
    {
        CullCheck();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            CullCheck();
        }
    }
    void CullCheck(){

        mainCamera = GetComponent<Camera>();
        layerCullDistances = new float[32];

        foreach (LayerCullSetting setting in layerCullSettings)
        {
            int layerIndex = LayerMask.NameToLayer(setting.layerName);
            if (layerIndex != -1)
            {
                layerCullDistances[layerIndex] = setting.cullDistance;
            }
            else
            {
                Debug.LogWarning("Layer \"" + setting.layerName + "\" not found. Please check the layer name in the LayerCullDistances component.");
            }
        }

        mainCamera.layerCullDistances = layerCullDistances;
    }
}