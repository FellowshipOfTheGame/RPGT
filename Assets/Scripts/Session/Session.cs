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
    // Dados para controle da partida
    public int numOfPlayers;
    public int numOfEnemies;
    private List<Initiative> turnQueue = new List<Initiative>();
    public static bool[,] canWalk;
    private MovementData movements;
    // Valores dos atributos das entidades
    public List<EntityData> playerData = new List<EntityData>();
    public List<EntityData> enemyData = new List<EntityData>(); 
    // Conjunto de entidades da partida
    private Map map;
    private BlockData blockData;
    private TileManager tileManager;
    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();

    private GameObject curEntity;
    Stack<Vector2Int> path = new Stack<Vector2Int>();

    void Start(){
        map = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<Map>();
        blockData = GameObject.FindGameObjectWithTag("DataHandler").GetComponent<BlockData>();
        tileManager = GameObject.Find("TileHandler").GetComponent<TileManager>();
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
            players.Add(Instantiate(playerPrefab, new Vector3(i + map.centerOffset, 3*map.centerOffset, map.centerOffset), Quaternion.identity));
            players[i].GetComponent<PlayerMovement>().map = map;
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
            enemies.Add(Instantiate(enemyPrefab, new Vector3(i + map.centerOffset, 3*map.centerOffset, (map.mapCols - 1) + map.centerOffset), Quaternion.identity));
            enemies[i].GetComponent<PlayerMovement>().map = map;
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
            pos = players[i].GetComponent<Player>().gridCoord;
            canWalk[pos.x, pos.y] = false;
        }
        // Marca as posições dos inimigos como inválidas (para evitar duas entidades na mesma posição)   
        for(int i = 0; i < numOfEnemies; i++){
            pos = enemies[i].GetComponent<Player>().gridCoord;
            canWalk[pos.x, pos.y] = false;
        }
    }

    // Executa o turno para uma entidade
    void Turn(){
        tileManager.ClearPathInstances();
        path.Clear();
        // Recebe entidade com maior prioridade na lista
        Initiative entityInitiative = turnQueue[0];
        // Verifica se é jogador ou inimigo e excuta movimentação
        bool isPlayer = (entityInitiative.id < numOfPlayers); 
        curEntity = (isPlayer) ? players[entityInitiative.id] : enemies[entityInitiative.id - numOfPlayers];
        Vector2Int entityCoord = curEntity.GetComponent<Player>().gridCoord;
        // Coloca marcador na posição atual do personagem
        tileManager.InstantiateMarkerTile(entityCoord, BlockData.MarkerEnum.EntityPos);
        // Calcula movimentos possíveis
        movements = curEntity.GetComponent<PlayerMovement>().GetAvailableMovements(entityCoord);
        // Coloca marcador nas posições onde o personagem pode andar
        for(int i = 0; i <= movements.endRow - movements.startRow; i++)
            for(int j = 0; j <= movements.endCol - movements.startCol; j++)
                if(movements.visited[i,j]) 
                    tileManager.InstantiateMarkerTile(new Vector2Int(i + movements.startRow,j + movements.startCol), BlockData.MarkerEnum.CanWalkYes);  
        // Atualiza lista e limpa marcadores
        turnQueue.RemoveAt(0);
        turnQueue.Add(entityInitiative);
    }

    // Constrói o caminho até o ponto de destino usando a matriz de parentesco
    public void DrawPath(Vector2Int goal){
        tileManager.ClearPathInstances();
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
        tileManager.InstantiatePathTile(previous, curDir, BlockData.PathEnum.Arrow);
        
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
                if(previousDir == curDir) tileManager.InstantiatePathTile(previous, previousDir, BlockData.PathEnum.Line);
                else{
                    VoxelData.MoveDirection tileDir = VoxelData.GetCurveDirection(previousDir, curDir);
                    tileManager.InstantiatePathTile(previous, tileDir, BlockData.PathEnum.Curve);
                }
                // Armazena posição atual na pilha
                path.Push(dir);
            }
        }
    }

    // Movimenta o personagem
    public void Move(Vector2Int goal){
        // Atualiza matriz de posições
        Vector2Int gridPos = curEntity.GetComponent<Player>().gridCoord;
        canWalk[gridPos.x, gridPos.y] = true;
        canWalk[goal.x, goal.y] = false;
        // Executa movimentação do jogador
        curEntity.GetComponent<PlayerMovement>().Move(path, goal);
        // Inicia próximo turno
        tileManager.ClearMarkerInstances();
        Turn();
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