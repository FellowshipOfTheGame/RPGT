using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tab : MonoBehaviour{
    public VoxelData.VoxelType tabContent;
    private Button button;
    
    private void Start(){
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => MapMaker.singleton.ChangeHotbarContent(tabContent)); 
    }
}
