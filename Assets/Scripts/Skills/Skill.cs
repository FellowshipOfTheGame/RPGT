using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class SkillSerializer 
{
    const byte BASICATTACK = 0;
    const byte BASICHEAL = 1;
    const byte BASICFIREBALL = 2;

    public static void WriteItem(this NetworkWriter writer, Skill skill)
    {
        if (skill is BasicAttack basicAttack)
        {
            writer.WriteByte(BASICATTACK);
        }
        else if (skill is BasicHeal basicHeal)
        {
            writer.WriteByte(BASICHEAL);
        }
        else if (skill is Fireball fireBall)
        {
            writer.WriteByte(BASICFIREBALL);
            writer.Write<Effect>((skill as Fireball).effect);
        }
        writer.WriteString(skill.name);
        writer.WriteString(skill.description);
        writer.WriteInt32(skill.range);
        writer.WriteInt32(skill.area);
        writer.WriteInt32(skill.modifier);
        writer.WriteInt32(skill.cost);
    }

    public static Skill ReadItem(this NetworkReader reader)
    {
        byte type = reader.ReadByte();
        switch(type)
        {
            case BASICATTACK:
                return new BasicAttack
                {
                    name = reader.ReadString(),
                    description = reader.ReadString(),
                    range = reader.ReadInt32(),
                    area = reader.ReadInt32(),
                    modifier = reader.ReadInt32(),
                    cost = reader.ReadInt32()
                };
            case BASICHEAL:
                return new BasicHeal
                {
                    name = reader.ReadString(),
                    description = reader.ReadString(),
                    range = reader.ReadInt32(),
                    area = reader.ReadInt32(),
                    modifier = reader.ReadInt32(),
                    cost = reader.ReadInt32()
                };
            case BASICFIREBALL:
                return new Fireball
                {
                    effect = reader.Read<Effect>(),
                    name = reader.ReadString(),
                    description = reader.ReadString(),
                    range = reader.ReadInt32(),
                    area = reader.ReadInt32(),
                    modifier = reader.ReadInt32(),
                    cost = reader.ReadInt32()
                };
            default:
                throw new Exception($"Invalid weapon type {type}");
        }
    }
}

public class Skill
{
    public string name;
    public string description;
    public int range;
    public int area;
    public int modifier;
    public int cost;

    public virtual void Apply(BlockContent block) {
        Debug.LogError("Acessando Apply da classe base de Skill");
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
    public Effect effect;
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
        }
        block.effect = effect;
    }
}