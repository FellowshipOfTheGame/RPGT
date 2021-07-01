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
    public TileManager tileManager;
    public GameObject skillPanel;

    public void Awake() {
        if (singleton != null) {
            Debug.LogWarning("Houve uma tentativa de instanciar mais de um NetworkSession");
            Destroy(gameObject);
        }
        singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    [TargetRpc]
    public void TargetDoTurn(NetworkConnection target) {
        Entity targetPlayer = target.identity.GetComponent<Entity>(); // Tentei usar o curEntity ao invés do targetPlayer, mas as vezes o curEntity não sincronizava a tempo

        tileManager.InstantiateTile(targetPlayer.gridCoord, TileManager.MarkerEnum.EntityPos);
        // Calcula movimentos possíveis
        PlayerMovement playerMovement = targetPlayer.GetComponent<PlayerMovement>();
        playerMovement.GetAvailableMovements(targetPlayer.gridCoord);
        // Coloca marcador nas posições onde o personagem pode andar
        for(int i = 0; i <= playerMovement.movements.endRow - playerMovement.movements.startRow; i++)
            for(int j = 0; j <= playerMovement.movements.endCol - playerMovement.movements.startCol; j++)
                if(playerMovement.movements.visited[i,j]) 
                    tileManager.InstantiateTile(new Vector2Int(i + playerMovement.movements.startRow,j + playerMovement.movements.startCol), TileManager.MarkerEnum.CanWalkYes);
    }

    // Movimenta o personagem
    [Command(requiresAuthority = false)]
    public void CmdMove(Vector2Int goal, NetworkConnectionToClient sender = null){
        if (sender.identity.GetComponent<Entity>() != curEntity) {
            return;
        }
        // Atualiza matriz de posições
        NetworkMap.singleton.MoveEntity(curEntity.gridCoord, goal);
        // Executa movimentação do jogador
        curEntity.gridCoord = goal;

        TargetSelectSkill(curEntity.netIdentity.connectionToClient);
    }

    [TargetRpc]
    public void TargetSelectSkill(NetworkConnection target) {
        skillPanel.SetActive(true);
    }

    // Movimenta o personagem
    [Command(requiresAuthority = false)]
    public void CmdUseSkill(Vector2Int playerPos, Vector2Int skillPos, Skill skill){

        MovementData movements = skill.GetSkillAttackPositions(playerPos, skillPos);
        // Coloca marcador nas posições onde o personagem pode andar
        for(int i = 0; i <= movements.endRow - movements.startRow; i++)
            for(int j = 0; j <= movements.endCol - movements.startCol; j++)
                if(movements.visited[i,j])
                    skill.Apply(NetworkMap.singleton.GetMapContent(i, j));

        NextTurn();
    }

    void NextTurn() {
        if (curEntity != null) {
            Debug.Log("Removendo " + curEntity + " da fila");
            turnQueue.Remove(curEntity);
            curEntity.turn++;
            turn = curEntity.turn;
            Debug.Log("Adicionando " + curEntity + " na fila");
            turnQueue.Add(curEntity);
        }

        foreach (Entity entity in turnQueue) {
            curEntity = entity;
            TargetDoTurn(curEntity.netIdentity.connectionToClient);
            break;
        }
    }

    bool executeOnlyOnce = true;
    void Update() {
        if (isServer && executeOnlyOnce && tileManager != null && (CustomNetworkRoomManager.singleton as CustomNetworkRoomManager).hasSceneLoadedForAllClients) {
            // TODO verificar porque isso é necessário. Do que notei, na hora que o CustomNEtworkRoomManager adiciona os players na turnQueue,
            // os players ainda não possuem um netId. Por esse motivo, é possível que a estrutura do syncedet fique zoada. Ao retirar tudo do
            // set e colocar novamente, volta a funcionar pois provavelmente a estrutura é montada corretamente
            SyncSortedSet<Entity> aux = new SyncSortedSet<Entity>();
            foreach (Entity entity in turnQueue) {
                aux.Add(entity);
            }
            turnQueue = aux;

            NextTurn();
            executeOnlyOnce = false;
        }
    }
}
