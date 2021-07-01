using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    CustomNetworkRoomManager networkRoomManager;
    NetworkMap networkMap;

    public void Start() {
        networkRoomManager = NetworkManager.singleton as CustomNetworkRoomManager;
        networkMap = NetworkMap.singleton;

        if (networkRoomManager == null)
            Debug.LogWarning("networkRoomManager null");

        if (networkMap == null)
            Debug.LogWarning("networkMap null");
    }

    public void Host() {
        networkRoomManager.StartHost();
    }

    public void Join() {
        networkRoomManager.StartClient();
    }

    public void SetAdress(Text input)
    {
        networkRoomManager.networkAddress = input.text;
    }

    public void ReadFile() {
        networkMap.ReadFile();
    }
}
