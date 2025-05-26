using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneReplacer : MonoBehaviour
{
    public static void ReplaceBones(SkinnedMeshRenderer anchor, SkinnedMeshRenderer target)
    {
        Transform[] anchorBones = anchor.bones;
        Transform[] targetBones = target.bones;

        for (int i = 0; i < targetBones.Length; i++)
        {
            for (int j = 0; j < anchorBones.Length; j++)
            {
                if (targetBones[i].name == anchorBones[j].name)
                {
                    Destroy(targetBones[i].gameObject);
                    targetBones[i] = anchorBones[j];
                }
            }
        }
        target.bones = targetBones;
    }
}
