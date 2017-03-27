using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Block(GridPoint gridPoint, int layer)
    {
        this.gridPoint = gridPoint;
        this.layer = layer;
    }

    public GridPoint gridPoint;
    public int layer;
    public int id;
    public List<Block> supports = new List<Block>();
    public List<Block> supportedBy = new List<Block>();
    public List<Block> blocks = new List<Block>();
    public List<Block> blockedBy = new List<Block>();
    public GameObject blockGameObject;
}
