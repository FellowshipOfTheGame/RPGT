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
    [SerializeField]
    GameObject skillPanel;

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
        // Debug.Log("NetworkSession:21 - TargetCheckForMyTurn(" + target.identity.netId + ") " + target.identity.GetComponent<Entity>());
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

    [TargetRpc]
    public void TargetActions(NetworkConnection target) {
        skillPanel.SetActive(true);
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

        TargetActions(curEntity.netIdentity.connectionToClient);
    }

    // Movimenta o personagem
    [Command(requiresAuthority = false)]
    public void CmdUseSkill(Skill skill, Vector2Int pos){
        Debug.Log("Applying skills " + skill + " " + skill.GetType());
        ApplySkills(skill, pos);
        NextTurn();
    }

    void ApplySkills(Skill skill, Vector2Int curPos) {
        int availableMoves = skill.area;
        // Calcula os limites da movimentação do personagem no mapa
        int startRow = (Map.singleton.IsPositionInMap(curPos.x - availableMoves, curPos.y)) ? curPos.x - availableMoves : 0;
        int endRow = (Map.singleton.IsPositionInMap(curPos.x + availableMoves, curPos.y)) ? curPos.x + availableMoves : Map.singleton.mapRows-1;
        int startCol = (Map.singleton.IsPositionInMap(curPos.x, curPos.y - availableMoves)) ? curPos.y - availableMoves : 0;
        int endCol = (Map.singleton.IsPositionInMap(curPos.x, curPos.y + availableMoves)) ? curPos.y + availableMoves : Map.singleton.mapCols-1;
        // Dados para o BFS
        Queue<(Vector2Int, int)> nodesToVisit = new Queue<(Vector2Int, int)>();
        // Controle dos blocos disponíveis para caminhar
        bool[,] visited = new bool[(endRow - startRow) + 1, (endCol - startCol) + 1];

        // Insere a posição inicial nas listas
        nodesToVisit.Enqueue((curPos, 0));
        visited[curPos.x - startRow, curPos.y - startCol] = true;
        Debug.Log("Applying on coord " + curPos + ":");
        Debug.Log("Antes: " + NetworkMap.singleton.GetMapContent(curPos.x, curPos.y) + ":");
        skill.Apply(NetworkMap.singleton.GetMapContent(curPos.x, curPos.y));
        Debug.Log("Depois: " + NetworkMap.singleton.GetMapContent(curPos.x, curPos.y) + ":");

        // Executa BFS
        while(nodesToVisit.Count > 0){
            (Vector2Int, int) toProcess = nodesToVisit.Dequeue();


            foreach(Vector2Int move in VoxelData.movements) {
                Vector2Int neighbor = new Vector2Int(toProcess.Item1.x + move.x, toProcess.Item1.y + move.y);
                if (Map.singleton.IsPositionInMap(neighbor.x, neighbor.y) && !visited[neighbor.x - startRow, neighbor.y - startCol] && toProcess.Item2 + 1 < availableMoves) {
                    visited[neighbor.x - startRow, neighbor.y - startCol] = true;
                    Debug.Log("Applying on coord " + neighbor + ":");
                    Debug.Log("Antes: " + NetworkMap.singleton.GetMapContent(neighbor.x, neighbor.y) + ":");
                    skill.Apply(NetworkMap.singleton.GetMapContent(neighbor.x, neighbor.y));
                    Debug.Log("Depois: " + NetworkMap.singleton.GetMapContent(neighbor.x, neighbor.y) + ":");
                    nodesToVisit.Enqueue((neighbor, toProcess.Item2 + 1));
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
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
