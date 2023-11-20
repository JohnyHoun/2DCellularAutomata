using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateMap_2 : MonoBehaviour  //Fluid Fill   <--
{
    [Header("Map Variables")]
    [SerializeField] Vector2Int mapSize;

    [SerializeField] float wallPercentage;

    [SerializeField] int limitToFall;
    [SerializeField] int limitToRaise;

    [Header("Generation Variables")]
    [SerializeField] int seed;
    [SerializeField] int generationsNumber;
    [SerializeField] bool randomizeGeneration;

    [Header("Prefabs")]
    [SerializeField] Tilemap groundTilemap;
    [SerializeField] Tilemap wallTilemap;
    [SerializeField] Tile groundTile;
    [SerializeField] RuleTile wallTile;

    [Header("Scene Components")]
    [SerializeField] GameObject pathGenerator;

    private bool[,] map;

    GameObject[,] instantiatedMap;

    private void Start()
    {
        int randSeed = Random.Range(0, 100000000);
        seed = randSeed;

        Random.InitState(seed);

        StartMap();

        for (int i = 0; i <= generationsNumber; i++)
            SecondMap();

        VerifyConection();

        //AddWallsToTHeEdges();

        BuildMap();

        StartCoroutine(TurnOnPath(0.5f));
    }

    private void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            DestroyMap();

            int randSeed = Random.Range(0, 100000000);
            seed = randSeed;

            Random.InitState(seed);

            StartMap();

            for (int i = 0; i <= generationsNumber; i++)
                SecondMap();

            VerifyConection();

            BuildMap();
        }*/
    }

    void StartMap()
    {
        map = new bool[mapSize.x, mapSize.y];

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (Random.value < wallPercentage)
                    map[i, j] = true;
                else
                    map[i, j] = false;

                /*int middleMap = map.GetLength(0) / 2;


                if (Mathf.Abs(i - j) < 2)
                {
                    map[i, j] = false;
                }

                if (i >= middleMap - 1 && i <= middleMap + 1)
                {
                    map[i, j] = false;
                }

                if (j >= middleMap - 1 && j <= middleMap + 1)
                {
                    map[i, j] = false;
                }*/
            }
        }
    }

    void BuildMap()
    {
        instantiatedMap = new GameObject[mapSize.x, mapSize.y];

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j])
                {
                    wallTilemap.SetTile(new Vector3Int(i, j), wallTile);
                }
                else
                {
                    groundTilemap.SetTile(new Vector3Int(i, j), groundTile);
                }
            }
        }
    }

    int NeighborWallNum(int x, int y)
    {
        int wallNum = 0;

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int xPos = x + i;
                int yPos = y + j;

                //if the bool is out of the map it doesn´t count
                if (i == 0 && j == 0 || xPos < 0 || yPos < 0 || xPos >= mapSize.x || yPos >= mapSize.y)
                    continue;

                if (map[xPos, yPos] == true)
                    wallNum++;
            }
        }

        return wallNum;
    }

    void DestroyMap()
    {
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    void SecondMap()
    {
        bool[,] nextMap = new bool[mapSize.x, mapSize.y];

        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                if (map[i, j] == true)
                {
                    if (NeighborWallNum(i, j) <= limitToFall)
                        nextMap[i, j] = false;
                    else
                        nextMap[i, j] = true;
                }
                else
                {
                    if (NeighborWallNum(i, j) >= limitToRaise)
                        nextMap[i, j] = true;
                    else
                        nextMap[i, j] = false;
                }
            }
        }

        map = nextMap;
    }

    //Turn true the boards of the map
    void AddWallsToTHeEdges()
    {
        for(int i = 0; i < mapSize.x; i++)
        {
            for(int j = 0; j < mapSize.y; j++)
            {
                if (i == 0 || j == 0 || i == mapSize.x - 1 || j == mapSize.y - 1)
                {
                    map[i, j] = true;
                }
            }
        }
    }

    //Fluid Fill
    void VerifyConection()
    {
        bool[,] verifyMap = new bool[mapSize.x, mapSize.y];

        Vector2Int initialPoint = FindInitialPoint();
       
        Queue<Vector2Int> elements = new Queue<Vector2Int>();

        elements.Enqueue(initialPoint);

        verifyMap[initialPoint.x, initialPoint.y] = true;

        while(elements.Count > 0)
        {
            Vector2Int actualElement = elements.Dequeue();

            verifyMap[actualElement.x, actualElement.y] = true;

            int x = actualElement.x;
            int y = actualElement.y;

            if(x > 0 && map[x - 1, y] == false && verifyMap[x - 1, y] == false)
            {
                verifyMap[x - 1, y] = true;
                elements.Enqueue(new Vector2Int(x - 1, y));
            }

            if (y > 0 && map[x, y - 1] == false && verifyMap[x, y - 1] == false)
            {
                verifyMap[x, y - 1] = true;
                elements.Enqueue(new Vector2Int(x, y - 1));
            }

            if (x < mapSize.x - 1 && map[x + 1, y] == false && verifyMap[x + 1, y] == false)
            {
                verifyMap[x + 1, y] = true;
                elements.Enqueue(new Vector2Int(x + 1, y));
            }
           

            if (y < mapSize.y - 1 && map[x, y + 1] == false && verifyMap[x, y + 1] == false)
            {
                verifyMap[x, y + 1] = true;
                elements.Enqueue(new Vector2Int(x, y + 1));
            }
        }

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (verifyMap[i, j] == false && map[i, j] == false)
                {
                    map[i, j] = true;
                }
            }
        }
    }

    Vector2Int FindInitialPoint()
    {
        Vector2Int middleMap = new Vector2Int(mapSize.x / 2, mapSize.y / 2);

        for (int i = middleMap.x; i < mapSize.x; i++)
        {
            for (int j = middleMap.y; j < mapSize.y; j++)
            {
                if (map[i, j] == false)
                {
                    return new Vector2Int(i, j);
                    
                }
            }
        }

        return new Vector2Int(0, 0);
    }

    IEnumerator TurnOnPath(float time)
    {
        yield return new WaitForSeconds(time);
        pathGenerator.SetActive(true);
    }
}
