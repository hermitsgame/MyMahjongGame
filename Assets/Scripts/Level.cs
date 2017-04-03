using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;
    public int gridLayers;
    public Vector3 gridUnitSize = Vector3.one;   
    public Vector3 blockPlacementOffset;
    public GameObject blockMarkerPrefab;
    public GameObject[] blockTypes;

    new private Transform transform;
    private BlockMarker blockMarker;
    private Block[][,] grid; // maybe make an int obj index table to mark space and separate object table
    private Vector2 gridFarpoint;
    private GridPoint lastPoint = new GridPoint(invalidPoint.width, invalidPoint.height);

    private static readonly GridPoint invalidPoint = new GridPoint(-1, -1);

    void Awake()
    {
        grid = new Block[gridLayers][,];
        for (int i = 0; i < gridLayers; i++)
        {
            grid[i] = new Block[gridWidth, gridHeight];
        }
        gridFarpoint = new Vector2(gridWidth / 2f * gridUnitSize.x, gridHeight / 2f * gridUnitSize.z);
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
                Build();
                break;

            case GameController.Mode.Play:
                Play();
                break;

            default:
                break;
        }
    }

    void Play()
    {
        if (blockMarker)
        {
            Destroy(blockMarker);
            blockMarker = null;
        }
    }

    void Build()
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
                    Mathf.RoundToInt(Mathf.Lerp(0, gridWidth - 1, Mathf.InverseLerp(-gridFarpoint.x, gridFarpoint.x, hit.point.x))), 
                    Mathf.RoundToInt(Mathf.Lerp(0, gridHeight - 1, Mathf.InverseLerp(-gridFarpoint.y, gridFarpoint.y, hit.point.z))));

                List<Block> bottomBlocks = new List<Block>();
                List<Block> leftBlocks = new List<Block>();
                List<Block> rightBlocks = new List<Block>();
                if (CheckSpace(ref point, out bottomBlocks, out leftBlocks, out rightBlocks))
                {
                    Vector3 newPos = RoundToGrid(hit.point);
                    blockMarker.transform.position = new Vector3(
                        Mathf.Lerp(-gridFarpoint.x, gridFarpoint.x, Mathf.InverseLerp(0f, gridWidth - 1, point.width)), 
                        blockPlacementOffset.y + point.layer * gridUnitSize.y, 
                        Mathf.Lerp(-gridFarpoint.y, gridFarpoint.y, Mathf.InverseLerp(0f, gridHeight - 1, point.height)));
                    lastPoint = point;

                    if (!blockMarker.gameObject.activeInHierarchy)
                    {
                        blockMarker.gameObject.SetActive(true);
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        CreateBlock(point, bottomBlocks, leftBlocks, rightBlocks);
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
    }

    bool CheckSpace(ref GridPoint point, out List<Block> bottomBlocks, out List<Block> leftBlocks, out List<Block> rightBlocks)
    {
        bool freeSpace = true;
        bool xNegEdge = point.width > 0;
        bool xPosEdge = point.width < gridWidth - 1;
        bool yNegEdge = point.height > 0;
        bool yPosEdge = point.height < gridHeight - 1;

        bottomBlocks = new List<Block>();
        leftBlocks = new List<Block>();
        rightBlocks = new List<Block>();

        // Loop checking block occupation
        while (point.layer < gridLayers)
        {
            freeSpace = true;
            // Block on the same spot (point.width, point.height)
            if (grid[point.layer][point.width, point.height] != null && freeSpace)
            {
                freeSpace = false;
                point.layer++;
                continue;
            }
            else
            {
                // Block on the spot (point.width - 1, point.height)
                if (xNegEdge && freeSpace)
                {
                    if (grid[point.layer][point.width - 1, point.height] != null)
                    {
                        freeSpace = false;

                        // Block also on the spot (point.width + 1, point.height), supports elevated blocks -> proceed to next point.layer
                        if (xPosEdge)
                        {
                            if (grid[point.layer][point.width + 1, point.height] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { grid[point.layer][point.width - 1, point.height], grid[point.layer][point.width + 1, point.height] });
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
                            if (grid[point.layer][point.width + 1, point.height - 1] != null && grid[point.layer][point.width + 1, point.height + 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { grid[point.layer][point.width - 1, point.height], grid[point.layer][point.width + 1, point.height - 1], grid[point.layer][point.width + 1, point.height + 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width + 1, point.height)
                if (xPosEdge && freeSpace)
                {
                    if (grid[point.layer][point.width + 1, point.height] != null)
                    {
                        freeSpace = false;

                        // Blocks also on the spots (point.width + 1, point.height - 1), (point.width + 1, point.height + 1), supports elevated blocks -> proceed to next point.layer
                        if (xNegEdge && yNegEdge && yPosEdge)
                        {
                            if (grid[point.layer][point.width - 1, point.height - 1] != null && grid[point.layer][point.width - 1, point.height + 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { grid[point.layer][point.width + 1, point.height], grid[point.layer][point.width - 1, point.height - 1], grid[point.layer][point.width - 1, point.height + 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width, point.height - 1)
                if (yNegEdge && freeSpace)
                {
                    if (grid[point.layer][point.width, point.height - 1] != null)
                    {
                        freeSpace = false;

                        // Block also on the spot (point.width, point.height + 1), supports elevated blocks -> proceed to next point.layer
                        if (yPosEdge)
                        {
                            if (grid[point.layer][point.width, point.height + 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { grid[point.layer][point.width, point.height - 1], grid[point.layer][point.width, point.height + 1] });
                                point.layer++;
                                continue;
                            }
                        }
                        // Blocks also on the spots (point.width + 1, point.height + 1), (point.width - 1, point.height + 1), (point.width + 1, point.height - 1), supports elevated blocks -> proceed to next point.layer
                        if (yPosEdge && xNegEdge && xPosEdge)
                        {
                            if (grid[point.layer][point.width + 1, point.height + 1] != null && grid[point.layer][point.width - 1, point.height + 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { grid[point.layer][point.width, point.height - 1], grid[point.layer][point.width + 1, point.height + 1], grid[point.layer][point.width - 1, point.height + 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width, point.height + 1)
                if (yPosEdge && freeSpace)
                {
                    if (grid[point.layer][point.width, point.height + 1] != null)
                    {
                        freeSpace = false;

                        // Blocks also on the spots (point.width + 1, point.height + 1), (point.width - 1, point.height + 1), supports elevated blocks -> proceed to next point.layer
                        if (yNegEdge && xNegEdge && xPosEdge)
                        {
                            if (grid[point.layer][point.width + 1, point.height - 1] != null && grid[point.layer][point.width - 1, point.height - 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { grid[point.layer][point.width, point.height + 1], grid[point.layer][point.width + 1, point.height - 1], grid[point.layer][point.width - 1, point.height - 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width - 1, point.height - 1)
                if (xNegEdge && yNegEdge && freeSpace)
                {
                    if (grid[point.layer][point.width - 1, point.height - 1] != null)
                    {
                        freeSpace = false;

                        // Blocks also on the spots (point.width + 1, point.height + 1), (point.width - 1, point.height + 1), (point.width + 1, point.height - 1), supports elevated blocks -> proceed to next point.layer
                        if (yPosEdge && xPosEdge)
                        {
                            if (grid[point.layer][point.width + 1, point.height + 1] != null && grid[point.layer][point.width - 1, point.height + 1] != null && grid[point.layer][point.width + 1, point.height - 1] != null)
                            {
                                bottomBlocks.AddRange(new Block[] { grid[point.layer][point.width - 1, point.height - 1], grid[point.layer][point.width + 1, point.height + 1], grid[point.layer][point.width - 1, point.height + 1], grid[point.layer][point.width + 1, point.height - 1] });
                                point.layer++;
                                continue;
                            }
                        }
                    }
                }
                // Block on the spot (point.width - 1, point.height + 1)
                if (xNegEdge && yPosEdge && freeSpace)
                {
                    if (grid[point.layer][point.width - 1, point.height + 1] != null)
                    {
                        freeSpace = false;
                    }
                }
                // Block on the spot (point.width + 1, point.height - 1)
                if (xPosEdge && yNegEdge && freeSpace)
                {
                    if (grid[point.layer][point.width + 1, point.height - 1] != null)
                    {
                        freeSpace = false;
                    }
                }
                // Block on the spot (point.width + 1, point.height + 1)
                if (xPosEdge && yPosEdge && freeSpace)
                {
                    if (grid[point.layer][point.width + 1, point.height + 1] != null)
                    {
                        freeSpace = false;
                    }
                }
                // Blocks on spots (point.width - 2, point.height +- 1) -> blocks possible selection from left
                if (yNegEdge && yPosEdge && point.width > 1)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        if (grid[point.layer][point.width - 2, point.height + i] != null)
                        {
                            leftBlocks.Add(grid[point.layer][point.width - 2, point.height + i]);
                        }
                    }
                }
                // Blocks on spots (point.width + 2, point.height +- 1) -> blocks possible selection from right
                if (yNegEdge && yPosEdge && point.width < gridWidth - 2)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        if (grid[point.layer][point.width + 2, point.height + i] != null)
                        {
                            rightBlocks.Add(grid[point.layer][point.width + 2, point.height + i]);
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

    public void CreateBlock(GridPoint point, List<Block> bottomBlocks, List<Block> leftBlocks, List<Block> rightBlocks)
    {
        if (blockMarker)
        {
            int type = Random.Range(0, blockTypes.Length - 1);
            GameObject go = Instantiate(blockTypes[type], blockMarker.transform.position, blockMarker.transform.rotation, transform);
            go.name = /*blockTypes[type].name +*/ "[" + type +"]";
            BlockGameObject blockGameObject = go.GetComponent<BlockGameObject>();
            blockGameObject.Init(this, point, type);
            Block block = new Block(point, type, bottomBlocks, leftBlocks, rightBlocks, blockGameObject);
            for (int i = 0; i < leftBlocks.Count; i++)
            {
                leftBlocks[i].rightBlocks.Add(block);
            }
            for (int i = 0; i < rightBlocks.Count; i++)
            {
                rightBlocks[i].leftBlocks.Add(block);
            }
            grid[point.layer][point.width, point.height] = block;
            print("Block created at [" + point.width + "," + point.height + "] on layer [" + point.layer + "]");
        }
        else
        {
            Debug.LogWarning("Block marker is not present");
        }
    }

    public bool RemoveBlock(GridPoint point)
    {
        if (IsBlockFree(point))
        {
            grid[point.layer][point.width, point.height].Remove();
            grid[point.layer][point.width, point.height] = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsBlockFree(GridPoint point)
    {
        return grid[point.layer][point.width, point.height].IsFree();
    }
}