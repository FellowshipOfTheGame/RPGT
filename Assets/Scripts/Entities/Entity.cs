using Mirror;
using System;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity), typeof(NetworkTransform))]
public class Entity : NetworkBehaviour, IComparable
{
    // Dados da entidade
    public BasicStats initial;
    public BasicStats current;
    
    [HideInInspector]
    public Vector2Int gridCoord { 
        get { return new Vector2Int((int) transform.position.x, (int) transform.position.z); }
        set { transform.position = new Vector3(value.x + 0.5f, 1.5f, value.y + 0.5f); }
    }

    [HideInInspector]
    public int turn = 0;

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() {
        // Debug.Log("Entity:13 - OnStartClient()");
        // Sets the player transform
        transform.SetParent(GameObject.FindObjectOfType<Session>().playersTransform);
    }

    [TargetRpc]
    public void TargetClearMarkerAndPathInstances(NetworkConnection target) {
        // Debug.Log(name + " " + netId);
        TileManager.singleton.ClearInstances();
        TileManager.singleton.ClearPathInstances();
    }

    // Informações visuais - Ainda indisponíveis devido a falta de um serializer
    // public Image entitySprite;
    // public Image entityIcon;
    public int CompareTo(object obj)
    {
        if (obj is Entity)
        {
            Entity entity = obj as Entity;
            if (turn != entity.turn)
                return turn - entity.turn;
            if (entity.current.initiative != current.initiative)
                return current.initiative - entity.current.initiative;
            return (int) netId - (int) entity.netId;
        }
        throw new System.Exception("Compara��o de Entity com " + obj.GetType() + "n�o � v�lida");
    }

    public override string ToString()
    {
        return $"{{netId: {netId}, turn: {turn}, gridCoord: {gridCoord}}}";
    }
}

[System.Serializable]
public class BasicStats{
    public int health = 10;
    public int initiative = 10;
    public int moveDistance = 3;
}