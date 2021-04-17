using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSession : NetworkBehaviour
{
    public SyncSortedSet<Entity> turnQueue = new SyncSortedSet<Entity>();
    [SyncVar]
    public Entity curEntity = null;
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
        Debug.Log("NetworkSession:21 - TargetCheckForMyTurn(" + target.identity.netId + ") " + target.identity.GetComponent<Entity>());
        Entity targetPlayer = target.identity.GetComponent<Entity>(); // Tentei usar o curEntity ao invés do targetPlayer, mas as vezes o curEntity não sincronizava a tempo

        TileManager.singleton.InstantiateMarkerTile(targetPlayer.gridCoord, BlockData.MarkerEnum.EntityPos);
        // Calcula movimentos possíveis
        PlayerMovement playerMovement = targetPlayer.GetComponent<PlayerMovement>();
        playerMovement.GetAvailableMovements(targetPlayer.gridCoord);
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
        NetworkMap.singleton.MoveEntity(curEntity.GetComponent<Player>().gridCoord, goal);
        // Executa movimentação do jogador
        curEntity.gridCoord = goal;
        curEntity.TargetClearMarkerAndPathInstances(curEntity.netIdentity.connectionToClient);

        NextTurn();
    }

    void NextTurn() {
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
