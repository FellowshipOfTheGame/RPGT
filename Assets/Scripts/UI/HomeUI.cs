using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    public GameObject home;
    public GameObject mapSelection;
    public NetworkRoomManager networkManager;

    public void SetAdress(Text input)
    {
        networkManager.networkAddress = input.text;
    }

    void DeativateAllPanels() {
        home.SetActive(false);
        mapSelection.SetActive(false);
    }

    public void ChangeToMapSelection() {
        home.SetActive(false);

        mapSelection.SetActive(true);
    }
}
