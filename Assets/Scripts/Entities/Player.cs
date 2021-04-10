using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity {
    public static Player localPlayer = null;

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() {
        Debug.Log("Player:12 - OnStartLocalPlayer()");
        if (localPlayer != null) {
            Debug.LogWarning("Houve uma tentativa de setar dois localplayers");
            Destroy(this);
        }
        localPlayer = this;

        // Manually check for my turn
        NetworkSession.singleton.CheckForMyTurn(null, NetworkSession.singleton.curEntity);
    }
}
