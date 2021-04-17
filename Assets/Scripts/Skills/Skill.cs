using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Skill
{
    public string name;
    public string description;
    public int range;
    public int area;
    public int modifier;
    public int cost;

    public virtual void Apply(BlockContent block) {
        Debug.LogError("Acessando Apply da classe base de Skill")
        throw new MethodAccessException();
    }
}

public class BasicAttack : Skill
{
    public BasicAttack() {
        name = "Basic Attack";
        description = "A basic attack on a tile";
        range = 1;
        area = 1;
        modifier = 1;
        cost = 1;
    }

    public override void Apply(BlockContent block) {
        if (block.entity)
            block.entity.current.health -= modifier;
    }
}

public class BasicHeal : Skill
{
    public BasicHeal() {
        name = "Basic Heal";
        description = "A basic heal on a tile";
        range = 1;
        area = 1;
        modifier = 1;
        cost = 1;
    }

    public override void Apply(BlockContent block) {
        if (block.entity)
            block.entity.current.health += modifier;
    }
}

public class Fireball : Skill
{
    Effect effect;
    public Fireball() {
        name = "Basic Fireball";
        description = "A basic Fireball on a tile";
        range = 4;
        area = 2;
        modifier = 2;
        cost = 1;
        effect = new Fire();
        effect.modifier = modifier;
    }

    public override void Apply(BlockContent block) {
        if (block.entity) {
            block.entity.current.health -= modifier;
            Debug.Log("Usado fireball no player " + block.entity);
        }
        block.effect = effect;
    }
}