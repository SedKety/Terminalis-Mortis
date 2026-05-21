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

   

    [System.Serializable]
    public class CharacterPrefab
    {
        public AsciiCharacters character;
        public GameObject prefab;
    }
    
    void Awake()
    {
        BuildDictionary();
        AsciiCharacters[,] myMap = new AsciiCharacters[,] {
            {AsciiCharacters.Wall, AsciiCharacters.Wall, AsciiCharacters.Wall},
            {AsciiCharacters.Wall, AsciiCharacters.Malware, AsciiCharacters.Wall },
            {AsciiCharacters.Wall, AsciiCharacters.Player, AsciiCharacters.Wall }

        };
        SetMap(myMap);
    }


    public void SetMap(AsciiCharacters[,] newMap)
    {
        map = newMap;
        GenerateMap(map);
    }

    private void BuildDictionary()
    {
        prefabDictionary = new Dictionary<AsciiCharacters, GameObject>();

        foreach (var item in characterPrefabs)
        {
            if (!prefabDictionary.ContainsKey(item.character))
                prefabDictionary.Add(item.character, item.prefab);
        }
    }

    private void GenerateMap(AsciiCharacters[,] asciiCharacters)
    {
        ClearMap();
        if (asciiCharacters == null) return;

        int width = asciiCharacters.GetLength(0);
        int height = asciiCharacters.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                AsciiCharacters tile = asciiCharacters[x, z];
                if (tile == AsciiCharacters.Empty)
                    continue;
                if (!prefabDictionary.TryGetValue(tile, out GameObject prefab))
                    continue;

                Vector3 pos = new Vector3(x * tileSize, 0, -z * tileSize);
                Instantiate(prefab, pos, Quaternion.identity, transform);
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