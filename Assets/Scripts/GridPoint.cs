[System.Serializable]
public struct GridPoint
{
    public GridPoint(int width, int height, int layer)
    {
        this.width = width;
        this.height = height;
        this.layer = layer;
    }

    public GridPoint(int width, int height)
    {
        this.width = width;
        this.height = height;
        layer = 0;
    }

    public int width;
    public int height;
    public int layer;
}
