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
    
    public override void OnStartClient() {
        // Sets the player transform
        transform.SetParent(Session.singleton.playersTransform);
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