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
        if (tileManager.tileLayers[(int) TileManager.MarkerEnum.Attack].childCount == 0)
            Player.localPlayer.GetComponent<PlayerAttack>().DrawAttackTiles(coord);
    }

    private void OnMouseExit(){
        tileManager.ClearInstances(TileManager.MarkerEnum.Attack);
    }

    public void OnMouseDown(){
        NetworkSession.singleton.CmdUseSkill(Player.localPlayer.GetComponent<Entity>().gridCoord, coord, Player.localPlayer.GetComponent<PlayerAttack>().curSkill);
        tileManager.ClearInstances();
    }
}
