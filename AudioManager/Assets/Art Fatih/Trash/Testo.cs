using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testo : MonoBehaviour
{

    public GameObject targetMesh;
    private int index;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
	{
	    Vector3 offset = targetMesh.GetComponent<MeshFilter>().sharedMesh.vertices[index];
	    transform.position = targetMesh.transform.position + offset;
	    index++;
	}
    }
}
