using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity), typeof(NetworkTransform))]
public class Entity : NetworkBehaviour
{
    // Dados da entidade
    public BasicStats initial;
    public BasicStats current;
    public Vector2Int gridCoord;

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() {
        Debug.Log("Entity:13 - OnStartClient()");
        // Sets the player transform
        transform.SetParent(GameObject.FindObjectOfType<Session>().playersTransform);
    }

    // Informações visuais - Ainda indisponíveis devido a falta de um serializer
    // public Image entitySprite;
    // public Image entityIcon;
}

[System.Serializable]
public class BasicStats{
    public int health = 10;
    public int initiative = 10;
    public int moveDistance = 3;
}