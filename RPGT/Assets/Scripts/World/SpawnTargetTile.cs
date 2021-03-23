using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTargetTile : MonoBehaviour{
    private Session session;
    private Vector2Int coord;
    private GameObject targetTile;

    private void Start(){
        session = GameObject.Find("GameHandler").GetComponent<Session>();
        coord = gameObject.GetComponent<PathCoord>().coord;
        targetTile = gameObject.transform.GetChild(0).gameObject;
    }

    private void OnMouseOver(){
        targetTile.SetActive(true);
        session.DrawPath(coord);
    }

    private void OnMouseExit(){
        targetTile.SetActive(false);
        session.ClearPathInstances();
    }

    public void OnMouseDown(){
        session.Move(coord);
    }
}