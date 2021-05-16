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
    private int sceneLoadedForClients = 0;
    public bool hasSceneLoadedForAllClients { get { return roomSlots.Count != 0 ? sceneLoadedForClients == roomSlots.Count : false; } }

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

        Debug.Log("Adicionando " + player + " na fila");
        networkSession.turnQueue.Add(player);

        return gameObjPlayer;
    }

    /// <summary>
    /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
    /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
    /// </summary>
    /// <param name="conn">The connection of the player</param>
    /// <param name="roomPlayer">The room player object.</param>
    /// <param name="gamePlayer">The game player object.</param>
    /// <returns>False to not allow this player to replace the room player.</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        sceneLoadedForClients++;
        
        return true;
    }

    public override void OnGUI()
    {
        if (!showRoomGUI)
            return;

        if (NetworkServer.active && IsSceneActive(GameplayScene))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
            if (GUILayout.Button("Return to Room"))
                ServerChangeScene(RoomScene);
            GUILayout.EndArea();
        }

        if (IsSceneActive(RoomScene)) {
            GUI.Box(new Rect(Screen.width / 2 - 400f, 450f, 520f, 400), "PLAYERS");
        }
    }
}
