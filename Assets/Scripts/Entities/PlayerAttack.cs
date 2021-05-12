using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public List<Skill> skills = new List<Skill> { new BasicAttack(), new BasicHeal(), new Fireball() };
    public Skill curSkill;

    public void UseSkill(Skill skill) {
        curSkill = skill;
        Vector2Int playerPos = GetComponent<Entity>().gridCoord;
        // Instanciar os tiles de attackRange
        MovementData movements = skill.GetSkillAttackRange(playerPos);
        // Coloca marcador nas posições onde o personagem pode andar
        for(int i = 0; i <= movements.endRow - movements.startRow; i++)
            for(int j = 0; j <= movements.endCol - movements.startCol; j++)
                if(movements.visited[i,j] && !(playerPos.x == movements.startRow + i && playerPos.y == movements.startCol + j)) 
                    TileManager.singleton.InstantiateTile(new Vector2Int(i + movements.startRow,j + movements.startCol), TileManager.MarkerEnum.AttackRange);
    }

    public void DrawAttackTiles(Vector2Int attackPos) {
        // Instanciar os tiles de attackRange
        MovementData movements = curSkill.GetSkillAttackPositions(GetComponent<Entity>().gridCoord, attackPos);
        // Coloca marcador nas posições onde o personagem pode andar
        for(int i = 0; i <= movements.endRow - movements.startRow; i++)
            for(int j = 0; j <= movements.endCol - movements.startCol; j++)
                if(movements.visited[i,j])
                    TileManager.singleton.InstantiateTile(new Vector2Int(i + movements.startRow,j + movements.startCol), TileManager.MarkerEnum.Attack);
    }
}