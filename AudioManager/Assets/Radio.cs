using System.Collections;
using System.Collections.Generic;
using AudioScripts;
using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private SourceParams sourceParams;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AudioManager.Instance.PlayRandom(clips, transform.position,sourceParams);
        }
    }
}
