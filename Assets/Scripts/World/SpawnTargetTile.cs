using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTargetTile : MonoBehaviour{
    private Session session;
    private Vector2Int coord;
    private GameObject targetTile;
    private TileManager tileManager;

    private void Start(){
        session = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<Session>();
        tileManager = GameObject.Find("TileHandler").GetComponent<TileManager>();
        coord = gameObject.GetComponent<PathCoord>().coord;
        targetTile = gameObject.transform.GetChild(0).gameObject;
    }

    private void OnMouseOver(){
        targetTile?.SetActive(true); // TODO verificar por que não funciona sem ?. ao usar uma habilidade
        if (tileManager?.pathLayer.childCount == 0) 
            Player.localPlayer.GetComponent<PlayerMovement>().DrawPath(coord);
    }

    private void OnMouseExit(){
        targetTile.SetActive(false);
        tileManager.ClearPathInstances();
    }

    public void OnMouseDown(){
        NetworkSession.singleton.CmdMove(coord);
        TileManager.singleton.ClearInstances();
        TileManager.singleton.ClearPathInstances();
    }
}