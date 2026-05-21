using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float tileSize = 1f;

    [Header("Prefabs")]
    [SerializeField] private List<CharacterPrefab> characterPrefabs;

    private Dictionary<AsciiCharacters, GameObject> prefabDictionary;

    private AsciiCharacters[,] map;
    private Vector2Int _playerPosition;
    private DungeonEnemyController _enemyController;
    private DungeonCombatController _combatController;
    private TurnManager _turnManager;
    private DungeonFileNavigator _fileNavigator;
    private string _lastEnemyTurnSummary;
    private int _enemyTurnCounter;

    [Header("Combat")]
    [SerializeField] private int startingPlayerHealth = 10;
    private int _playerHealth;

    [Header("Dungeon Files")]
    [SerializeField] private string dungeonRootPath = "users/dungeons";
    [SerializeField] private GameObject batFilePopup;

   

    [System.Serializable]
    public class CharacterPrefab
    {
        public AsciiCharacters character;
        public GameObject prefab;
    }
    
    void Awake()
    {
        BuildDictionary();
        _playerHealth = startingPlayerHealth;
        _fileNavigator = new DungeonFileNavigator(dungeonRootPath);

        if (map == null)
        {
            SetMap(CreateRoomForCurrentFile());
        }
    }

    private AsciiCharacters[,] CreateRoomForCurrentFile()
    {
        if (_fileNavigator == null)
        {
            _fileNavigator = new DungeonFileNavigator(dungeonRootPath);
        }

        return CreateFirstRoom(9, 9, _fileNavigator.CurrentFileIndex);
    }

    private AsciiCharacters[,] CreateFirstRoom(int width, int height, int levelIndex)
    {
        if (width < 3) width = 3;
        if (height < 3) height = 3;

        AsciiCharacters[,] room = new AsciiCharacters[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool border = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                room[x, y] = border ? AsciiCharacters.Wall : AsciiCharacters.Empty;
            }
        }

        room[width / 2, height / 2] = AsciiCharacters.Player;

        Vector2Int[] spawnPoints =
        {
            new Vector2Int(width / 2 + 1, height / 2),
            new Vector2Int(width / 2 - 2, height / 2),
            new Vector2Int(width / 2, height / 2 + 2),
            new Vector2Int(width / 2, height / 2 - 2),
            new Vector2Int(width / 2 + 2, height / 2 + 1),
            new Vector2Int(width / 2 - 2, height / 2 - 1),
            new Vector2Int(width / 2 + 1, height / 2 - 2),
            new Vector2Int(width / 2 - 1, height / 2 + 2)
        };

        List<AsciiCharacters> enemies = new List<AsciiCharacters>();
        int malwareCount = 1 + Mathf.Clamp(levelIndex, 0, 4);
        int virusCount = 1 + Mathf.Clamp(levelIndex - 1, 0, 4);
        int trojanCount = Mathf.Clamp(levelIndex, 0, 4);
        int ransomwareCount = Mathf.Clamp(levelIndex - 2, 0, 4);

        for (int i = 0; i < malwareCount; i++) enemies.Add(AsciiCharacters.Malware);
        for (int i = 0; i < virusCount; i++) enemies.Add(AsciiCharacters.Virus);
        for (int i = 0; i < trojanCount; i++) enemies.Add(AsciiCharacters.Trojan);
        for (int i = 0; i < ransomwareCount; i++) enemies.Add(AsciiCharacters.Ransomware);

        int spawnCount = Mathf.Min(spawnPoints.Length, enemies.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2Int p = spawnPoints[i];
            if (p.x > 0 && p.x < width - 1 && p.y > 0 && p.y < height - 1)
            {
                room[p.x, p.y] = enemies[i];
            }
        }

        return room;
    }


    public void SetMap(AsciiCharacters[,] newMap)
    {
        if (_turnManager != null)
        {
            _turnManager.OnTurnAdvanced -= HandleEnemyTurn;
        }

        if (prefabDictionary == null)
        {
            BuildDictionary();
        }

        map = CloneMap(newMap);
        _enemyTurnCounter = 0;
        if (!TryFindPlayer(out _playerPosition))
        {
            _playerPosition = new Vector2Int(map.GetLength(0) / 2, map.GetLength(1) / 2);
            map[_playerPosition.x, _playerPosition.y] = AsciiCharacters.Player;
        }

        _enemyController = new DungeonEnemyController();
        _enemyController.InitializeFromMap(map.GetLength(0), map.GetLength(1), (x, y) => IsEnemyTile(map[x, y]));
        _combatController = new DungeonCombatController();
        _turnManager = new TurnManager();
        _turnManager.OnTurnAdvanced += HandleEnemyTurn;
        _lastEnemyTurnSummary = string.Empty;

        RefreshView();
    }

    public bool TryMovePlayer(MonitorCommandType direction, out string result)
    {
        Vector2Int delta;

        switch (direction)
        {
            case MonitorCommandType.North:
                delta = new Vector2Int(0, 1);
                break;
            case MonitorCommandType.South:
                delta = new Vector2Int(0, -1);
                break;
            case MonitorCommandType.East:
                delta = new Vector2Int(1, 0);
                break;
            case MonitorCommandType.West:
                delta = new Vector2Int(-1, 0);
                break;
            default:
                result = "Invalid direction. Use: north | south | east | west.";
                return false;
        }

        Vector2Int target = _playerPosition + delta;

        if (!IsInside(target))
        {
            result = "You can't move there.";
            return false;
        }

        AsciiCharacters targetTile = map[target.x, target.y];
        if (targetTile == AsciiCharacters.Wall)
        {
            result = "A wall blocks your path.";
            return false;
        }

        if (IsEnemyTile(targetTile))
        {
            result = "An enemy blocks your path.";
            return false;
        }

        map[_playerPosition.x, _playerPosition.y] = AsciiCharacters.Empty;
        _playerPosition = target;
        map[_playerPosition.x, _playerPosition.y] = AsciiCharacters.Player;

        AdvanceTurn();
        RefreshView();

        result = string.IsNullOrEmpty(_lastEnemyTurnSummary)
            ? "You moved " + direction + "."
            : "You moved " + direction + ". " + _lastEnemyTurnSummary;
        return true;
    }

    public string Attack(string targetOrDirection)
    {
        if (string.IsNullOrWhiteSpace(targetOrDirection))
        {
            return "Choose an attack direction: north | south | east | west.";
        }

        Vector2Int delta = DirectionFromString(targetOrDirection);
        if (delta == Vector2Int.zero)
        {
            return "Invalid attack direction.";
        }

        Vector2Int target = _playerPosition + delta;
        if (!IsInside(target))
        {
            return "You swing into empty space.";
        }

        AsciiCharacters tile = map[target.x, target.y];
        if (!IsEnemyTile(tile))
        {
            return "No enemy in that direction.";
        }

        _combatController.DealDamageToEnemy(target, 1, _enemyController);
        if (!_enemyController.IsEnemyAlive(target))
        {
            map[target.x, target.y] = AsciiCharacters.Empty;
        }

        AdvanceTurn();
        RefreshView();

        return string.IsNullOrEmpty(_lastEnemyTurnSummary)
            ? "Hit confirmed."
            : "Hit confirmed. " + _lastEnemyTurnSummary;
    }

    public string GetCurrentPath()
    {
        if (_fileNavigator == null)
        {
            _fileNavigator = new DungeonFileNavigator(dungeonRootPath);
        }

        return _fileNavigator.GetCurrentPath();
    }

    public string GetStatusOverview()
    {
        if (_fileNavigator == null)
        {
            _fileNavigator = new DungeonFileNavigator(dungeonRootPath);
        }

        string currentPath = _fileNavigator.GetCurrentPath();
        int fileNumber = _fileNavigator.CurrentFileIndex + 1;
        int totalFiles = _fileNavigator.FileCount;
        int aliveEnemies = CountAliveEnemies();

        string nextHint;
        if (_fileNavigator.TryGetNextPath(out string nextPath))
        {
            nextHint = nextPath;
        }
        else
        {
            nextHint = "No further files. This is the final endpoint.";
        }

        return "Path: " + currentPath
            + " | File " + fileNumber + "/" + totalFiles
            + " | Enemies remaining: " + aliveEnemies
            + " | Next file: " + nextHint;
    }

    public string GetQuickTutorial()
    {
        return "[SYS] This is turn-based: each move or attack spends one turn.\n"
            + "[SYS] move <north|south|east|west> moves you one tile in that direction.\n"
            + "[SYS] attack <direction> hits one adjacent tile.\n"
            + "[SYS] Clear all enemies to proceed, then run locate to continue.\n"
            + "[SYS] Use tutorial anytime to re-open training pages.";
    }

    public string LocateNextFile()
    {
        if (_enemyController != null)
        {
            foreach (Vector2Int enemy in _enemyController.GetAliveEnemyPositions())
            {
                if (_enemyController.IsEnemyAlive(enemy))
                {
                    return "Access denied. Eliminate all enemies first. Use 'status' to see enemies remaining and next path hint.";
                }
            }
        }

        if (_fileNavigator == null)
        {
            _fileNavigator = new DungeonFileNavigator(dungeonRootPath);
        }

        bool advanced = _fileNavigator.TryAdvance(out string nextPath);

        if (batFilePopup != null)
        {
            batFilePopup.SetActive(true);
        }

        if (!advanced && _fileNavigator.IsComplete)
        {
            return "All files cleared. Final path reached: " + nextPath + ". BAT files are exhausted.";
        }

        SetMap(CreateRoomForCurrentFile());
        return "BAT opened and executed. Next file located: " + nextPath + ". Use 'status' if you need direction hints.";
    }

    private void AdvanceTurn()
    {
        _lastEnemyTurnSummary = string.Empty;
        _turnManager?.AdvanceTurn();

        if (AllEnemiesDefeated())
        {
            if (batFilePopup != null)
            {
                batFilePopup.SetActive(true);
            }

            _lastEnemyTurnSummary = "All enemies eliminated. BAT file available. Use 'locate' to proceed.";
        }
    }

    private void HandleEnemyTurn(int turnNumber)
    {
        _lastEnemyTurnSummary = ProcessEnemyTurn();
    }

    private AsciiCharacters[,] CloneMap(AsciiCharacters[,] source)
    {
        int width = source.GetLength(0);
        int height = source.GetLength(1);
        AsciiCharacters[,] copy = new AsciiCharacters[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                copy[x, y] = source[x, y];
            }
        }

        return copy;
    }

    private bool TryFindPlayer(out Vector2Int position)
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == AsciiCharacters.Player)
                {
                    position = new Vector2Int(x, y);
                    return true;
                }
            }
        }

        position = default;
        return false;
    }

    private int CountAliveEnemies()
    {
        if (_enemyController == null)
        {
            return 0;
        }

        int alive = 0;
        foreach (Vector2Int enemy in _enemyController.GetAliveEnemyPositions())
        {
            if (_enemyController.IsEnemyAlive(enemy))
            {
                alive++;
            }
        }

        return alive;
    }

    private bool IsInside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < map.GetLength(0) && pos.y >= 0 && pos.y < map.GetLength(1);
    }

    private bool IsEnemyTile(AsciiCharacters tile)
    {
        return tile == AsciiCharacters.Malware
            || tile == AsciiCharacters.Virus
            || tile == AsciiCharacters.Trojan
            || tile == AsciiCharacters.Ransomware;
    }

    private Vector2Int DirectionFromString(string direction)
    {
        switch (direction.Trim().ToLowerInvariant())
        {
            case "north":
            case "n":
                return new Vector2Int(0, 1);
            case "south":
            case "s":
                return new Vector2Int(0, -1);
            case "east":
            case "e":
                return new Vector2Int(1, 0);
            case "west":
            case "w":
                return new Vector2Int(-1, 0);
            default:
                return Vector2Int.zero;
        }
    }

    private string ProcessEnemyTurn()
    {
        if (_enemyController == null || _combatController == null)
        {
            return string.Empty;
        }

        _enemyTurnCounter++;
        bool canMoveThisTurn = _enemyTurnCounter % 2 == 0;

        int attacks = 0;
        int moves = 0;
        int enemyDamage = GetEnemyAttackDamage();
        List<Vector2Int> enemies = new List<Vector2Int>(_enemyController.GetAliveEnemyPositions());

        foreach (Vector2Int enemyPos in enemies)
        {
            if (!_enemyController.IsEnemyAlive(enemyPos))
            {
                continue;
            }

            if (!IsInside(enemyPos) || !IsEnemyTile(map[enemyPos.x, enemyPos.y]))
            {
                continue;
            }

            int distance = Mathf.Abs(enemyPos.x - _playerPosition.x) + Mathf.Abs(enemyPos.y - _playerPosition.y);
            if (distance <= 1)
            {
                _combatController.DealDamageToPlayer(ref _playerHealth, enemyDamage);
                attacks++;
                continue;
            }

            if (!canMoveThisTurn)
            {
                continue;
            }

            Vector2Int next = GetStepTowards(enemyPos, _playerPosition);
            if (next == enemyPos)
            {
                continue;
            }

            if (next == _playerPosition)
            {
                _combatController.DealDamageToPlayer(ref _playerHealth, enemyDamage);
                attacks++;
                continue;
            }

            if (map[next.x, next.y] == AsciiCharacters.Empty)
            {
                map[next.x, next.y] = map[enemyPos.x, enemyPos.y];
                map[enemyPos.x, enemyPos.y] = AsciiCharacters.Empty;
                _enemyController.MoveEnemy(enemyPos, next);
                moves++;
            }
        }

        if (attacks == 0 && moves == 0)
        {
            return string.Empty;
        }

        string summary = "Enemies: ";
        if (moves > 0)
        {
            summary += moves + " moved";
        }

        if (attacks > 0)
        {
            if (moves > 0) summary += ", ";
            summary += attacks + " attacked you (HP: " + _playerHealth + ")";
        }

        return summary + ".";
    }

    private Vector2Int GetStepTowards(Vector2Int from, Vector2Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        Vector2Int primary = Mathf.Abs(dx) >= Mathf.Abs(dy)
            ? new Vector2Int(Math.Sign(dx), 0)
            : new Vector2Int(0, Math.Sign(dy));
        Vector2Int secondary = primary.x != 0
            ? new Vector2Int(0, Math.Sign(dy))
            : new Vector2Int(Math.Sign(dx), 0);

        Vector2Int candidate = from + primary;
        if (primary != Vector2Int.zero && IsInside(candidate) && (map[candidate.x, candidate.y] == AsciiCharacters.Empty || candidate == _playerPosition))
        {
            return candidate;
        }

        candidate = from + secondary;
        if (secondary != Vector2Int.zero && IsInside(candidate) && (map[candidate.x, candidate.y] == AsciiCharacters.Empty || candidate == _playerPosition))
        {
            return candidate;
        }

        return from;
    }

    private int GetEnemyAttackDamage()
    {
        if (_fileNavigator == null)
        {
            return 1;
        }

        return 1 + Mathf.Clamp(_fileNavigator.CurrentFileIndex, 0, 4);
    }

    private void RefreshView()
    {
        GenerateMap(map);
    }

    private bool AllEnemiesDefeated()
    {
        if (_enemyController == null)
        {
            return false;
        }

        foreach (Vector2Int enemy in _enemyController.GetAliveEnemyPositions())
        {
            if (_enemyController.IsEnemyAlive(enemy))
            {
                return false;
            }
        }

        return true;
    }

    private void BuildDictionary()
    {
        prefabDictionary = new Dictionary<AsciiCharacters, GameObject>();

        foreach (var item in characterPrefabs)
        {
            if (!prefabDictionary.ContainsKey(item.character))
                prefabDictionary.Add(item.character, item.prefab);
        }

        if (!prefabDictionary.ContainsKey(AsciiCharacters.Empty))
        {
            prefabDictionary.Add(AsciiCharacters.Empty, BuildEmptyTilePrefab());
        }
    }

    private GameObject BuildEmptyTilePrefab()
    {
        GameObject emptyTile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        emptyTile.name = "GeneratedEmptyTile";

        Collider col = emptyTile.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        Renderer renderer = emptyTile.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = new Color(0.05f, 0.05f, 0.05f, 0.35f);
            renderer.sharedMaterial = mat;
        }

        emptyTile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        return emptyTile;
    }

    private void GenerateMap(AsciiCharacters[,] asciiCharacters)
    {
        ClearMap();
        if (asciiCharacters == null) return;

        int width = asciiCharacters.GetLength(0);
        int length = asciiCharacters.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                AsciiCharacters tile = asciiCharacters[x, z];

                if (!prefabDictionary.TryGetValue(tile, out GameObject prefab))
                    continue;

                var asciiChar = Instantiate(prefab, transform);
                asciiChar.transform.localPosition = new Vector3(x * tileSize, 0, -z * tileSize);
                asciiChar.transform.localScale = Vector3.one * tileSize;

                if (IsEnemyTile(tile))
                {
                    Renderer renderer = asciiChar.GetComponentInChildren<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.color = Color.red;
                    }
                }
            }
        }
    }

    private void ClearMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

}