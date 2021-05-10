using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEdit : MonoBehaviour{    
    private void OnMouseOver(){
        Vector2Int coord = gameObject.GetComponent<PathCoord>().coord;
        MapMaker.PlaceHighlight(coord);
        if(Input.GetMouseButton(0)) MapMaker.UpdateVoxel(coord); 
    }

    private void OnMouseExit(){
        MapMaker.highlightBlock.SetActive(false);    
    }

    private void OnMouseDown(){
        Vector2Int coord = gameObject.GetComponent<PathCoord>().coord;
        MapMaker.UpdateVoxel(coord);
    }
}