using System.Collections.Generic;
using Mirror;

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
}
