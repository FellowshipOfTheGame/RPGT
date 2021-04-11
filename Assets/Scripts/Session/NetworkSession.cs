using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSession : NetworkBehaviour
{
    [SyncVar]
    public Entity curEntity = null;
    public SyncSortedSet<Entity> turnQueue = new SyncSortedSet<Entity>();
    public int turn = 0;
    public static NetworkSession singleton;

    public void Awake() {
        // Debug.Log("NetworkSession:12 - Awake()");
        if (singleton != null) {
            Debug.LogWarning("Houve uma tentativa de instanciar mais de um NetworkSession");
            Destroy(this);
        }
        singleton = this;
    }

    [TargetRpc]
    public void TargetCheckForMyTurn(NetworkConnection target) {
        Debug.Log("NetworkSession:21 - TargetCheckForMyTurn(" + target + ")");
        if (curEntity == null) {
            return;
        }

        TileManager.singleton.InstantiateMarkerTile(curEntity.gridCoord, BlockData.MarkerEnum.EntityPos);
        // Calcula movimentos possíveis
        PlayerMovement playerMovement = Player.localPlayer.GetComponent<PlayerMovement>();
        playerMovement.GetAvailableMovements(curEntity.gridCoord);
        // Coloca marcador nas posições onde o personagem pode andar
        for(int i = 0; i <= playerMovement.movements.endRow - playerMovement.movements.startRow; i++)
            for(int j = 0; j <= playerMovement.movements.endCol - playerMovement.movements.startCol; j++)
                if(playerMovement.movements.visited[i,j]) 
                    TileManager.singleton.InstantiateMarkerTile(new Vector2Int(i + playerMovement.movements.startRow,j + playerMovement.movements.startCol), BlockData.MarkerEnum.CanWalkYes);
    }

    // Movimenta o personagem
    [Command(requiresAuthority = false)]
    public void CmdMove(Vector2Int goal, NetworkConnectionToClient sender = null){
        if (sender.identity.GetComponent<Entity>() != curEntity) {
            return;
        }
        // Atualiza matriz de posições
        Vector2Int gridPos = curEntity.GetComponent<Player>().gridCoord;
        NetworkMap.singleton.SetMapContent(gridPos.x, gridPos.y, NetworkMap.singleton.GetMapContent(gridPos.x, gridPos.y).with(null));
        NetworkMap.singleton.SetMapContent(goal.x, goal.y, NetworkMap.singleton.GetMapContent(goal.x, goal.y).with(curEntity));
        // Executa movimentação do jogador
        curEntity.GetComponent<PlayerMovement>().Move(goal);

        turnQueue.Remove(curEntity);
        curEntity.turn++;
        turn = curEntity.turn;
        turnQueue.Add(curEntity);

        foreach (Entity entity in turnQueue) {
            curEntity = entity;
            TargetCheckForMyTurn(curEntity.netIdentity.connectionToClient);
            break;
        }
    }
}
