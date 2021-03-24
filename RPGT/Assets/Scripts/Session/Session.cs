using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Session : MonoBehaviour{
    // Prefabs para geração das entidades 
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    // Objetos para organização das entidades
    public Transform playerList;
    public Transform enemyList;
    public Transform markerInstanceList;
    public Transform pathInstanceList;
    // Dados para controle da partida
    public int numOfPlayers;
    public int numOfEnemies;
    private List<Initiative> turnQueue = new List<Initiative>();
    private bool[,] canWalk;
    private MovementData movements;
    // Valores dos atributos das entidades
    public List<EntityData> playerData = new List<EntityData>();
    public List<EntityData> enemyData = new List<EntityData>(); 
    // Conjunto de entidades da partida
    private Map map;
    private BlockData blockData;
    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();

    private GameObject curEntity;
    Stack<Vector2Int> path = new Stack<Vector2Int>();

    void Start(){
        map = GameObject.Find("GameHandler").GetComponent<Map>();
        blockData = GameObject.Find("DataHandler").GetComponent<BlockData>();
        canWalk = new bool[map.mapRows, map.mapCols];
        InstantiatePlayers();
        InstantiateEnemies();  
        InitTurnQueue();        
        SetWalkablePositions();            
        Turn();
    }

    // Cria instancias para todos os jogadores da partida
    void InstantiatePlayers(){
        Player player;
        for(int i = 0; i < numOfPlayers; i++){
            players.Add(Instantiate(playerPrefab, new Vector3(i + 0.5f, 1.5f, 0.5f), Quaternion.identity));
            SetPlayerData(i);
            player = players[i].GetComponent<Player>();
            players[i].name = "Player " + (i+1);
            // Recebe as informações base do jogador
            players[i].GetComponent<Player>().gridCoord = new Vector2Int(i, 0);
            players[i].GetComponent<Player>().curHealth = player.data.health;
            players[i].GetComponent<Player>().curInitiative = player.data.initiative;
            players[i].GetComponent<Player>().curMoveDistance = player.data.moveDistance;
            // Coloca a instância no objeto de listagem
            players[i].transform.SetParent(playerList);
        }
    }

    // Cria instancias para todos os jogadores da partida
    void InstantiateEnemies(){
        Player enemy;
        for(int i = 0; i < numOfEnemies; i++){
            enemies.Add(Instantiate(enemyPrefab, new Vector3(i + 0.5f, 1.5f, (map.mapCols - 1) + 0.5f), Quaternion.identity));
            SetEnemyData(i);
            enemy = enemies[i].GetComponent<Player>();
            enemies[i].name = "Enemy " + (i+1);
            // Recebe as informações base do inimigo
            enemies[i].GetComponent<Player>().gridCoord = new Vector2Int(i, map.mapCols - 1);
            enemies[i].GetComponent<Player>().curHealth = enemy.data.health;
            enemies[i].GetComponent<Player>().curInitiative = enemy.data.initiative;
            enemies[i].GetComponent<Player>().curMoveDistance = enemy.data.moveDistance;
            // Coloca a instância no objeto de listagem
            enemies[i].transform.SetParent(enemyList);
        }
    }

    // Atribui os dados para o jogador correspondente
    void SetPlayerData(int index){
        players[index].GetComponent<Player>().data = playerData[index];
    }

    // Atribui os dados para o inimigo correspondente
    void SetEnemyData(int index){
        enemies[index].GetComponent<Player>().data = enemyData[index];
    }

    // Inicializa a fila que determina a ordem de jogadas do turno
    void InitTurnQueue(){
        turnQueue.Clear();
        // Insere as entidades na lista
        for(int i = 0; i < numOfPlayers; i++) turnQueue.Add(new Initiative(i, players[i].GetComponent<Player>().curInitiative));
        for(int i = 0; i < numOfEnemies; i++) turnQueue.Add(new Initiative(i + numOfPlayers, enemies[i].GetComponent<Player>().curInitiative));
        // Ordena a lista pela iniciativa
        // TODO substituir por uma fila de prioridade e incluir atributo de turno
        turnQueue.Sort((e1, e2) => -e1.initiative.CompareTo(e2.initiative));
    }

    // Inicializa matriz de posições percorríveis de acordo com o mapa e a posição das entidades
    void SetWalkablePositions(){
        // Marca coordenadas cujos blocos apresentam alguma restrição de passagem
        for(int i = 0; i < map.mapRows; i++)
            for(int j = 0; j < map.mapCols; j++)
                canWalk[i,j] = blockData.blockList[map.voxelMap[i,j]].canWalk;

        Vector2Int pos;
        // Marca as posições dos jogadores como inválidas (para evitar duas entidades na mesma posição)   
        for(int i = 0; i < numOfPlayers; i++){
            pos = new Vector2Int(Mathf.FloorToInt(players[i].transform.position.x - 0.5f), Mathf.FloorToInt(players[i].transform.position.z - 0.5f));
            canWalk[pos.x, pos.y] = false;
        }
        // Marca as posições dos inimigos como inválidas (para evitar duas entidades na mesma posição)   
        for(int i = 0; i < numOfEnemies; i++){
            pos = new Vector2Int(Mathf.FloorToInt(enemies[i].transform.position.x - 0.5f), Mathf.FloorToInt(enemies[i].transform.position.z - 0.5f));
            canWalk[pos.x, pos.y] = false;
        }
    }

    // Executa o turno para uma entidade
    void Turn(){
        ClearPathInstances();
        // Recebe entidade com maior prioridade na lista
        Initiative entityInitiative = turnQueue[0];
        // Verifica se é jogador ou inimigo e excuta movimentação
        bool isPlayer = (entityInitiative.id < numOfPlayers); 
        curEntity = (isPlayer) ? players[entityInitiative.id] : enemies[entityInitiative.id - numOfPlayers];
        Vector2Int entityCoord = new Vector2Int(Mathf.FloorToInt(curEntity.transform.position.x - 0.5f), Mathf.FloorToInt(curEntity.transform.position.z - 0.5f)); 
        movements = GetAvailableMovements(entityCoord, GetMovementDistance(curEntity.GetComponent<Player>().curMoveDistance));
        // Atualiza lista e limpa marcadores
        turnQueue.RemoveAt(0);
        turnQueue.Add(entityInitiative);
    }

    // Calcula o número de movimentos que a entidade pode realizar
    int GetMovementDistance(int baseMoveDistance){
        return baseMoveDistance;
    }

    // Calcula e exibe movimentações possíveis usando A*
    MovementData GetAvailableMovements(Vector2Int pos, int availableMoves){
        // Instancia marcador para a posição inicial
        GameObject entityPosPath = Instantiate(blockData.markerList[(int)BlockData.MarkerEnum.EntityPos], new Vector3(pos.x + 0.5f, 1.001f, pos.y + 0.5f), Quaternion.identity);
        entityPosPath.GetComponent<PathCoord>().coord = pos;
        entityPosPath.name = pos.x + "," + pos.y;
        entityPosPath.transform.SetParent(markerInstanceList);
        entityPosPath.SetActive(true);
        // Dados para o A*
        Vector2Int[,] parents = new Vector2Int[map.mapRows, map.mapCols];
        List<PriorityQueueNode> nodesToVisit = new List<PriorityQueueNode>();
        List<PriorityQueueNode> nodesVisited = new List<PriorityQueueNode>();
        // Controle dos blocos disponíveis para caminhar
        GameObject availablePath;
        bool[,] visited = new bool[map.mapRows, map.mapCols];
        // Marca todas as posições como não visitdas
        for(int i = 0; i < map.mapRows; i++)
            for(int j = 0; j < map.mapCols; j++)
                visited[i,j] = false;

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
                if(IsPositionInMap(neighbor.pos.x, neighbor.pos.y) && canWalk[neighbor.pos.x, neighbor.pos.y] && neighbor.cost <= availableMoves){
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

        for(int i = 0; i < map.mapRows; i++){
            for(int j = 0; j < map.mapCols; j++){
                if(visited[i,j]){
                    // Instancia marcador para posição disponível
                    availablePath = Instantiate(blockData.markerList[(int)BlockData.MarkerEnum.CanWalkYes], new Vector3(i + 0.5f, 1.001f, j + 0.5f), Quaternion.identity);
                    availablePath.GetComponent<PathCoord>().coord = new Vector2Int(i,j);
                    availablePath.name = i + "," + j;
                    availablePath.transform.SetParent(markerInstanceList);
                    availablePath.SetActive(true);   
                }
            }
        }

        nodesToVisit.Clear();
        nodesVisited.Clear();
        return new MovementData(parents, 0, map.mapRows-1, 0, map.mapCols-1);
    }

    bool IsPositionInMap(int x, int y){
        if(x < 0 || x >= map.mapRows) return false; 
        if(y < 0 || y >= map.mapCols) return false;
        return true;
    }

    // Constrói o caminho até o ponto de destino usando a matriz de parentesco
    public void DrawPath(Vector2Int goal){
        ClearPathInstances();
        // Posições
        Vector2Int previous = new Vector2Int(goal.x - movements.startRow, goal.y - movements.startCol);
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
        InstantiatePathTile(previous, curDir, BlockData.PathEnum.Arrow);
        
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
                if(previousDir == curDir) InstantiatePathTile(previous, previousDir, BlockData.PathEnum.Line);
                else{
                    VoxelData.MoveDirection tileDir = VoxelData.GetCurveDirection(previousDir, curDir);
                    InstantiatePathTile(previous, tileDir, BlockData.PathEnum.Curve);
                }
                // Armazena posição atual na pilha
                path.Push(dir);
            }
        }
    }

    // Movimenta o personagem
    public void Move(Vector2Int goal){
        Vector2Int gridMove;
        Vector2Int gridPos = curEntity.GetComponent<Player>().gridCoord;
        
        canWalk[gridPos.x, gridPos.y] = true;
        canWalk[goal.x, goal.y] = false;

        while(path.Count > 0){
            gridMove = path.Pop();
            gridPos = new Vector2Int(gridMove.x, gridMove.y); 
            curEntity.transform.Translate(new Vector3(gridPos.x, 0f, gridPos.y));
        }

        // Atualiza posição do personagem
        curEntity.GetComponent<Player>().gridCoord = goal;
        ClearMarkerInstances();
        Turn();
    }

    void InstantiatePathTile(Vector2Int pos, VoxelData.MoveDirection dir, BlockData.PathEnum tile){
        GameObject pathTile = Instantiate(blockData.pathList[(int)tile], new Vector3(pos.x + 0.5f, 1.003f, pos.y + 0.5f), Quaternion.identity);
        pathTile.transform.SetParent(pathInstanceList);
        pathTile.SetActive(true);
        // Checa a direção da seta para efetuar rotação do objeto
        if(dir == VoxelData.MoveDirection.North) pathTile.transform.Rotate(90f, 0f, 90f, Space.World);
        else if(dir == VoxelData.MoveDirection.East) pathTile.transform.Rotate(90f, 0f, 180f, Space.World);
        else if(dir == VoxelData.MoveDirection.South) pathTile.transform.Rotate(90f, 0f, -90f, Space.World);
        else if(dir == VoxelData.MoveDirection.West) pathTile.transform.Rotate(90f, 0f, 0f, Space.World);
    }

    // Remove marcadores de caminho do cenário
    public void ClearPathInstances(){
        GameObject[] allChildren = new GameObject[pathInstanceList.childCount];
        int childIndex = 0;

        foreach(Transform child in pathInstanceList){
            allChildren[childIndex] = child.gameObject;
            childIndex += 1;
        }

        foreach(GameObject child in allChildren) 
            DestroyImmediate(child.gameObject); 
        path.Clear(); 
    }

    // Remove marcadores do cenário
    void ClearMarkerInstances(){
        GameObject[] allChildren = new GameObject[markerInstanceList.childCount];
        int childIndex = 0;

        foreach(Transform child in markerInstanceList){
            allChildren[childIndex] = child.gameObject;
            childIndex += 1;
        }

        foreach(GameObject child in allChildren) 
            DestroyImmediate(child.gameObject);
    }
}

public class Initiative{
    public int id;
    public int initiative;
    public bool isActive;

    public Initiative(int id, int initiative){
        this.id = id;
        this.initiative = initiative;
        this.isActive = true;
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
    public int startRow;
    public int endRow;
    public int startCol;
    public int endCol;

    public MovementData(Vector2Int[,] parent, int startRow, int endRow, int startCol, int endCol){
        this.parent = parent;
        this.startRow = startRow;
        this.endRow = endRow;
        this.startCol = startCol;
        this.endCol = endCol;
    }
}