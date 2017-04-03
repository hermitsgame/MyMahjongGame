using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGameObject : MonoBehaviour
{
    private GridPoint _point;

    public void Init(GridPoint point)
    {
        _point = point;
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
