using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int gridWidth;
    public int gridHeight;
    public int gridLayers;

    [HideInInspector]
    public Block[][,] grid; // maybe make an int obj index table to mark space and separate object table
}
