using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Block(GridPoint point, int type, List<Block> isSupportedBy, BlockGameObject blockGameObject)
    {
        _point = point;
        _isSupportedBy = isSupportedBy;
        _type = type;
    }

    private int _type;
    private GridPoint _point;
    private int _thisSupportsCount;
    private List<Block> _isSupportedBy = new List<Block>();
    private BlockGameObject _gameObject;

    public int type
    {
        get { return _type; }
    }

    public BlockGameObject gameObject
    {
        get { return _gameObject; }
    }

    public int thisSupportsCount
    {
        get { return _thisSupportsCount; }
        set { _thisSupportsCount = value; }
    }

    public bool IsFree()
    {
        return thisSupportsCount == 0;
    }
}
