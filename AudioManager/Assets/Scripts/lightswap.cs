using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightswap : MonoBehaviour
{
    public GameObject light1, light2;
    public bool swap;

    // Update is called once per frame
    void FixedUpdate()
    {
        light1.SetActive(swap);
        light2.SetActive(!swap);

        swap = !swap;
    }
}
