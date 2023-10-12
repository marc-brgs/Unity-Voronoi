using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    private GameObject myObject;
    private LineRenderer lr;
    
    // Start is called before the first frame update
    void Start()
    {
        myObject = this.gameObject;
        myObject.name = "PolygonToClip";
        
        //Components
        lr = myObject.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
