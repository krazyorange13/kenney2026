using System.Collections.Generic;
using UnityEngine;

public class GenerateCity : MonoBehaviour
{
    public List<GameObject> buildingPrefabs;
    public GameObject roadIntersectionPrefab;
    public GameObject roadStraightPrefab;

    public int cityWidth;
    public int cityHeight;

    public float tileWidth = 25.0f;
    public float tileHeight = 25.0f;

    void Start()
    {
        for (int i = 0; i < cityWidth; i++)
        {
            for (int j = 0; j < cityHeight; j++)
            {
                float x = i * tileWidth;
                float y = j * tileHeight;

                GameObject prefab;
                Vector3 position = new Vector3(x, 0, y);
                Quaternion rotation = Quaternion.identity;

                int _i = i - 1;
                int _j = j - 1;

                if (_i % 2 == 0)
                {
                    if (_j % 2 == 0)
                    {
                        prefab = roadIntersectionPrefab;
                    }
                    else
                    {
                        prefab = roadStraightPrefab;
                        rotation = Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f));
                    }
                }
                else
                {
                    if (_j % 2 == 0)
                    {
                        prefab = roadStraightPrefab;
                    }
                    else
                    {
                        prefab = buildingPrefabs[0];
                        float randomRotation = Random.Range(0, 4) * 90.0f;
                        rotation = Quaternion.Euler(new Vector3(0.0f, randomRotation, 0.0f));
                    }
                }

                GameObject newTile = Instantiate(prefab, position, rotation, transform);
                newTile.transform.localScale = new Vector3(tileWidth, tileHeight, tileWidth);
            }
        }
    }
}
