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
            writer.WriteInt32(basicAttack.damage);
        }
        else if (skill is BasicHeal basicHeal)
        {
            writer.WriteByte(BASICHEAL);
            writer.WriteInt32(basicHeal.heal);
        }
        else if (skill is Fireball fireball)
        {
            writer.WriteByte(BASICFIREBALL);
            writer.Write<Effect>(fireball.effect);
            writer.WriteInt32(fireball.fireDamage);
            writer.WriteInt32(fireball.damage);
        }
        writer.WriteString(skill.name);
        writer.WriteString(skill.description);
    }

    public static Skill ReadItem(this NetworkReader reader)
    {
        byte type = reader.ReadByte();
        switch(type)
        {
            case BASICATTACK:
                return new BasicAttack
                {
                    damage = reader.ReadInt32(),
                    name = reader.ReadString(),
                    description = reader.ReadString()
                };
            case BASICHEAL:
                return new BasicHeal
                {
                    heal = reader.ReadInt32(),
                    name = reader.ReadString(),
                    description = reader.ReadString()
                };
            case BASICFIREBALL:
                return new Fireball
                {
                    effect = reader.Read<Effect>(),
                    fireDamage = reader.ReadInt32(),
                    damage = reader.ReadInt32(),
                    name = reader.ReadString(),
                    description = reader.ReadString()
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

    public virtual void Apply(BlockContent block) {
        Debug.LogError("Acessando Apply da classe base de Skill");
        throw new MethodAccessException();
    }

    public virtual MovementData GetSkillAttackRange(Vector2Int playerPos) {
        return GetSkillAttackRangeWithRange(playerPos, 1);
    }

    public virtual MovementData GetSkillAttackPositions(Vector2Int playerPos, Vector2Int attackPoint) {
        return GetSkillAttackPositionsWithArea(playerPos, attackPoint, 1);
    }

    MovementData GetSkillAttackRangeWithRange(Vector2Int playerPos, int range) {
        int availableMoves = range + 1;
        
        // Calcula os limites da movimentação do personagem no mapa
        int startRow = (Map.singleton.IsPositionInMap(playerPos.x - availableMoves, playerPos.y)) ? playerPos.x - availableMoves : 0;
        int endRow = (Map.singleton.IsPositionInMap(playerPos.x + availableMoves, playerPos.y)) ? playerPos.x + availableMoves : Map.singleton.mapRows-1;
        int startCol = (Map.singleton.IsPositionInMap(playerPos.x, playerPos.y - availableMoves)) ? playerPos.y - availableMoves : 0;
        int endCol = (Map.singleton.IsPositionInMap(playerPos.x, playerPos.y + availableMoves)) ? playerPos.y + availableMoves : Map.singleton.mapCols-1;
        
        // Dados para o BFS
        Queue<(Vector2Int, int)> nodesToVisit = new Queue<(Vector2Int, int)>();
        
        // Controle dos blocos disponíveis para caminhar
        bool[,] visited = new bool[(endRow - startRow) + 1, (endCol - startCol) + 1];

        // Insere a posição inicial nas listas
        nodesToVisit.Enqueue((playerPos, 0));
        visited[playerPos.x - startRow, playerPos.y - startCol] = true;

        // Executa BFS
        while(nodesToVisit.Count > 0){
            (Vector2Int, int) toProcess = nodesToVisit.Dequeue();


            foreach(Vector2Int move in VoxelData.movements) {
                Vector2Int neighbor = new Vector2Int(toProcess.Item1.x + move.x, toProcess.Item1.y + move.y);
                if (Map.singleton.IsPositionInMap(neighbor.x, neighbor.y) && !visited[neighbor.x - startRow, neighbor.y - startCol] && toProcess.Item2 + 1 < availableMoves) {
                    visited[neighbor.x - startRow, neighbor.y - startCol] = true;
                    nodesToVisit.Enqueue((neighbor, toProcess.Item2 + 1));
                }
            }
        }

        return new MovementData(null, visited, startRow, endRow, startCol, endCol);
    }

    MovementData GetSkillAttackPositionsWithArea(Vector2Int playerPos, Vector2Int attackPoint, int area) {
        int availableMoves = area;
        
        // Calcula os limites da movimentação do personagem no mapa
        int startRow = (Map.singleton.IsPositionInMap(attackPoint.x - availableMoves, attackPoint.y)) ? attackPoint.x - availableMoves : 0;
        int endRow = (Map.singleton.IsPositionInMap(attackPoint.x + availableMoves, attackPoint.y)) ? attackPoint.x + availableMoves : Map.singleton.mapRows-1;
        int startCol = (Map.singleton.IsPositionInMap(attackPoint.x, attackPoint.y - availableMoves)) ? attackPoint.y - availableMoves : 0;
        int endCol = (Map.singleton.IsPositionInMap(attackPoint.x, attackPoint.y + availableMoves)) ? attackPoint.y + availableMoves : Map.singleton.mapCols-1;
        
        // Dados para o BFS
        Queue<(Vector2Int, int)> nodesToVisit = new Queue<(Vector2Int, int)>();
        
        // Controle dos blocos disponíveis para caminhar
        bool[,] visited = new bool[(endRow - startRow) + 1, (endCol - startCol) + 1];

        // Insere a posição inicial nas listas
        nodesToVisit.Enqueue((attackPoint, 0));
        visited[attackPoint.x - startRow, attackPoint.y - startCol] = true;

        // Executa BFS
        while(nodesToVisit.Count > 0){
            (Vector2Int, int) toProcess = nodesToVisit.Dequeue();


            foreach(Vector2Int move in VoxelData.movements) {
                Vector2Int neighbor = new Vector2Int(toProcess.Item1.x + move.x, toProcess.Item1.y + move.y);
                if (Map.singleton.IsPositionInMap(neighbor.x, neighbor.y) && !visited[neighbor.x - startRow, neighbor.y - startCol] && toProcess.Item2 + 1 < availableMoves) {
                    visited[neighbor.x - startRow, neighbor.y - startCol] = true;
                    nodesToVisit.Enqueue((neighbor, toProcess.Item2 + 1));
                }
            }
        }

        return new MovementData(null, visited, startRow, endRow, startCol, endCol);
    }
}

public class BasicAttack : Skill
{
    public int damage;
    public BasicAttack() {
        name = "Basic Attack";
        description = "A basic attack on a tile";
        damage = 1;
    }

    public override void Apply(BlockContent block) {
        if (block.entity)
            block.entity.current.health -= damage;
    }
}

public class BasicHeal : Skill
{
    public int heal;
    public BasicHeal() {
        name = "Basic Heal";
        description = "A basic heal on a tile";
        heal = 1;
    }

    public override void Apply(BlockContent block) {
        if (block.entity)
            block.entity.current.health += heal;
    }
}

public class Fireball : Skill
{
    public Effect effect;
    public int fireDamage;
    public int damage;
    public Fireball() {
        name = "Basic Fireball";
        description = "A basic Fireball on a tile";
        damage = 1;
        effect = new Fire();
        effect.modifier = fireDamage;
    }

    public override void Apply(BlockContent block) {
        if (block.entity) {
            block.entity.current.health -= damage;
        }
        block.effect = effect;
    }

    public override MovementData GetSkillAttackRange(Vector2Int playerPos) {
        // Calcula os limites da movimentação do personagem no mapa
        int startRow = (Map.singleton.IsPositionInMap(playerPos.x - 2, playerPos.y)) ? playerPos.x - 2 : 0;
        int endRow = (Map.singleton.IsPositionInMap(playerPos.x + 2, playerPos.y)) ? playerPos.x + 2 : Map.singleton.mapRows-1;
        int startCol = (Map.singleton.IsPositionInMap(playerPos.x, playerPos.y - 2)) ? playerPos.y - 2 : 0;
        int endCol = (Map.singleton.IsPositionInMap(playerPos.x, playerPos.y + 2)) ? playerPos.y + 2 : Map.singleton.mapCols-1;
        
        bool[,] visited = new bool[(endRow - startRow) + 1, (endCol - startCol) + 1];
        
        if (Map.singleton.IsPositionInMap(playerPos.x - 2 - startRow, playerPos.y - startCol))
            visited[playerPos.x - 2 - startRow, playerPos.y - startCol] = true;
        if (Map.singleton.IsPositionInMap(playerPos.x + 2 - startRow, playerPos.y - startCol))
            visited[playerPos.x + 2 - startRow, playerPos.y - startCol] = true;
        if (Map.singleton.IsPositionInMap(playerPos.x - startRow, playerPos.y - 2 - startCol))
            visited[playerPos.x - startRow, playerPos.y - 2 - startCol] = true;
        if (Map.singleton.IsPositionInMap(playerPos.x - startRow, playerPos.y + 2 - startCol))
            visited[playerPos.x - startRow, playerPos.y + 2 - startCol] = true;

        return new MovementData(null, visited, startRow, endRow, startCol, endCol);
    }

    public override MovementData GetSkillAttackPositions(Vector2Int playerPos, Vector2Int attackPoint) {
        // Calcula os limites da movimentação do personagem no mapa
        int startRow = (Map.singleton.IsPositionInMap(playerPos.x - 4, playerPos.y)) ? playerPos.x - 4 : 0;
        int endRow = (Map.singleton.IsPositionInMap(playerPos.x + 4, playerPos.y)) ? playerPos.x + 4 : Map.singleton.mapRows-1;
        int startCol = (Map.singleton.IsPositionInMap(playerPos.x, playerPos.y - 4)) ? playerPos.y - 4 : 0;
        int endCol = (Map.singleton.IsPositionInMap(playerPos.x, playerPos.y + 4)) ? playerPos.y + 4 : Map.singleton.mapCols-1;
        
        bool[,] visited = new bool[(endRow - startRow) + 1, (endCol - startCol) + 1];
        
        if (playerPos.x == attackPoint.x) {
            for (int i = -2; i < 3; i++) {
                Debug.Log("x == x " + (attackPoint.x + i - startRow) + ", " + (attackPoint.y - startCol));
                if (Map.singleton.IsPositionInMap(attackPoint.x + i - startRow, attackPoint.y - startCol))
                    visited[attackPoint.x + i - startRow, attackPoint.y - startCol] = true;
            }
        }
        else if (playerPos.y == attackPoint.y) {
            for (int i = -2; i < 3; i++) {
                Debug.Log("yy == yy " + (attackPoint.x - startRow) + ", " + (attackPoint.y + i - startCol));
                if (Map.singleton.IsPositionInMap(attackPoint.x - startRow, attackPoint.y + i - startCol))
                    visited[attackPoint.x - startRow, attackPoint.y + i - startCol] = true;
            }
        }

        return new MovementData(null, visited, startRow, endRow, startCol, endCol);
    }
}