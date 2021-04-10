using Mirror;
using System.Collections.Generic;
using UnityEngine;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class CustomNetworkRoomManager : NetworkManager
{
	public NetworkMap networkMap;						// NetworkMap Singleton isn't ready fast enough for OnStartServer, so we need to link it directly
        
	/// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer() {
        // Instancia o mapa geral que é sincronizado entre todos os players
		List<BlockContent> tmpMapContent = new List<BlockContent>();
        for (int i = 0; i < networkMap.mapRows * networkMap.mapCols; i++)
            tmpMapContent.Add(new BlockContent(null, (int) BlockData.BlockEnum.Ground));

        networkMap.mapContent.AddRange(tmpMapContent);
	}

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("CustomNetworkRoomManager:31 - OnServerAddPlayer(" + conn + ")");

        // Spawns the player, fills its position on the mapContent and translates him to such position
        Entity player = conn.identity.GetComponent<Entity>();
        Vector2Int spawnPos = networkMap.GetEmptyPosition();
        networkMap.SetMapContent(spawnPos.x, spawnPos.y, networkMap.GetMapContent(spawnPos.x, spawnPos.y).with(player));

        player.SetGridCoord(spawnPos);

        if (NetworkSession.singleton.curEntity == null) {
            Debug.Log("Adicionando " + conn + " na turnQueue");
            NetworkSession.singleton.turnQueue.Add(player);
            Debug.Log("Adicionando " + conn + " como curEntity");
            NetworkSession.singleton.curEntity = player;
        }
        else {
            Debug.Log("Adicionando " + conn + " na turnQueue");
            NetworkSession.singleton.turnQueue.Add(player);
        }
    }
}
