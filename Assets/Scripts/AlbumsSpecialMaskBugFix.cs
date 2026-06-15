using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlbumsSpecialMaskBugFix : MonoBehaviour
{
    public GameObject PhoneBGMask;
    // Start is called before the first frame update
    void OnEnable()
    {
        PhoneBGMask.SetActive(false);
    }

}
