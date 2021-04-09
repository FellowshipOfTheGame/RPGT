using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour{
    private Transform cam;
    void Start(){
        cam = Camera.main.transform;    
    }
    
    void LateUpdate(){
        transform.rotation = cam.rotation;
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }
}
