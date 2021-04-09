using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEdit : MonoBehaviour{    
    private void OnMouseOver(){
        Vector2Int coord = gameObject.GetComponentInParent<PathCoord>().coord;
        MapMaker.PlaceHighlight(coord);
    }

    private void OnMouseDown(){
        Vector2Int coord = gameObject.GetComponentInParent<PathCoord>().coord;
        MapMaker.UpdateVoxel(coord);
    }
}