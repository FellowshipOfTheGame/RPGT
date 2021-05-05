using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Effect {
    public string name;
    public string description;
    public int modifier;
    public int duration;

    public virtual void Apply(BlockContent block) {}

    public override string ToString() {
        return JsonUtility.ToJson(this);
    }
}

public class Fire : Effect {
    public Fire() {
        name = "Fire";
        description = "Fire";
        modifier = 1;
        duration = 5;
    }
    public override void Apply(BlockContent block) {
        throw new NotImplementedException();
    }
}

