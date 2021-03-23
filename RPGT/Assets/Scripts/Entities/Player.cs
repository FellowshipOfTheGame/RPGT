using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour{
    public EntityData data;
    public Vector2Int gridCoord;
    public int curHealth;
    public int curInitiative;
    public int curMoveDistance;
}

[System.Serializable]
public class EntityData{
    // Dados da entidade
    public string name;
    public int health;
    public int initiative;
    public int moveDistance;
    // Informações visuais
    public Image entitySprite;
    public Image entityIcon;
}