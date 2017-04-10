using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField]
    private LevelData levelData;

    public Vector3 gridUnitSize = Vector3.one;   
    public Vector3 blockPlacementOffset;
    public GameObject blockMarkerPrefab;
    public GameObject blockTypelessPrefab;
    public GameObject[] blockTypePrefabs;

    new private Transform transform;
    private BlockMarker blockMarker;
    private Vector2 gridFarpoint;
    private GridPoint lastPoint = new GridPoint(invalidPoint.width, invalidPoint.height);
    private int[][,] blockTypes;
    private BlockGameObject[][,] blockGameObjects;

    public static readonly GridPoint invalidPoint = new GridPoint(-1, -1);

    void Awake()
    {
        levelData.grid = new Block[levelData.gridLayers][,];
        for (int i = 0; i < levelData.gridLayers; i++)
        {
            levelData.grid[i] = new Block[levelData.gridWidth, levelData.gridHeight];
        }
        gridFarpoint = new Vector2(levelData.gridWidth / 2f * gridUnitSize.x, levelData.gridHeight / 2f * gridUnitSize.z);
        transform = GetComponent<Transform>();
    }

    void Start()
    {
        // Grid limit debug
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(gridFarpoint.x, 0f, gridFarpoint.y);
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(-gridFarpoint.x, 0f, -gridFarpoint.y);
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(gridFarpoint.x, 0f, -gridFarpoint.y);
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(-gridFarpoint.x, 0f, gridFarpoint.y);
    }

    void Update()
    {
        switch (GameController.instance.mode)
        {
            case GameController.Mode.Build:
                BuildUpdate();
                break;

            case GameController.Mode.Play:
                PlayUpdate();
                break;

            default:
                break;
        }
    }

    void PlayUpdate()
    {
        if (blockMarker)
        {
            Destroy(blockMarker);
            blockMarker = null;
        }
    }

    void BuildUpdate()
    {
        if (!blockMarker)
        {
            blockMarker = Instantiate(blockMarkerPrefab, transform).GetComponent<BlockMarker>();
            blockMarker.name = blockMarkerPrefab.name;
        }

        RaycastHit hit;

        if (GameController.instance.GetPointerPos(out hit))
        {
            if (hit.point.x >= -gridFarpoint.x && hit.point.x <= gridFarpoint.x && hit.point.z >= -gridFarpoint.y && hit.point.z <= gridFarpoint.y)
            {
                GridPoint point = new GridPoint(
                    Mathf.RoundToInt(Mathf.Lerp(0, levelData.gridWidth - 1, Mathf.InverseLerp(-gridFarpoint.x, gridFarpoint.x, hit.point.x))), 
                    Mathf.RoundToInt(Mathf.Lerp(0, levelData.gridHeight - 1, Mathf.InverseLerp(-gridFarpoint.y, gridFarpoint.y, hit.point.z))));

                List<Block> bottomBlocks = new List<Block>();
                List<Block> leftBlocks = new List<Block>();
                List<Block> rightBlocks = new List<Block>();

                if (CheckSpaceInGrid(ref point, out bottomBlocks, out leftBlocks, out rightBlocks))
                {
                    Vector3 newPos = RoundToGrid(hit.point);
                    blockMarker.transform.position = new Vector3(
                        Mathf.Lerp(-gridFarpoint.x, gridFarpoint.x, Mathf.InverseLerp(0f, levelData.gridWidth - 1, point.width)), 
                        blockPlacementOffset.y + point.layer * gridUnitSize.y, 
                        Mathf.Lerp(-gridFarpoint.y, gridFarpoint.y, Mathf.InverseLerp(0f, levelData.gridHeight - 1, point.height)));
                    lastPoint = point;

                    if (!blockMarker.gameObject.activeInHierarchy)
                    {
                        blockMarker.gameObject.SetActive(true);
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject go = Instantiate(blockTypelessPrefab, blockMarker.transform.position, blockMarker.transform.rotation, transform);
                        go.name = "[" + blockTypelessPrefab.name + "]";
                        BlockGameObject blockGameObject = go.GetComponent<BlockGameObject>();
                        blockGameObject.Init(this, point);
                        Block block = new Block(point, bottomBlocks, leftBlocks, rightBlocks, blockGameObject);
                        for (int i = 0; i < leftBlocks.Count; i++)
                        {
                            leftBlocks[i].rightBlocks.Add(block);
                        }
                        for (int i = 0; i < rightBlocks.Count; i++)
                        {
                            rightBlocks[i].leftBlocks.Add(block);
                        }
                        levelData.grid[point.layer][point.width, point.height] = block;
                        //print("Block created at [" + point.width + "," + point.height + "] on layer [" + point.layer + "]");
                    }
                }
                else
                {
                    lastPoint = invalidPoint;

                    if (blockMarker.gameObject.activeInHierarchy)
                    {
                        blockMarker.gameObject.SetActive(false);
                    }
                }
            }
        }

        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            FileIO.BinarySerialize("level1.lvl", levelData);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            FileIO.BinaryDeserialize("level1.lvl");
        }*/
    }

    void InitPlay()
    {
        List<int> asd = new List<int>();
        for (int i = 0; i < levelData.grid.Length; i++)
        {
            for (int j = 0; j < levelData.grid[i].GetLength(0); j++)
            {
                for (int k = 0; k < levelData.grid[i].GetLength(1); k++)
                {
                    if (levelData.grid[i][j,k] != null)
                    {
                        //
                        //levelData.grid[i][j,k].
                        //
                    }
                }
            }
        }
    }

    bool CheckSpaceInGrid(ref GridPoint point, out List<Block> bottomBlocks, out List<Block> leftBlocks, out List<Block> rightBlocks)
    {
        bool freeSpace = true;
        bool xNegEdge = point.width > 0;
        bool xPosEdge = point.width < levelData.gridWidth - 1;
        bool yNegEdge = point.height > 0;
        bool yPosEdge = point.height < levelData.gridHeight - 1;

        bottomBlocks = new List<Block>();
        leftBlocks = new List<Block>();
        rightBlocks = new List<Block>();

        // Loop checking block occupation
        while (point.layer < levelData.gridLayers)
        {
            freeSpace = true;
            // Block on the same spot (point.width, point.height)
            if (levelData.grid[point.layer][point.width, point.height] != null && freeSpace)
            {
                bottomBlocks.Add(levelData.grid[point.layer][point.width, point.height]);
                freeSpace = false;
                point.layer++;
                continue;
            }
            else
            {
                // Block on the spot (point.width - 1, point.height)
                if (xNegEdge && freeSpace)
                {
                    if (levelData.grid[point.layer][point.width - 1, point.height] != null)
                    {
                        freeSpace = false;

                        // Block also on the spot (point.width + 1, point.height), supports elevated blocks -> proceed to next point.layer
                        if (xPosEdge)
                        {
                            if (levelData.grid[point.layer][point.width + 1, point.height] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { levelData.grid[point.layer][point.width - 1, point.height], levelData.grid[point.layer][point.width + 1, point.height] });
                                point.layer++;
                                for (int i = 0; i < bottomBlocks.Count; i++)
                                {
                                    if (bottomBlocks[i] == null)
                                        print(bottomBlocks.Count);
                                }
                                continue;
                            }
                        }
                        // Blocks also on the spots (point.width + 1, point.height - 1), (point.width + 1, point.height + 1), supports elevated blocks -> proceed to next point.layer
                        if (xPosEdge && yNegEdge && yPosEdge)
                        {
                            if (levelData.grid[point.layer][point.width + 1, point.height - 1] != null && levelData.grid[point.layer][point.width + 1, point.height + 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { levelData.grid[point.layer][point.width - 1, point.height], levelData.grid[point.layer][point.width + 1, point.height - 1], levelData.grid[point.layer][point.width + 1, point.height + 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width + 1, point.height)
                if (xPosEdge && freeSpace)
                {
                    if (levelData.grid[point.layer][point.width + 1, point.height] != null)
                    {
                        freeSpace = false;

                        // Blocks also on the spots (point.width + 1, point.height - 1), (point.width + 1, point.height + 1), supports elevated blocks -> proceed to next point.layer
                        if (xNegEdge && yNegEdge && yPosEdge)
                        {
                            if (levelData.grid[point.layer][point.width - 1, point.height - 1] != null && levelData.grid[point.layer][point.width - 1, point.height + 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { levelData.grid[point.layer][point.width + 1, point.height], levelData.grid[point.layer][point.width - 1, point.height - 1], levelData.grid[point.layer][point.width - 1, point.height + 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width, point.height - 1)
                if (yNegEdge && freeSpace)
                {
                    if (levelData.grid[point.layer][point.width, point.height - 1] != null)
                    {
                        freeSpace = false;

                        // Block also on the spot (point.width, point.height + 1), supports elevated blocks -> proceed to next point.layer
                        if (yPosEdge)
                        {
                            if (levelData.grid[point.layer][point.width, point.height + 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { levelData.grid[point.layer][point.width, point.height - 1], levelData.grid[point.layer][point.width, point.height + 1] });
                                point.layer++;
                                continue;
                            }
                        }
                        // Blocks also on the spots (point.width + 1, point.height + 1), (point.width - 1, point.height + 1), (point.width + 1, point.height - 1), supports elevated blocks -> proceed to next point.layer
                        if (yPosEdge && xNegEdge && xPosEdge)
                        {
                            if (levelData.grid[point.layer][point.width + 1, point.height + 1] != null && levelData.grid[point.layer][point.width - 1, point.height + 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { levelData.grid[point.layer][point.width, point.height - 1], levelData.grid[point.layer][point.width + 1, point.height + 1], levelData.grid[point.layer][point.width - 1, point.height + 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width, point.height + 1)
                if (yPosEdge && freeSpace)
                {
                    if (levelData.grid[point.layer][point.width, point.height + 1] != null)
                    {
                        freeSpace = false;

                        // Blocks also on the spots (point.width + 1, point.height + 1), (point.width - 1, point.height + 1), supports elevated blocks -> proceed to next point.layer
                        if (yNegEdge && xNegEdge && xPosEdge)
                        {
                            if (levelData.grid[point.layer][point.width + 1, point.height - 1] != null && levelData.grid[point.layer][point.width - 1, point.height - 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { levelData.grid[point.layer][point.width, point.height + 1], levelData.grid[point.layer][point.width + 1, point.height - 1], levelData.grid[point.layer][point.width - 1, point.height - 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width - 1, point.height - 1)
                if (xNegEdge && yNegEdge && freeSpace)
                {
                    if (levelData.grid[point.layer][point.width - 1, point.height - 1] != null)
                    {
                        freeSpace = false;

                        // Blocks also on the spots (point.width + 1, point.height + 1), (point.width - 1, point.height + 1), (point.width + 1, point.height - 1), supports elevated blocks -> proceed to next point.layer
                        if (yPosEdge && xPosEdge)
                        {
                            if (levelData.grid[point.layer][point.width + 1, point.height + 1] != null && levelData.grid[point.layer][point.width - 1, point.height + 1] != null && levelData.grid[point.layer][point.width + 1, point.height - 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { levelData.grid[point.layer][point.width - 1, point.height - 1], levelData.grid[point.layer][point.width + 1, point.height + 1], levelData.grid[point.layer][point.width - 1, point.height + 1], levelData.grid[point.layer][point.width + 1, point.height - 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width - 1, point.height + 1)
                if (xNegEdge && yPosEdge && freeSpace)
                {
                    if (levelData.grid[point.layer][point.width - 1, point.height + 1] != null)
                    {
                        freeSpace = false;
                    }
                }
                // Block on the spot (point.width + 1, point.height - 1)
                if (xPosEdge && yNegEdge && freeSpace)
                {
                    if (levelData.grid[point.layer][point.width + 1, point.height - 1] != null)
                    {
                        freeSpace = false;
                    }
                }
                // Block on the spot (point.width + 1, point.height + 1)
                if (xPosEdge && yPosEdge && freeSpace)
                {
                    if (levelData.grid[point.layer][point.width + 1, point.height + 1] != null)
                    {
                        freeSpace = false;
                    }
                }
                // Blocks on spots (point.width - 2, point.height +- 1) -> blocks possible selection from left
                if (yNegEdge && yPosEdge && point.width > 1)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        if (levelData.grid[point.layer][point.width - 2, point.height + i] != null)
                        {
                            leftBlocks.Add(levelData.grid[point.layer][point.width - 2, point.height + i]);
                        }
                    }
                }
                // Blocks on spots (point.width + 2, point.height +- 1) -> blocks possible selection from right
                if (yNegEdge && yPosEdge && point.width < levelData.gridWidth - 2)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        if (levelData.grid[point.layer][point.width + 2, point.height + i] != null)
                        {
                            rightBlocks.Add(levelData.grid[point.layer][point.width + 2, point.height + i]);
                        }
                    }
                }
            }
            break;
        }
        return freeSpace;
    }

    float RoundToGrid(float value, float distance)
    {
        return Mathf.RoundToInt(value / distance) * distance;
    }

    Vector3 RoundToGrid(Vector3 value)
    {
        return new Vector3(RoundToGrid(value.x, gridUnitSize.x), RoundToGrid(value.y, gridUnitSize.y), RoundToGrid(value.z, gridUnitSize.z));
    }

    public bool RemoveBlock(GridPoint point)
    {
        if (IsBlockFree(point))
        {
            levelData.grid[point.layer][point.width, point.height].Remove();
            levelData.grid[point.layer][point.width, point.height] = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsBlockFree(GridPoint point)
    {
        return levelData.grid[point.layer][point.width, point.height].IsFree();
    }
}