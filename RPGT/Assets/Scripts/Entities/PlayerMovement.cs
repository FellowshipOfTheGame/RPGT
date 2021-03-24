using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{
    public Map map;

    public void Move(Stack<Vector2Int> path, Vector2Int goal){
        Vector2Int gridMove;
        Vector3 target;
        // Percorre cada posição na grid conforme o caminho
        while(path.Count > 0){
            gridMove = path.Pop(); 
            target = new Vector3(this.transform.position.x + gridMove.x, this.transform.position.y, this.transform.position.z + gridMove.y);
            this.transform.position = Vector3.MoveTowards(this.transform.position, target, 100f);
        }
        // Atualiza posição do personagem
        gameObject.GetComponent<Player>().gridCoord = goal;
    }

    // Calcula o número de movimentos que a entidade pode realizar
    public int GetMovementDistance(){
        return this.GetComponent<Player>().curMoveDistance;
    }

    // Calcula e exibe movimentações possíveis usando A*
    public MovementData GetAvailableMovements(Vector2Int pos){
        int availableMoves = GetMovementDistance();
        // Dados para o A*
        Vector2Int[,] parents = new Vector2Int[map.mapRows, map.mapCols];
        List<PriorityQueueNode> nodesToVisit = new List<PriorityQueueNode>();
        List<PriorityQueueNode> nodesVisited = new List<PriorityQueueNode>();
        // Controle dos blocos disponíveis para caminhar
        bool[,] visited = new bool[map.mapRows, map.mapCols];

        // Insere a posição inicial nas listas
        nodesToVisit.Add(new PriorityQueueNode(pos, 0));
        parents[pos.x, pos.y] = new Vector2Int(-1, -1);

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
                if(map.IsPositionInMap(neighbor.pos.x, neighbor.pos.y) && Session.canWalk[neighbor.pos.x, neighbor.pos.y] && neighbor.cost <= availableMoves){
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
                        parents[neighbor.pos.x,neighbor.pos.y] = cur.pos;
                        // Marca posição como visitada
                        visited[neighbor.pos.x, neighbor.pos.y] = true;
                        nodesToVisit.Add(neighbor);
                    }
                }
            }
            nodesVisited.Add(cur);
        }

        nodesToVisit.Clear();
        nodesVisited.Clear();

        return new MovementData(parents, visited, 0, map.mapRows-1, 0, map.mapCols-1);
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