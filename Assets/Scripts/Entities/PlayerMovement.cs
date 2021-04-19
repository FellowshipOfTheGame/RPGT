using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour{
    Stack<Vector2Int> path = new Stack<Vector2Int>();
    [HideInInspector]
    public MovementData movements;

    // Calcula o número de movimentos que a entidade pode realizar
    public int GetMovementDistance(){
        return this.GetComponent<Player>().current.moveDistance;
    }

    // Calcula e exibe movimentações possíveis usando A*
    public void GetAvailableMovements(Vector2Int pos){
        // Debug.Log("PlayerMovement:32 - GetAvailableMovements(" + pos + ")");
        int availableMoves = GetMovementDistance();
        // Calcula os limites da movimentação do personagem no mapa
        int startRow = (Map.singleton.IsPositionInMap(pos.x - availableMoves, pos.y)) ? pos.x - availableMoves : 0;
        int endRow = (Map.singleton.IsPositionInMap(pos.x + availableMoves, pos.y)) ? pos.x + availableMoves : Map.singleton.mapRows-1;
        int startCol = (Map.singleton.IsPositionInMap(pos.x, pos.y - availableMoves)) ? pos.y - availableMoves : 0;
        int endCol = (Map.singleton.IsPositionInMap(pos.x, pos.y + availableMoves)) ? pos.y + availableMoves : Map.singleton.mapCols-1;
        // Dados para o A*
        Vector2Int[,] parents = new Vector2Int[(endRow - startRow) + 1, (endCol - startCol) + 1];
        List<PriorityQueueNode> nodesToVisit = new List<PriorityQueueNode>();
        List<PriorityQueueNode> nodesVisited = new List<PriorityQueueNode>();
        // Controle dos blocos disponíveis para caminhar
        bool[,] visited = new bool[(endRow - startRow) + 1, (endCol - startCol) + 1];

        // Insere a posição inicial nas listas
        nodesToVisit.Add(new PriorityQueueNode(pos, 0));
        parents[pos.x - startRow, pos.y - startCol] = new Vector2Int(-1, -1);

        // Executa A*
        while(nodesToVisit.Count > 0){
            // Seleciona índice com menor custo
            int bestIndex = 0;
            for(int i = 1; i < nodesToVisit.Count; i++)
                if(nodesToVisit[i].cost < nodesToVisit[bestIndex].cost)
                    bestIndex = i;
            // Recebe dados do nó para analisar
            PriorityQueueNode cur = nodesToVisit[bestIndex];
            nodesToVisit.RemoveAt(bestIndex);
            // Verifica posições vizinhas
            foreach(Vector2Int move in VoxelData.movements){
                PriorityQueueNode neighbor = new PriorityQueueNode(new Vector2Int(cur.pos.x + move.x, cur.pos.y + move.y), cur.cost + 1);
                if(Map.singleton.IsPositionInMap(neighbor.pos.x, neighbor.pos.y) && NetworkMap.singleton.GetMapContent(neighbor.pos.x, neighbor.pos.y).canWalk() && neighbor.cost <= availableMoves){
                    bool hasLowerCost = false;
                    // Procura por nó na mesma posição (lista de nós abertos), mas com custo menor
                    foreach(PriorityQueueNode cmpNode in nodesToVisit){ 
                        if(cmpNode.pos.Equals(neighbor.pos) && cmpNode.cost < neighbor.cost){ 
                            hasLowerCost = true;
                            break;
                        }
                    }
                    // Procura por nó na mesma posição (lista de nós fechados), mas com custo menor
                    foreach(PriorityQueueNode cmpNode in nodesVisited){
                        if(cmpNode.pos.Equals(neighbor.pos) && cmpNode.cost < neighbor.cost){
                            hasLowerCost = true;
                            break;
                        }
                    }   
                    // Se não tiver encontrado nós com custo menor, adiciona vizinho na lista
                    if(!hasLowerCost){
                        // Define o pai da posição adjacente
                        parents[neighbor.pos.x - startRow,neighbor.pos.y - startCol] = cur.pos;
                        // Marca posição como visitada
                        visited[neighbor.pos.x - startRow, neighbor.pos.y - startCol] = true;
                        nodesToVisit.Add(neighbor);
                    }
                }
            }
            nodesVisited.Add(cur);
        }

        nodesToVisit.Clear();
        nodesVisited.Clear();

        movements = new MovementData(parents, visited, startRow, endRow, startCol, endCol);
    }

    // Constrói o caminho até o ponto de destino usando a matriz de parentesco
    public void DrawPath(Vector2Int goal){
        TileManager.singleton.ClearPathInstances();
        path.Clear();
        // Posições
        Vector2Int previous = goal;
        Vector2Int cur = movements.parent[goal.x - movements.startRow, goal.y - movements.startCol];
        // Direções
        VoxelData.MoveDirection previousDir;
        VoxelData.MoveDirection curDir;
        Vector2Int dir;

        // Calcula direção para posicionar seta
        dir = new Vector2Int(previous.x - cur.x, previous.y - cur.y);
        // Insere destino ao caminho
        path.Push(dir);
        // Instancia seta no mapa e define primeira direção
        curDir = VoxelData.GetDirectionIndex(dir);
        TileManager.singleton.InstantiatePathTile(previous, curDir, TileManager.PathEnum.Arrow);
        
        while(!(cur.x == -1 && cur.y == -1)){
            // Atualiza posições
            previous = cur;
            cur = movements.parent[previous.x - movements.startRow, previous.y - movements.startCol];
            // Critério de parada
            if(!(cur.x == -1 && cur.y == -1)){
                // Calcula direção em formato de vetor e recebe o índice correspondente
                previousDir = curDir;
                dir = new Vector2Int(previous.x - cur.x, previous.y - cur.y);
                curDir = VoxelData.GetDirectionIndex(dir);
                // Avalia qual será o tile utilizado na posição "previous"
                if(previousDir == curDir) TileManager.singleton.InstantiatePathTile(previous, previousDir, TileManager.PathEnum.Line);
                else{
                    VoxelData.MoveDirection tileDir = VoxelData.GetCurveDirection(previousDir, curDir);
                    TileManager.singleton.InstantiatePathTile(previous, tileDir, TileManager.PathEnum.Curve);
                }
                // Armazena posição atual na pilha
                path.Push(dir);
            }
        }
    }
}

public class PriorityQueueNode{
    public Vector2Int pos;
    public int cost;

    public PriorityQueueNode(Vector2Int pos, int cost){
        this.pos = pos;
        this.cost = cost;
    }
}

public class MovementData{
    public Vector2Int[,] parent;
    public bool[,] visited;
    public int startRow;
    public int endRow;
    public int startCol;
    public int endCol;

    public MovementData(Vector2Int[,] parent, bool[,] visited, int startRow, int endRow, int startCol, int endCol){
        this.parent = parent;
        this.visited = visited;
        this.startRow = startRow;
        this.endRow = endRow;
        this.startCol = startCol;
        this.endCol = endCol;
    }
}