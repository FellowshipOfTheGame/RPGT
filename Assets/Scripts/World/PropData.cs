using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropData : MonoBehaviour{
    public enum PropEnum {Mushroom, Rock, Bush, None};
    public List<Prop> propList = new List<Prop>();
    public static PropData singleton;

    void Awake() {
        if(singleton != null) {
            Debug.LogWarning("Houve uma tentativa de criar 2 PropData");
            Destroy(this);
        }
        singleton = this;
    }
}

[System.Serializable]
public class Prop{
    public string name;
    public bool canWalk;
    public Vector2Int size;
    public GameObject model;
}