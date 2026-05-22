using System;
using System.Collections.Generic;
using UnityEngine;
using TerminalisMortis.Events;

/// <summary>
/// Generates a dungeon map from ASCII character data and manages player/enemy interactions.
/// TODO: Split into separate MapGenerator, EnemyController, CombatController, and FileNavigator classes for better separation of concerns and testability.
/// </summary>
/// <remarks>Currently a monolith class, this is to be split up when our prototype looks good enough to us.</remarks>
public class GridGenerator : MonoBehaviour
{
    /// <summary>
    /// Gets the active <see cref="GridGenerator"/> instance.
    /// </summary>
    public static GridGenerator Instance { get; private set; }

    /// <summary>
    /// World-space size multiplier applied to each generated tile.
    /// </summary>
    [Header("Settings")]
    [SerializeField] private float tileSize = 1f;

    /// <summary>
    /// Configured prefab mappings for ASCII characters.
    /// </summary>
    [Header("Prefabs")]
    [SerializeField] private List<CharacterPrefab> characterPrefabs;

    /// <summary>
    /// Runtime lookup from map character to prefab.
    /// </summary>
    private Dictionary<AsciiCharacters, GameObject> prefabDictionary;

    /// <summary>
    /// Active dungeon map.
    /// </summary>
    private AsciiCharacters[,] map;
    /// <summary>
    /// Current player position in map coordinates.
    /// </summary>
    private Vector2Int _playerPosition;
    /// <summary>
    /// Controller that tracks and updates enemy entities.
    /// </summary>
    private DungeonEnemyController _enemyController;
    /// <summary>
    /// Shared enemy data source.
    /// </summary>
    [SerializeField] private EnemyManager enemyManager;
    /// <summary>
    /// Controller that resolves combat interactions.
    /// </summary>
    private DungeonCombatController _combatController;
    /// <summary>
    /// Turn sequencer used for advancing game turns.
    /// </summary>
    private TurnManager _turnManager;
    /// <summary>
    /// Navigator for dungeon file progression.
    /// </summary>
    private DungeonFileNavigator _fileNavigator;
    /// <summary>
    /// Last generated enemy turn summary message.
    /// </summary>
    private string _lastEnemyTurnSummary;
    /// <summary>
    /// Counter used to determine enemy movement cadence.
    /// </summary>
    private int _enemyTurnCounter;
    /// <summary>
    /// Enemy tile cleared in the previous player action.
    /// </summary>
    private Vector2Int? _recentlyClearedEnemyTile;

    /// <summary>
    /// Starting player health value for a new map.
    /// </summary>
    [Header("Combat")]
    [SerializeField] private int startingPlayerHealth = 10;
    /// <summary>
    /// Current player health.
    /// </summary>
    private int _playerHealth;

    /// <summary>
    /// Root path used to locate dungeon files.
    /// </summary>
    [Header("Dungeon Files")]
    [SerializeField] private string dungeonRootPath = "users/dungeons";
    /// <summary>
    /// Popup shown when a BAT file is available/opened.
    /// </summary>
    [SerializeField] private GameObject batFilePopup;

   

    [System.Serializable]
    /// <summary>
    /// Defines a mapping between an ASCII map character and its prefab.
    /// </summary>
    public class CharacterPrefab
    {
        /// <summary>
        /// The ASCII character key.
        /// </summary>
        public AsciiCharacters character;
        /// <summary>
        /// Prefab to instantiate for the configured character.
        /// </summary>
        public GameObject prefab;
    }

    /// <summary>
    /// Initializes singleton state and prepares the first room.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GridGenerator detected. Destroying duplicate instance on " + gameObject.name + ".");
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);

            return;
        }

        Instance = this;

        BuildDictionary();
        _playerHealth = startingPlayerHealth;
        _fileNavigator = new DungeonFileNavigator(dungeonRootPath);

        if (enemyManager == null)
            enemyManager = EnemyManager.Instance;

        if (map == null)
            SetMap(CreateRoomForCurrentFile());
    }

    /// <summary>
    /// Creates a room layout for the currently selected dungeon file.
    /// </summary>
    /// <returns>The generated room map.</returns>
    private AsciiCharacters[,] CreateRoomForCurrentFile()
    {
        if (_fileNavigator == null)
            _fileNavigator = new DungeonFileNavigator(dungeonRootPath);

        return CreateFirstRoom(9, 9, _fileNavigator.CurrentFileIndex);
    }

    /// <summary>
    /// Builds the initial room with walls, player spawn, and enemy spawns.
    /// </summary>
    /// <param name="width">Requested room width.</param>
    /// <param name="height">Requested room height.</param>
    /// <param name="levelIndex">Current dungeon level index.</param>
    /// <returns>A populated room map.</returns>
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


    /// <summary>
    /// Sets a new active map and reinitializes turn/combat state.
    /// </summary>
    /// <param name="newMap">Map to apply.</param>
    public void SetMap(AsciiCharacters[,] newMap)
    {
        if (_turnManager != null)
            _turnManager.OnTurnAdvanced -= HandleEnemyTurn;

        if (prefabDictionary == null)
            BuildDictionary();

        map = CloneMap(newMap);
        _enemyTurnCounter = 0;
        if (!TryFindPlayer(out _playerPosition))
        {
            _playerPosition = new Vector2Int(map.GetLength(0) / 2, map.GetLength(1) / 2);
            map[_playerPosition.x, _playerPosition.y] = AsciiCharacters.Player;
        }

        _enemyController = new DungeonEnemyController();
        _enemyController.InitializeFromMap(map, enemyManager, IsEnemyTile);
        _combatController = new DungeonCombatController();
        _turnManager = new TurnManager();
        _turnManager.OnTurnAdvanced += HandleEnemyTurn;
        _lastEnemyTurnSummary = string.Empty;
        _recentlyClearedEnemyTile = null;

        RefreshView();
    }

    /// <summary>
    /// Attempts to move the player one tile in the given direction.
    /// </summary>
    /// <param name="direction">Movement direction.</param>
    /// <param name="result">Result message for the attempted move.</param>
    /// <returns><see langword="true"/> if movement succeeded; otherwise <see langword="false"/>.</returns>
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

    /// <summary>
    /// Performs a directional melee attack against an adjacent tile.
    /// </summary>
    /// <param name="targetOrDirection">Direction text for the attack.</param>
    /// <returns>Combat result message.</returns>
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

        if (TMEventManager.Instance != null)
        {
            TMEventManager.Instance.RaiseEnemyHit(target);
        }

        bool enemyKilled = _combatController.DealDamageToEnemy(target, 1, _enemyController);
        if (enemyKilled)
        {
            map[target.x, target.y] = AsciiCharacters.Empty;
            _recentlyClearedEnemyTile = target;
        }
        else
        {
            _recentlyClearedEnemyTile = null;

            if (_enemyController.HasThorns(target))
            {
                int thornsDamage = _enemyController.GetThornsDamage(target);
                if (thornsDamage > 0)
                {
                    _combatController.DealDamageToPlayer(ref _playerHealth, thornsDamage);
                }
            }
        }

        AdvanceTurn();
        RefreshView();

        return string.IsNullOrEmpty(_lastEnemyTurnSummary)
            ? "Hit confirmed."
            : "Hit confirmed. " + _lastEnemyTurnSummary;
    }

    /// <summary>
    /// Gets the current dungeon file path.
    /// </summary>
    /// <returns>Current path string.</returns>
    public string GetCurrentPath()
    {
        if (_fileNavigator == null)
        {
            _fileNavigator = new DungeonFileNavigator(dungeonRootPath);
        }

        return _fileNavigator.GetCurrentPath();
    }

    /// <summary>
    /// Builds a compact status summary for path and combat progression.
    /// </summary>
    /// <returns>Status overview string.</returns>
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

    /// <summary>
    /// Gets tutorial text describing available commands and flow.
    /// </summary>
    /// <returns>Quick tutorial message.</returns>
    public string GetQuickTutorial()
    {
        string enemyLegend = GetConfiguredEnemyLegend();
        return "[SYS] This is turn-based: each move or attack spends one turn.\n"
            + "[SYS] move <north|south|east|west> moves you one tile in that direction.\n"
            + "[SYS] attack <direction> hits one adjacent tile.\n"
            + "[SYS] Clear all enemies to proceed, then run locate to continue.\n"
            + "[SYS] Enemy types in this build: " + enemyLegend + "\n"
            + "[SYS] Use tutorial anytime to re-open training pages.";
    }

    /// <summary>
    /// Builds the configured enemy legend from the enemy manager.
    /// </summary>
    /// <returns>Enemy legend text.</returns>
    public string GetConfiguredEnemyLegend()
    {
        EnemyManager manager = enemyManager != null ? enemyManager : EnemyManager.Instance;
        if (manager == null)
        {
            return "M, V, T, R";
        }

        List<string> entries = new List<string>();
        foreach (AsciiCharacters enemyType in manager.GetConfiguredEnemyTypes())
        {
            entries.Add(enemyType.ToString() + " (" + (char)enemyType + ")");
        }

        if (entries.Count == 0)
        {
            return "M, V, T, R";
        }

        return string.Join(", ", entries);
    }

    /// <summary>
    /// Advances to the next file encounter when all enemies are defeated.
    /// </summary>
    /// <returns>Operation result message.</returns>
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

    /// <summary>
    /// Advances the turn and checks for encounter completion.
    /// </summary>
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

    /// <summary>
    /// Handles turn-advanced events and records enemy action summaries.
    /// </summary>
    /// <param name="turnNumber">Current turn number.</param>
    private void HandleEnemyTurn(int turnNumber)
    {
        _lastEnemyTurnSummary = ProcessEnemyTurn();
    }

    /// <summary>
    /// Creates a deep copy of the provided map.
    /// </summary>
    /// <param name="source">Source map.</param>
    /// <returns>Cloned map.</returns>
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

    /// <summary>
    /// Searches the map for the player tile.
    /// </summary>
    /// <param name="position">Resolved player position when found.</param>
    /// <returns><see langword="true"/> if the player tile is found; otherwise <see langword="false"/>.</returns>
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

    /// <summary>
    /// Counts currently alive enemies.
    /// </summary>
    /// <returns>Alive enemy count.</returns>
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

    /// <summary>
    /// Determines whether a map coordinate is inside map bounds.
    /// </summary>
    /// <param name="pos">Map coordinate to test.</param>
    /// <returns><see langword="true"/> if the position is inside bounds; otherwise <see langword="false"/>.</returns>
    private bool IsInside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < map.GetLength(0) && pos.y >= 0 && pos.y < map.GetLength(1);
    }

    /// <summary>
    /// Determines whether a tile value represents an enemy.
    /// </summary>
    /// <param name="tile">Tile value to test.</param>
    /// <returns><see langword="true"/> if the tile is an enemy type; otherwise <see langword="false"/>.</returns>
    private bool IsEnemyTile(AsciiCharacters tile)
    {
        return tile == AsciiCharacters.Malware
            || tile == AsciiCharacters.Virus
            || tile == AsciiCharacters.Trojan
            || tile == AsciiCharacters.Ransomware;
    }

    /// <summary>
    /// Converts a text direction token into a movement vector.
    /// </summary>
    /// <param name="direction">Direction string.</param>
    /// <returns>Direction vector or <see cref="Vector2Int.zero"/> when invalid.</returns>
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

    /// <summary>
    /// Processes one enemy turn, including movement and attacks.
    /// </summary>
    /// <returns>Enemy action summary text.</returns>
    private string ProcessEnemyTurn()
    {
        if (_enemyController == null || _combatController == null)
        {
            return string.Empty;
        }

        Vector2Int? protectedTile = _recentlyClearedEnemyTile;
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
                if (_enemyController.CanAttackOnTouch(enemyPos))
                {
                    int touchDamage = _enemyController.GetEnemyDamage(enemyPos);
                    _combatController.DealDamageToPlayer(ref _playerHealth, touchDamage);
                    if (TMEventManager.Instance != null)
                    {
                        TMEventManager.Instance.RaisePlayerHit(enemyPos);
                    }

                    attacks++;
                }

                continue;
            }

            if (!canMoveThisTurn || !_enemyController.CanMoveThisTurn(enemyPos))
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
                if (_enemyController.CanAttackOnTouch(enemyPos))
                {
                    int touchDamage = _enemyController.GetEnemyDamage(enemyPos);
                    _combatController.DealDamageToPlayer(ref _playerHealth, touchDamage);
                    if (TMEventManager.Instance != null)
                    {
                        TMEventManager.Instance.RaisePlayerHit(enemyPos);
                    }

                    attacks++;
                }

                continue;
            }

            if (map[next.x, next.y] == AsciiCharacters.Empty
                && (!protectedTile.HasValue || next != protectedTile.Value))
            {
                map[next.x, next.y] = map[enemyPos.x, enemyPos.y];
                map[enemyPos.x, enemyPos.y] = AsciiCharacters.Empty;
                _enemyController.MoveEnemy(enemyPos, next);
                moves++;
            }
        }

        _recentlyClearedEnemyTile = null;

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

    /// <summary>
    /// Computes the next step from one tile toward another.
    /// </summary>
    /// <param name="from">Starting coordinate.</param>
    /// <param name="to">Target coordinate.</param>
    /// <returns>Next coordinate to move to, or <paramref name="from"/> if blocked.</returns>
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

    /// <summary>
    /// Gets the current enemy attack damage scaling value.
    /// </summary>
    /// <returns>Enemy attack damage value.</returns>
    private int GetEnemyAttackDamage()
    {
        if (_fileNavigator == null)
        {
            return 1;
        }

        return 1 + Mathf.Clamp(_fileNavigator.CurrentFileIndex, 0, 4);
    }

    /// <summary>
    /// Regenerates the visual map representation.
    /// </summary>
    private void RefreshView()
    {
        GenerateMap(map);
    }

    /// <summary>
    /// Determines whether all tracked enemies are defeated.
    /// </summary>
    /// <returns><see langword="true"/> if no alive enemies remain; otherwise <see langword="false"/>.</returns>
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

    /// <summary>
    /// Builds the runtime prefab dictionary from configured mappings.
    /// </summary>
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

    /// <summary>
    /// Creates a generated prefab for empty floor tiles.
    /// </summary>
    /// <returns>Generated empty tile prefab.</returns>
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

    /// <summary>
    /// Rebuilds map visuals from ASCII tile data.
    /// </summary>
    /// <param name="asciiCharacters">Source tile grid.</param>
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

    /// <summary>
    /// Clears all currently generated tile objects.
    /// </summary>
    private void ClearMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.SetActive(false);

            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

}