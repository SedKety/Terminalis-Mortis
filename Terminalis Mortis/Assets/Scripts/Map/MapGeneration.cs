using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class MapGeneration : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private string[] map;

    [SerializeField] private float tileSize;

    [System.Serializable]
    public class CharacterPrefab
    {
        public char character;
        public GameObject prefab;
    }

    [Header("Prefabs")]
    [SerializeField] private List<CharacterPrefab> characterPrefabs;

    private Dictionary<char, GameObject> prefabDictionary;

    void Start()
    {
        // Build dictionary
        prefabDictionary = new Dictionary<char, GameObject>();

        foreach (var item in characterPrefabs)
        {
            prefabDictionary[item.character] = item.prefab;
        }

        GenerateMap();
    }

    void SetNewMap(string[] newMap)
    {
        map = newMap;
        GenerateMap();
    }

    void GenerateMap()
    {
        OffsetMap();

        for (int z = 0; z < map.Length; z++)
        {
            string row = map[z];

            for (int x = 0; x < row.Length; x++)
            {
                char tile = row[x];

                Vector3 position = new Vector3(x * tileSize, 0, -z * tileSize);
                Quaternion rotation = Quaternion.Euler(45, 0, 0);

                if (prefabDictionary.ContainsKey(tile))
                {
                    Instantiate( prefabDictionary[tile], position, rotation, transform);
                }
            }
        }
    }

    void OffsetMap()
    {
        int maxWidth = 0;

        foreach (string row in map)
        {
            if (row.Length > maxWidth)
                maxWidth = row.Length;
        }

        float offsetX = (maxWidth * tileSize) / 2f - tileSize / 2f;
        float offsetZ = (map.Length * tileSize) / 2f - tileSize / 2f;

        gameObject.transform.position = new Vector3 (offsetX, 0, -offsetZ);
    }
}