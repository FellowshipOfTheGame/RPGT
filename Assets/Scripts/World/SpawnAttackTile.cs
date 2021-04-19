using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAttackTile : MonoBehaviour
{
    private Session session;
    private Vector2Int coord;
    private TileManager tileManager;

    private void Start(){
        session = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<Session>();
        tileManager = GameObject.Find("TileHandler").GetComponent<TileManager>();
        coord = gameObject.GetComponent<PathCoord>().coord;
    }

    private void OnMouseOver(){
        // Debug.Log("Hovering attack range tile");
        if (tileManager.tileLayers[(int) TileManager.MarkerEnum.Attack].childCount == 0)
            Player.localPlayer.GetComponent<PlayerAttack>().DrawAttackTiles(coord);
    }

    private void OnMouseExit(){
        tileManager.ClearInstances(TileManager.MarkerEnum.Attack);
    }

    public void OnMouseDown(){
        Debug.Log("Enviando skill " + Player.localPlayer.GetComponent<PlayerAttack>().curSkill + " " + Player.localPlayer.GetComponent<PlayerAttack>().curSkill.GetType());
        NetworkSession.singleton.CmdUseSkill(Player.localPlayer.GetComponent<PlayerAttack>().curSkill, coord);
        tileManager.ClearInstances();
    }
}
