using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;
    public int gridLayers;
    public Vector3 gridUnitSize = Vector3.one;
    public GameObject blockMarkerPrefab;
    public GameObject blockPlacedPrefab;
    public Vector3 blockPlacementOffset;

    new private Transform transform;
    private BlockMarker blockMarker;
    private GridPoint markerGridPoint;
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
        BlockPlacement();
    }

    void BlockPlacement()
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
                int layer = 0;

                int x = Mathf.RoundToInt(Mathf.Lerp(0, gridWidth - 1, Mathf.InverseLerp(-gridFarpoint.x, gridFarpoint.x, hit.point.x)));
                int y = Mathf.RoundToInt(Mathf.Lerp(0, gridHeight - 1, Mathf.InverseLerp(-gridFarpoint.y, gridFarpoint.y, hit.point.z)));              

                bool freeSpace = true;
                bool xNegEdge = x > 0;
                bool xPosEdge = x < gridWidth - 1;
                bool yNegEdge = y > 0;
                bool yPosEdge = y < gridHeight - 1;

                List<Block> supports = new List<Block>();

                // Loop checking block occupation for placing new blocks
                while (layer < gridLayers)
                {                    
                    freeSpace = true;
                    // Block on the same spot (x, y)
                    if (grid[layer][x, y] != null && freeSpace)
                    {
                        freeSpace = false;
                        layer++;
                        continue;
                    }
                    else
                    {
                        // Block on the spot (x - 1, y)
                        if (xNegEdge && freeSpace)
                        {
                            if (grid[layer][x - 1, y] != null)
                            {
                                freeSpace = false;

                                // Block also on the spot (x + 1, y), supports elevated blocks -> proceed to next layer
                                if (xPosEdge)
                                {
                                    if (grid[layer][x + 1, y] != null)
                                    {
                                        layer++;
                                        supports.AddRange(new Block[] { grid[layer][x - 1, y], grid[layer][x + 1, y] });
                                        continue;
                                    }
                                }
                                // Blocks also on the spots (x + 1, y - 1), (x + 1, y + 1), supports elevated blocks -> proceed to next layer
                                if (xPosEdge && yNegEdge && yPosEdge)
                                {
                                    if (grid[layer][x + 1, y - 1] != null && grid[layer][x + 1, y + 1] != null)
                                    {
                                        layer++;
                                        supports.AddRange(new Block[] { grid[layer][x - 1, y], grid[layer][x + 1, y - 1], grid[layer][x + 1, y + 1] });
                                        continue;
                                    }
                                }
                            }
                        }
                        // Block on the spot (x + 1, y)
                        if (xPosEdge && freeSpace)
                        {
                            if (grid[layer][x + 1, y] != null)
                            {
                                freeSpace = false;

                                // Blocks also on the spots (x + 1, y - 1), (x + 1, y + 1), supports elevated blocks -> proceed to next layer
                                if (xNegEdge && yNegEdge && yPosEdge)
                                {
                                    if (grid[layer][x - 1, y - 1] != null && grid[layer][x - 1, y + 1] != null)
                                    {
                                        layer++;
                                        supports.AddRange(new Block[] { grid[layer][x + 1, y], grid[layer][x - 1, y - 1], grid[layer][x - 1, y + 1] });
                                        continue;
                                    }
                                }
                            }
                        }
                        // Block on the spot (x, y - 1)
                        if (yNegEdge && freeSpace)
                        {
                            if (grid[layer][x, y - 1] != null)
                            {
                                freeSpace = false;

                                // Block also on the spot (x, y + 1), supports elevated blocks -> proceed to next layer
                                if (yPosEdge)
                                {
                                    if (grid[layer][x, y + 1] != null)
                                    {
                                        layer++;
                                        supports.AddRange(new Block[] { grid[layer][x, y - 1], grid[layer][x, y + 1] });
                                        continue;
                                    }
                                }
                                // Blocks also on the spots (x + 1, y + 1), (x - 1, y + 1), (x + 1, y - 1), supports elevated blocks -> proceed to next layer
                                if (yPosEdge && xNegEdge && xPosEdge)
                                {
                                    if (grid[layer][x + 1, y + 1] != null && grid[layer][x - 1, y + 1] != null)
                                    {
                                        layer++;
                                        supports.AddRange(new Block[] { grid[layer][x, y - 1], grid[layer][x + 1, y + 1], grid[layer][x - 1, y + 1] });
                                        continue;
                                    }
                                }
                            }
                        }
                        // Block on the spot (x, y + 1)
                        if (yPosEdge && freeSpace)
                        {
                            if (grid[layer][x, y + 1] != null)
                            {
                                freeSpace = false;

                                // Blocks also on the spots (x + 1, y + 1), (x - 1, y + 1), supports elevated blocks -> proceed to next layer
                                if (yNegEdge && xNegEdge && xPosEdge)
                                {
                                    if (grid[layer][x + 1, y - 1] != null && grid[layer][x - 1, y - 1] != null)
                                    {
                                        layer++;
                                        supports.AddRange(new Block[] { grid[layer][x, y + 1], grid[layer][x + 1, y - 1], grid[layer][x - 1, y - 1] });
                                        continue;
                                    }
                                }
                            }
                        }
                        // Block on the spot (x - 1, y - 1)
                        if (xNegEdge && yNegEdge && freeSpace)
                        {
                            if (grid[layer][x - 1, y - 1] != null)
                            {
                                freeSpace = false;

                                // Blocks also on the spots (x + 1, y + 1), (x - 1, y + 1), (x + 1, y - 1), supports elevated blocks -> proceed to next layer
                                if (yPosEdge && xPosEdge)
                                {
                                    if (grid[layer][x + 1, y + 1] != null && grid[layer][x - 1, y + 1] != null && grid[layer][x + 1, y - 1] != null)
                                    {
                                        layer++;
                                        supports.AddRange(new Block[] { grid[layer][x - 1, y - 1], grid[layer][x + 1, y + 1], grid[layer][x - 1, y + 1], grid[layer][x + 1, y - 1] });
                                        continue;
                                    }
                                }
                            }
                        }
                        // Block on the spot (x - 1, y + 1)
                        if (xNegEdge && yPosEdge && freeSpace)
                        {
                            if (grid[layer][x - 1, y + 1] != null)
                            {
                                freeSpace = false;
                            }
                        }
                        // Block on the spot (x + 1, y - 1)
                        if (xPosEdge && yNegEdge && freeSpace)
                        {
                            if (grid[layer][x + 1, y - 1] != null)
                            {
                                freeSpace = false;
                            }
                        }
                        // Block on the spot (x + 1, y + 1)
                        if (xPosEdge && yPosEdge && freeSpace)
                        {
                            if (grid[layer][x + 1, y + 1] != null)
                            {
                                freeSpace = false;
                            }
                        }
                    }
                    
                    if (freeSpace)
                    {
                        Vector3 newPos = RoundToGrid(hit.point);
                        blockMarker.transform.position = new Vector3(Mathf.Lerp(-gridFarpoint.x, gridFarpoint.x, Mathf.InverseLerp(0f, gridWidth - 1, x)), blockPlacementOffset.y + layer * gridUnitSize.y, Mathf.Lerp(-gridFarpoint.y, gridFarpoint.y, Mathf.InverseLerp(0f, gridHeight - 1, y)));
                        markerGridPoint = new GridPoint(x, y, layer);
                        lastPoint = new GridPoint(x, y, layer);

                        if (!blockMarker.gameObject.activeInHierarchy)
                        {
                            blockMarker.gameObject.SetActive(true);
                        }

                        if (Input.GetMouseButtonDown(0))
                        {
                            GameObject block = Instantiate(blockPlacedPrefab);
                            block.transform.position = blockMarker.transform.position;
                            grid[markerGridPoint.layer][markerGridPoint.width, markerGridPoint.height] = new Block(markerGridPoint, markerGridPoint.layer);
                            print("Block created at [" + markerGridPoint.width + "," + markerGridPoint.height + "] on layer [" + markerGridPoint.layer + "]");
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
                    break;
                }
            }
        }
    }

    float RoundToGrid(float value, float distance)
    {
        return Mathf.RoundToInt(value / distance) * distance;
    }

    Vector3 RoundToGrid(Vector3 value)
    {
        return new Vector3(RoundToGrid(value.x, gridUnitSize.x), RoundToGrid(value.y, gridUnitSize.y), RoundToGrid(value.z, gridUnitSize.z));
    }
}