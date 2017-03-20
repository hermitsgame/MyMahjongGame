using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public int gridSizeX;
    public int gridSizeY;
    public Vector3 gridUnitSize = Vector3.one;
    public GameObject blockMarkerPrefab;
    public GameObject blockPlacedPrefab;

    new private Transform transform;
    private BlockMarker blockMarker;
    //private bool markerActive = false;
    private GridPoint markerGridPoint;
    private Block[,] grid; // maybe make an int obj index table to mark space and separate object table
    private Vector2 gridFarpoint;

    void Awake()
    {
        grid = new Block[gridSizeX, gridSizeY];
        gridFarpoint = new Vector2(gridSizeX / 2f * gridUnitSize.x, gridSizeY / 2f * gridUnitSize.z);
        transform = GetComponent<Transform>();
    }

    void Update()
    {
        BlockMarker();
        BlockPlacement();
    }

    void BlockMarker()
    {
        if (!blockMarker)
        {
            blockMarker = Instantiate(blockMarkerPrefab).GetComponent<BlockMarker>();
        }

        Vector3 pointer;
        bool markerActive = false;

        if (GameController.instance.GetPointerPos(out pointer))
        {
            if (pointer.x >= -gridFarpoint.x && pointer.x <= gridFarpoint.x && pointer.z >= -gridFarpoint.y && pointer.z <= gridFarpoint.y)
            {
                int x = Mathf.RoundToInt(Mathf.Lerp(0, gridSizeX - 1, Mathf.InverseLerp(-gridFarpoint.x, gridFarpoint.x, pointer.x)));
                int y = Mathf.RoundToInt(Mathf.Lerp(0, gridSizeY - 1, Mathf.InverseLerp(-gridFarpoint.y, gridFarpoint.y, pointer.z)));

                bool freeSpace = true;

                if (x > 0)
                {
                    if (grid[x - 1,y] != null)
                    {
                        freeSpace = false;
                    }
                }
                if (x < grid.GetLength(0) - 1)
                {
                    if (grid[x + 1, y] != null)
                    {
                        freeSpace = false;
                    }
                }
                if (y > 0)
                {
                    if (grid[x, y - 1] != null)
                    {
                        freeSpace = false;
                    }
                }
                if (y < grid.GetLength(1) - 1)
                {
                    if (grid[x, y + 1] != null)
                    {
                        freeSpace = false;
                    }
                }

                if (freeSpace)
                {
                    Vector3 newPos = RoundToGrid(pointer);
                    blockMarker.transform.position = new Vector3(Mathf.Lerp(-gridFarpoint.x, gridFarpoint.x, Mathf.InverseLerp(0f, gridSizeX - 1, x)), 0f, Mathf.Lerp(-gridFarpoint.x, gridFarpoint.x, Mathf.InverseLerp(0f, gridSizeY - 1, y)));
                    markerGridPoint = new GridPoint(x, y);
                    markerActive = true;
                }
                else
                {
                    markerActive = false;
                }
            }
        }

        if (markerActive && !blockMarker.gameObject.activeInHierarchy)
        {
            blockMarker.gameObject.SetActive(true);
        }
        else if (!markerActive && blockMarker.gameObject.activeInHierarchy)
        {
            blockMarker.gameObject.SetActive(false);
        }
    }

    void BlockPlacement()
    {
        if (blockMarker.gameObject.activeInHierarchy && Input.GetMouseButtonDown(0))
        {
            GameObject block = Instantiate(blockPlacedPrefab);
            block.transform.position = blockMarker.transform.position;
            grid[markerGridPoint.x, markerGridPoint.y] = new Block(markerGridPoint);
            print("Block created at [" + markerGridPoint.x + "," + markerGridPoint.y + "]");
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

    Vector3 GridPointToPosition(GridPoint point)
    {
        gridFarpoint
    }
}