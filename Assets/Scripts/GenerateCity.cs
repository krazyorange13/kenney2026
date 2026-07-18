using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GenerateCity : MonoBehaviour
{
    public List<GameObject> buildingPrefabs;
    public GameObject buildingFillPrefab;
    public GameObject roadIntersectionPrefab;
    public GameObject roadStraightPrefab;
    public GameObject roadStraightSinglePrefab;
    public List<GameObject> carPrefabs;
    public List<GameObject> miscPrefabs;

    public float spread = 1.0f;
    public float sparsity = 0.1f;
    public float randomness = 0.25f;
    public float carDensity = 0.5f;
    public float carScale = 4.0f;
    public float carScaleDeviation = 1.0f;
    public int minMiscCount = 1;
    public int maxMiscCount = 4;
    public float miscScale = 25.0f;
    public float miscScaleDeviation = 5.0f;

    public int cityWidth;
    public int cityHeight;

    public float tileWidth = 25.0f;
    public float tileHeight = 25.0f;

    private GameObject[,] tiles;

    void Start()
    {
        tiles = new GameObject[cityWidth, cityHeight];

        Populate();
        Decimate();
    }

    void Populate()
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
                        PopulateCars(position);
                        PopulateMisc(position);
                    }
                    else
                    {
                        prefab = roadStraightPrefab;
                        rotation = Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f));
                        PopulateCars(position);
                        PopulateMisc(position);
                    }
                }
                else
                {
                    if (_j % 2 == 0)
                    {
                        prefab = roadStraightPrefab;
                        PopulateCars(position);
                        PopulateMisc(position);
                    }
                    else
                    {
                        GameObject fillPrefab = Instantiate(buildingFillPrefab, position, rotation, transform);
                        fillPrefab.transform.localScale = scale;

                        prefab = GetBuilding(i, j);
                        float randomRotation = UnityEngine.Random.Range(0, 4) * 90.0f;
                        position.y += 0.25f;
                        rotation = Quaternion.Euler(new Vector3(0.0f, randomRotation, 0.0f));
                    }
                }

                GameObject newTile = Instantiate(prefab, position, rotation, transform);
                newTile.transform.localScale = scale;

                tiles[i, j] = newTile;
            }
        }
    }

    void Decimate()
    {
        for (int i = 0; i < cityWidth; i++)
        {
            for (int j = 0; j < cityHeight; j++)
            {
                if (!(i % 2 == 0 && j % 2 == 0))
                {
                    continue;
                }

                if (UnityEngine.Random.value <= sparsity)
                {
                    Destroy(tiles[i, j]);
                    // ReplaceRoad(i - 1, j);
                    // ReplaceRoad(i + 1, j);
                    // ReplaceRoad(i, j - 1);
                    // ReplaceRoad(i, j + 1);
                }
            }
        }
    }

    void PopulateCars(Vector3 position)
    {
        if (UnityEngine.Random.value <= carDensity)
        {
            int carI = UnityEngine.Random.Range(0, carPrefabs.Count);
            GameObject carPrefab = carPrefabs[carI];
            position += new Vector3(UnityEngine.Random.Range(-tileWidth / 2.0f, tileWidth / 2.0f), 0.0f, UnityEngine.Random.Range(-tileHeight / 2.0f, tileHeight / 2.0f));
            Quaternion rotation = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0, 360), 0.0f);
            GameObject car = Instantiate(carPrefab, position, rotation, transform);
            car.transform.localScale = Vector3.one * (carScale + UnityEngine.Random.Range(-carScaleDeviation, carScaleDeviation));
        }
    }

    void PopulateMisc(Vector3 position)
    {
        int n = UnityEngine.Random.Range(minMiscCount, maxMiscCount + 1);
        for (int i = 0; i < n; i++)
        {
            int miscI = UnityEngine.Random.Range(0, miscPrefabs.Count);
            GameObject miscPrefab = miscPrefabs[miscI];
            position += new Vector3(UnityEngine.Random.Range(-tileWidth / 2.0f, tileWidth / 2.0f), 0.0f, UnityEngine.Random.Range(-tileHeight / 2.0f, tileHeight / 2.0f));
            Quaternion rotation = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0, 360), 0.0f);
            GameObject car = Instantiate(miscPrefab, position, rotation, transform);
            car.transform.localScale = Vector3.one * (miscScale + UnityEngine.Random.Range(-miscScaleDeviation, miscScaleDeviation));
        }
    }

    void ReplaceRoad(int i, int j)
    {
        if (i < 0 || j < 0 || i >= cityWidth || j >= cityHeight)
        {
            return;
        }

        Vector3 position = tiles[i, j].transform.position;
        Quaternion rotation = Quaternion.identity;
        Vector3 scale = tiles[i, j].transform.localScale;
        tiles[i, j] = Instantiate(roadStraightSinglePrefab, position, rotation, transform);
        tiles[i, j].transform.localScale = scale;
    }

    GameObject GetBuilding(int i, int j)
    {
        int height;
        height = Mathf.FloorToInt(GetBuildingHeight(i, j, 0.0f, (float)buildingPrefabs.Count));
        if (UnityEngine.Random.value >= randomness)
        {
            height += UnityEngine.Random.Range(-1, 2);
        }
        height = Mathf.Max(Mathf.Min(height, buildingPrefabs.Count - 1), 0);
        return buildingPrefabs[height];
    }

    float GetBuildingHeight(int i, int j, float mean = 0.0f, float stdev = 1.0f)
    {
        float s = spread * cityWidth * 15f / 51f;
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
