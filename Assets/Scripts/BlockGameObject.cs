using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGameObject : MonoBehaviour
{
    private bool _selected;
    private GridPoint _point;
    private int _type;
    private Level _level;

    private Material mat;

    public GridPoint point
    {
        get { return _point; }
    }

    public int type
    {
        get { return _type; }
    }

    public bool selected
    {
        get { return _selected; }
        set
        {
            if (!value)
            {
                mat.color = startCol;
            }
            _selected = value;
        }
    }

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        startCol = mat.color;
    }

    public void Init(Level level, GridPoint point, int type)
    {
        _point = point;
        _type = type;
        _level = level;
    }

    public bool IsFree()
    {
        return _level.IsBlockFree(_point);
    }

    public void Remove()
    {
        if (_level.RemoveBlock(_point))
            Destroy(gameObject);
    }

    private Color startCol;

    void Update()
    {
        if (selected)
        {
            Color col = mat.color;
            col.a = Mathf.Lerp(0.4f, 0.8f, (Mathf.Sin(Time.timeSinceLevelLoad * 10) + 1) / 2);
            mat.color = col;
        }

    }
}
