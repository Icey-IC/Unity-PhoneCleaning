using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour
{
    public static GalleryManager Instance;
    public bool multiMode = false;
    public Button deleteButton;
    public Text warningText;

    private List<PhotoItem> selectedPhotos = new List<PhotoItem>();

    // Update is called once per frame
    void Update()
    {
        
    }
}
