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
        MovementData movements = GetRangeAttackMovements(playerPos);
        // Coloca marcador nas posições onde o personagem pode andar
        for(int i = 0; i <= movements.endRow - movements.startRow; i++)
            for(int j = 0; j <= movements.endCol - movements.startCol; j++)
                if(movements.visited[i,j] && !(playerPos.x == movements.startRow + i && playerPos.y == movements.startCol + j)) 
                    TileManager.singleton.InstantiateTile(new Vector2Int(i + movements.startRow,j + movements.startCol), TileManager.MarkerEnum.AttackRange);
    }

    MovementData GetRangeAttackMovements(Vector2Int curPos, bool rangeOrArea = true) {
        int availableMoves = 0;
        if (rangeOrArea) {
            availableMoves = curSkill.range + 1;
        }
        else {
            availableMoves = curSkill.area;
        }
        // Calcula os limites da movimentação do personagem no mapa
        int startRow = (Map.singleton.IsPositionInMap(curPos.x - availableMoves, curPos.y)) ? curPos.x - availableMoves : 0;
        int endRow = (Map.singleton.IsPositionInMap(curPos.x + availableMoves, curPos.y)) ? curPos.x + availableMoves : Map.singleton.mapRows-1;
        int startCol = (Map.singleton.IsPositionInMap(curPos.x, curPos.y - availableMoves)) ? curPos.y - availableMoves : 0;
        int endCol = (Map.singleton.IsPositionInMap(curPos.x, curPos.y + availableMoves)) ? curPos.y + availableMoves : Map.singleton.mapCols-1;
        // Dados para o BFS
        Queue<(Vector2Int, int)> nodesToVisit = new Queue<(Vector2Int, int)>();
        // Controle dos blocos disponíveis para caminhar
        bool[,] visited = new bool[(endRow - startRow) + 1, (endCol - startCol) + 1];

        // Insere a posição inicial nas listas
        nodesToVisit.Enqueue((curPos, 0));
        visited[curPos.x - startRow, curPos.y - startCol] = true;

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

    public void DrawAttackTiles(Vector2Int attackPos) {
        // Instanciar os tiles de attackRange
        MovementData movements = GetRangeAttackMovements(attackPos, false);
        // Coloca marcador nas posições onde o personagem pode andar
        for(int i = 0; i <= movements.endRow - movements.startRow; i++)
            for(int j = 0; j <= movements.endCol - movements.startCol; j++)
                if(movements.visited[i,j])
                    TileManager.singleton.InstantiateTile(new Vector2Int(i + movements.startRow,j + movements.startCol), TileManager.MarkerEnum.Attack);
    }
}