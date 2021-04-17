using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMap : NetworkBehaviour
{
    public static NetworkMap singleton;
    public int mapRows;
    public int mapCols;
    public SyncList<BlockContent> mapContent = new SyncList<BlockContent>();
    public void Awake() {
        // Debug.Log("NetworkMap:12 - Start()");
        if (singleton != null) {
            Debug.LogWarning("Houve uma tentativa de instanciar mais de um NetworkMap");
            Destroy(this);
        }
        singleton = this;
    }

    public BlockContent GetMapContent(int i, int j) {
        // Debug.Log("NetworkMap:23 - GetMapContent(" + i + ", " + j + ")");
        return mapContent[(i * mapRows) + j];
    }

    [Command(requiresAuthority = false)]
    public void SetMapContent(int i, int j, BlockContent value) {
        // Debug.Log("NetworkMap:28 - SetMapContent(" + i + ", " + j + ", " + value + ")");
        mapContent[(i * mapRows) + j] = value;
    }

    [Command(requiresAuthority = false)]
    public void MoveEntity(Vector2Int from, Vector2Int to) {
        Entity entity = GetMapContent(from.x, from.y).entity;

        if (entity == null) {
            throw new System.Exception("Não há nenhuma entity em " + from);
        }

        SetMapContent(from.x, from.y, NetworkMap.singleton.GetMapContent(from.x, from.y).with(null));
        SetMapContent(to.x, to.y, NetworkMap.singleton.GetMapContent(to.x, to.y).with(entity));
    }

    public Vector2Int GetEmptyPosition() {
        for (int i = 0; i < mapRows; i++)
            for (int j = 0; j < mapCols; j++) 
                if (GetMapContent(i, j).canWalk())
                    return new Vector2Int(i, j);
        throw new System.Exception("Não há espaço vazio no mapa");
    }
}
