using Mirror;
using System.Collections.Generic;
using UnityEngine;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class CustomNetworkRoomManager : NetworkRoomManager
{
	public NetworkMap networkMap;
	public NetworkSession networkSession;

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <param name="roomPlayer">The room player object for this connection.</param>
    /// <returns>A new GamePlayer object.</returns>
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        // Spawns the player, fills its position on the mapContent and translates him to such position
        GameObject gameObjPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        Entity player = gameObjPlayer.GetComponent<Entity>();
        Vector2Int spawnPos = networkMap.GetEmptyPosition();
        networkMap.mapContent[(spawnPos.x * networkMap.mapRows) + spawnPos.y] = networkMap.GetMapContent(spawnPos.x, spawnPos.y).with(player);

        player.gridCoord = spawnPos;
        player.turn = networkSession.turn;

        networkSession.turnQueue.Add(player);

        return gameObjPlayer;
    }
}
