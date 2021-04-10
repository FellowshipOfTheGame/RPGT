using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSession : NetworkBehaviour
{
    [SyncVar(hook = nameof(CheckForMyTurn))]
    public Entity curEntity = null;
    public SyncSortedSet<Entity> turnQueue = new SyncSortedSet<Entity>();
    public static NetworkSession singleton;

    public void Awake() {
        // Debug.Log("NetworkSession:12 - Awake()");
        if (singleton != null) {
            Debug.LogWarning("Houve uma tentativa de instanciar mais de um NetworkSession");
            Destroy(this);
        }
        singleton = this;
    }

    public void CheckForMyTurn(Entity oldEntity, Entity newEntity) {
        // Debug.Log("NetworkSession:21 - CheckForMyTurn(" + oldEntity + ", " + newEntity + ")");
        if (newEntity == null || !newEntity.Equals(Player.localPlayer)) {
            return;
        }

        TileManager.singleton.InstantiateMarkerTile(newEntity.gridCoord, BlockData.MarkerEnum.EntityPos);
        // Calcula movimentos possíveis
        PlayerMovement playerMovement = Player.localPlayer.GetComponent<PlayerMovement>();
        playerMovement.GetAvailableMovements(newEntity.gridCoord);
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

        Debug.Log("Removing " + curEntity + ". Tamanho da lista: " + turnQueue.Count);
        turnQueue.Remove(curEntity);
        Debug.Log("Removed " + curEntity + ". Tamanho da lista: " + turnQueue.Count);
        curEntity.turn++;
        Debug.Log("Adding " + curEntity + ". Tamanho da lista: " + turnQueue.Count);
        turnQueue.Add(curEntity);
        Debug.Log("Added " + curEntity + ". Tamanho da lista: " + turnQueue.Count);

        foreach (Entity entity in turnQueue) {
            Debug.Log("Updating " + curEntity);
            curEntity = entity;
            Debug.Log("Updated " + curEntity);
            break;
        }
    }
}
