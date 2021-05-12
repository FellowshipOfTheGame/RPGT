using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class CustomPlayerRoom : NetworkRoomPlayer
{
    public List<Skill> skills = new List<Skill> { new BasicAttack(), new BasicHeal(), new Fireball() };
    public static CustomPlayerRoom localPlayerRoom = null;

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
}
