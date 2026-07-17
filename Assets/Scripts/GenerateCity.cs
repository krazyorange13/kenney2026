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
                Vector3 scale = new Vector3(tileWidth, tileHeight, tileWidth);

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
                        prefab = GetBuilding(i, j);
                        float randomRotation = UnityEngine.Random.Range(0, 4) * 90.0f;
                        position.y += 0.25f;
                        rotation = Quaternion.Euler(new Vector3(0.0f, randomRotation, 0.0f));
                    }
                }

                GameObject newTile = Instantiate(prefab, position, rotation, transform);
                newTile.transform.localScale = scale;
            }
        }
    }

    GameObject GetBuilding(int i, int j)
    {
        int height;
        height = Mathf.FloorToInt(GetBuildingHeight(i, j, 0.0f, 4.0f));
        if (UnityEngine.Random.value >= 0.25)
        {
            height += UnityEngine.Random.Range(-1, 2);
        }
        height = Mathf.Max(Mathf.Min(height, 3), 0);
        return buildingPrefabs[height];
    }

    float GetBuildingHeight(int i, int j, float mean = 0.0f, float stdev = 1.0f)
    {
        float s = 2.5f;
        float ci = cityWidth / 2.0f - 1;
        float cj = cityHeight / 2.0f - 1;
        float di = i - ci;
        float dj = j - cj;
        float exp = -((di * di) + (dj * dj)) / (2.0f * s * s);
        float gaus = Mathf.Exp(exp);
        float height = mean + stdev * gaus;
        return height;
    }
}
