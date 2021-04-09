using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMap : NetworkBehaviour
{
    public static NetworkMap singleton;
    public int mapRows;
    public int mapCols;
    public SyncList<BlockContent> mapContent = new SyncList<BlockContent>();

    public void Start() {
        Debug.Log("NetworkMap:12 - Start()");
        if (singleton != null) {
            Debug.LogWarning("Houve uma tentativa de instanciar mais de um NetworkMap");
            Destroy(this);
        }
        singleton = this;

        Debug.Log(mapContent);
    }

    public BlockContent MapContentAt(int i, int j) {
        // Debug.Log("NetworkMap:23 - MapContentAt(" + i + ", " + j + ")");
        return mapContent[(i * mapRows) + j];
    }
}
