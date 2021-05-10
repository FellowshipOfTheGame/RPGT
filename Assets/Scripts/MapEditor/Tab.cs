using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tab : MonoBehaviour{
    public VoxelData.VoxelType tabContent;
    private Button button;
    
    private void Start(){
        Debug.Log(MapMaker.singleton);
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => MapMaker.singleton.ChangeHotbarContent(tabContent)); 
    }
}
