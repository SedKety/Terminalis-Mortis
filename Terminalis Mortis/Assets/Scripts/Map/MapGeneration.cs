using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    [Header("Map Settings")]
    public string[] map;

    public float tileSize = 1f;

    [System.Serializable]
    public class CharacterPrefab
    {
        public char character;
        public GameObject prefab;
    }

    [Header("Prefabs")]
    public List<CharacterPrefab> characterPrefabs;

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
        for (int z = 0; z < map.Length; z++)
        {
            string row = map[z];

            for (int x = 0; x < row.Length; x++)
            {
                char tile = row[x];

                Vector3 position = new Vector3(x * tileSize, 0, -z * tileSize);

                if (prefabDictionary.ContainsKey(tile))
                {
                    Instantiate(
                        prefabDictionary[tile],
                        position,
                        Quaternion.identity,
                        transform
                    );
                }
            }
        }
    }
}