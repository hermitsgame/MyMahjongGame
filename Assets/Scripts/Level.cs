using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public enum Mode { Disabled, Play, Build }

    public int gridWidth;
    public int gridHeight;
    public int gridLayers;
    public Vector3 gridUnitSize = Vector3.one;   
    public Vector3 blockPlacementOffset;
    public Mode mode = Mode.Disabled;
    public GameObject blockMarkerPrefab;
    public GameObject[] blockTypes;

    new private Transform transform;
    private BlockMarker blockMarker;
    //private GridPoint markerGridPoint;
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
        switch (mode)
        {
            case Mode.Build:
                Build();
                break;

            case Mode.Play:
                Play();
                break;

            default:
                break;
        }
    }

    void Play()
    {
        
    }

    void Build()
    {
        if (!blockMarker)
        {
            blockMarker = Instantiate(blockMarkerPrefab).GetComponent<BlockMarker>();
        }

        RaycastHit hit;

        if (GameController.instance.GetPointerPos(out hit))
        {
            if (hit.point.x >= -gridFarpoint.x && hit.point.x <= gridFarpoint.x && hit.point.z >= -gridFarpoint.y && hit.point.z <= gridFarpoint.y)
            {
                GridPoint point = new GridPoint(
                    Mathf.RoundToInt(Mathf.Lerp(0, gridWidth - 1, Mathf.InverseLerp(-gridFarpoint.x, gridFarpoint.x, hit.point.x))), 
                    Mathf.RoundToInt(Mathf.Lerp(0, gridHeight - 1, Mathf.InverseLerp(-gridFarpoint.y, gridFarpoint.y, hit.point.z))));

                List<Block> supports = new List<Block>();
                if (CheckSpace(ref point, out supports))
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
                        CreateBlock(point, supports);
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

    bool CheckSpace(ref GridPoint point, out List<Block> supports)
    {
        bool freeSpace = true;
        bool xNegEdge = point.width > 0;
        bool xPosEdge = point.width < gridWidth - 1;
        bool yNegEdge = point.height > 0;
        bool yPosEdge = point.height < gridHeight - 1;

        supports = new List<Block>();

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
                                point.layer++;
                                supports.AddRange(new Block[] { grid[point.layer][point.width - 1, point.height], grid[point.layer][point.width + 1, point.height] });
                                continue;
                            }
                        }
                        // Blocks also on the spots (point.width + 1, point.height - 1), (point.width + 1, point.height + 1), supports elevated blocks -> proceed to next point.layer
                        if (xPosEdge && yNegEdge && yPosEdge)
                        {
                            if (grid[point.layer][point.width + 1, point.height - 1] != null && grid[point.layer][point.width + 1, point.height + 1] != null)
                            {
                                point.layer++;
                                supports.AddRange(new Block[] { grid[point.layer][point.width - 1, point.height], grid[point.layer][point.width + 1, point.height - 1], grid[point.layer][point.width + 1, point.height + 1] });
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
                                point.layer++;
                                supports.AddRange(new Block[] { grid[point.layer][point.width + 1, point.height], grid[point.layer][point.width - 1, point.height - 1], grid[point.layer][point.width - 1, point.height + 1] });
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
                                point.layer++;
                                supports.AddRange(new Block[] { grid[point.layer][point.width, point.height - 1], grid[point.layer][point.width, point.height + 1] });
                                continue;
                            }
                        }
                        // Blocks also on the spots (point.width + 1, point.height + 1), (point.width - 1, point.height + 1), (point.width + 1, point.height - 1), supports elevated blocks -> proceed to next point.layer
                        if (yPosEdge && xNegEdge && xPosEdge)
                        {
                            if (grid[point.layer][point.width + 1, point.height + 1] != null && grid[point.layer][point.width - 1, point.height + 1] != null)
                            {
                                point.layer++;
                                supports.AddRange(new Block[] { grid[point.layer][point.width, point.height - 1], grid[point.layer][point.width + 1, point.height + 1], grid[point.layer][point.width - 1, point.height + 1] });
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
                                point.layer++;
                                supports.AddRange(new Block[] { grid[point.layer][point.width, point.height + 1], grid[point.layer][point.width + 1, point.height - 1], grid[point.layer][point.width - 1, point.height - 1] });
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
                                point.layer++;
                                supports.AddRange(new Block[] { grid[point.layer][point.width - 1, point.height - 1], grid[point.layer][point.width + 1, point.height + 1], grid[point.layer][point.width - 1, point.height + 1], grid[point.layer][point.width + 1, point.height - 1] });
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

    public void CreateBlock(GridPoint point, List<Block> supports)
    {
        if (blockMarker)
        {
            int type = Random.Range(0, blockTypes.Length - 1);
            GameObject go = Instantiate(blockTypes[type], blockMarker.transform.position, blockMarker.transform.rotation, transform);
            BlockGameObject blockGameObject = go.GetComponent<BlockGameObject>();
            blockGameObject.Init(point);
            grid[point.layer][point.width, point.height] = new Block(point, type, supports, blockGameObject);
            print("Block created at [" + point.width + "," + point.height + "] on layer [" + point.layer + "]");
        }
        else
        {
            Debug.LogWarning("Block marker is not present");
        }
    }

    public void RemoveBlock(GridPoint point)
    {
        if (grid[point.layer][point.width, point.height].IsFree())
        {
            grid[point.layer][point.width, point.height].gameObject.Remove();
            grid[point.layer][point.width, point.height] = null;
        }
    }
}