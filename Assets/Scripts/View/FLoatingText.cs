using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FLoatingText : MonoBehaviour
{
    Transform mainCam;
    Transform unit;
    Transform canvas;

    public Vector3 offset;
    void Start()
    {
        mainCam = Camera.main.transform;
        unit = transform.parent;
        canvas = GameObject.FindObjectOfType<Canvas>().transform;
        transform.SetParent(canvas);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
