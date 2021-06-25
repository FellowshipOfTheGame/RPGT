using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomPlayerRoom : NetworkRoomPlayer
{
    public List<Skill> skills = new List<Skill> { new BasicAttack(), new BasicHeal(), new Fireball() };
    public static CustomPlayerRoom localPlayerRoom = null;

    public Text nameText;
    public Button readyButton;
    public Text readyText;

    void Awake() {
        Transform ui = GameObject.Find("UIPanel").transform.Find("Players").transform.Find("ScrollRect").transform.Find("Content (Players)");

        transform.SetParent(ui);
    }

    /// <summary>Like Start(), but only called on client and host.</summary>
    public override void OnStartClient() {
        if (isLocalPlayer) {
            readyButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() {
        if (localPlayerRoom != null) {
            Debug.LogWarning("Houve uma tentativa de setar dois localPlayerRooms");
            Destroy(this);
        }
        localPlayerRoom = this;
    }

    public void ChangeReadyStatus() {
        CmdChangeReadyState(!readyToBegin);
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) {
        if (newReadyState) {
            readyText.text = "Ready";
            readyText.color = new Color(0, 200, 0);
        }
        else {
            readyText.text = "Not Ready";
            readyText.color = new Color(200, 0, 0);
        }
    }


}
