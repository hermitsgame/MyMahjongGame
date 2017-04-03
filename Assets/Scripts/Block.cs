using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Block(GridPoint point, int type, List<Block> bottomBlocks, List<Block> leftBlocks, List<Block> rightBlocks, BlockGameObject blockGameObject)
    {
        _point = point;
        this.bottomBlocks = bottomBlocks;
        this.leftBlocks = leftBlocks;
        this.rightBlocks = rightBlocks;
        _type = type;

        for (int i = 0; i < bottomBlocks.Count; i++)
        {
            bottomBlocks[i]._thisSupportsCount++;
        }
    }

    public List<Block> rightBlocks = new List<Block>();
    public List<Block> leftBlocks = new List<Block>();
    public List<Block> bottomBlocks = new List<Block>();

    private int _type;
    private GridPoint _point;
    private int _thisSupportsCount;
    private BlockGameObject _gameObject;

    public int type
    {
        get { return _type; }
    }

    public BlockGameObject gameObject
    {
        get { return _gameObject; }
    }

    /*public int thisSupportsCount
    {
        get { return _thisSupportsCount; }
        set { _thisSupportsCount = value; }
    }*/

    public bool IsFree()
    {
        if ((leftBlocks.Count == 0 || rightBlocks.Count == 0) && _thisSupportsCount == 0)
        {
            Debug.Log(leftBlocks.Count + "   " + rightBlocks.Count + "   " + _thisSupportsCount);
            return true;
        } 
        else
            return false;
    }

    public void Remove()
    {
        for (int i = 0; i < leftBlocks.Count; i++)
        {
            for (int p = 0; p < leftBlocks[i].rightBlocks.Count; p++)
            {
                if (leftBlocks[i].rightBlocks[p].Equals(this))
                {
                    leftBlocks[i].rightBlocks.RemoveAt(p);
                    p--;
                }
            }
        }
        for (int i = 0; i < rightBlocks.Count; i++)
        {
            for (int p = 0; p < rightBlocks[i].leftBlocks.Count; p++)
            {
                if (rightBlocks[i].leftBlocks[p].Equals(this))
                {
                    rightBlocks[i].leftBlocks.RemoveAt(p);
                    p--;
                }
            }
        }
        for (int i = 0; i < bottomBlocks.Count; i++)
        {
            bottomBlocks[i]._thisSupportsCount--;
        }
    }
}
