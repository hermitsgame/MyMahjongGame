using System.Collections.Generic;

[System.Serializable]
public class Block
{
    // Serialization needs this
    public Block()
    {

    }

    public Block(GridPoint point, List<Block> bottomBlocks, List<Block> leftBlocks, List<Block> rightBlocks, BlockGameObject blockGameObject)
    {
        _point = point;
        this.bottomBlocks = bottomBlocks;
        this.leftBlocks = leftBlocks;
        this.rightBlocks = rightBlocks;

        for (int i = 0; i < bottomBlocks.Count; i++)
        {         
            bottomBlocks[i].topBlocks.Add(this);
        }
    }

    public List<Block> rightBlocks = new List<Block>();
    public List<Block> leftBlocks = new List<Block>();
    public List<Block> bottomBlocks = new List<Block>();
    public List<Block> topBlocks = new List<Block>();

    private GridPoint _point;
    //private BlockGameObject _gameObject;

    /*public BlockGameObject gameObject
    {
        get { return _gameObject; }
    }*/

    public bool IsFree()
    {
        if ((leftBlocks.Count == 0 || rightBlocks.Count == 0) && topBlocks.Count == 0)
        {
            //Debug.Log("Left: " + leftBlocks.Count + "  Right: " + rightBlocks.Count + "  Top: " + topBlocks.Count);
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
            for (int p = 0; p < bottomBlocks[i].topBlocks.Count; p++)
            {
                if (Equals(bottomBlocks[i].topBlocks[p]))
                {
                    bottomBlocks[i].topBlocks.RemoveAt(p);
                }
            }
        }
    }
}
