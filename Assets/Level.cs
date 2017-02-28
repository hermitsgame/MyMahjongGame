using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Vector3 center;
    public Vector3 gridUnitSize = Vector3.one;
    public GameObject blockMarkerPrefab;
    public GameObject blockPlacedPrefab;

    new private Transform transform;
    private BlockMarker blockMarker;

    void Awake()
    {
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
        if (GameController.instance.GetPointerPos(out pointer))
        {
            Vector3 newPos = RoundToGrid(pointer);

            if (!blockMarker.gameObject.activeInHierarchy)
            {
                blockMarker.gameObject.SetActive(true);
            }

            blockMarker.transform.position = newPos;
        }
        else
        {
            if (blockMarker.gameObject.activeInHierarchy)
            {
                blockMarker.gameObject.SetActive(false);
            }
        }
    }

    void BlockPlacement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject block = Instantiate(blockPlacedPrefab);
            block.transform.position = blockMarker.transform.position;
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

public class Block
{
    public int sizeX = 1;
    public int sizeY = 1;
    //public List<Block> blocksLeftSide = new List<Block>();
    //public List<Block> blocksRightSide = new List<Block>();
    public int posX = 0;
    public int posY = 0;
    public int height = 0;
}
