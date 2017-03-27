using UnityEngine;
using System.Collections;

public class Prkl : MonoBehaviour
{
    static readonly Vector2 INVALID_COORDS = new Vector2(-1, -1);
    const int INVALID_CELL_IDX = -1;
    const float CUBE_SCALE = 0.9f;

    public int _GridWidth;
    public int _GridHeight;

    private GridCell[] _Grid; // mark occupied cells here from bottom left to top right
    private Building _DraggedBuilding;
    private int _LastMouseIdx;

    void Start()
    {
        // center the grid to screen. fugly implementation
        transform.position = new Vector3(-(_GridWidth / 2f) + 0.5f, -(_GridHeight / 2f) + 0.5f, 0);

        // create a grid of "GridCell" cubes
        _Grid = new GridCell[_GridWidth * _GridHeight];
        for (int i = 0; i < _Grid.Length; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            cube.transform.parent = transform;
            cube.transform.localPosition = indexToCoord(i);
            cube.transform.localScale = Vector3.one * CUBE_SCALE;

            _Grid[i] = new GridCell(cube);
        }
        // mark some cells reserved 
        _Grid[coordToIndex(_GridWidth / 2, _GridHeight / 2)]._Reserved = true;
        _Grid[coordToIndex(_GridWidth / 2, (_GridHeight / 2) - 1)]._Reserved = true;

        // set colors for first time drawing
        resetHighlighting();

        // make a default dragged building
        _DraggedBuilding = new Building();
    }

    // calculate _Grid index based on given x,y -coordinates
    private int coordToIndex(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _GridWidth && y < _GridHeight)
            return x + (y * _GridWidth);
        else
            return INVALID_CELL_IDX;
    }

    // calculate _Grid coords based on given index
    private Vector2 indexToCoord(int i)
    {
        if (i >= 0 && i < _Grid.Length)
            return new Vector2(i % _GridWidth, i / _GridWidth);
        else
            return INVALID_COORDS;
    }

    void Update()
    {
        // tell building to rotate
        if (Input.GetMouseButtonDown(0))
        {
            _DraggedBuilding.rotateLeft();
            _LastMouseIdx = INVALID_CELL_IDX; // reset to make sure we checkFit() after rotation even if not changing cell
        }
        else if (Input.GetMouseButtonDown(1))
        {
            _DraggedBuilding.rotateRight();
            _LastMouseIdx = INVALID_CELL_IDX;
        }

        // check which world coordinate the mouse is over. Since gridCells are 1x1 units, 
        // it's translateable to grid coordinates straight away. Should work well enough
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos -= transform.position - new Vector3(0.5f, 1f, 0f);

        int hoverIndex = coordToIndex((int)worldPos.x, (int)worldPos.y);

        // only check if hovered cell is valid and changed 
        if (hoverIndex != INVALID_CELL_IDX && hoverIndex != _LastMouseIdx)
        {
            resetHighlighting();
            checkFit(_DraggedBuilding, hoverIndex);
        }
    }

    //check if current dragged building fits in this square
    void checkFit(Building building, int idx)
    {
        // bottom left coords of building in grid
        Vector2 startCoords = indexToCoord(idx);
        bool[] buildingCells = building.rotatedCells;

        // loop through building cells and see which cells are reserved in both building and grid
        for (int y = 0; y < Building.HEIGHT; y++)
        {
            for (int x = 0; x < Building.WIDTH; x++)
            {
                // this building cell matches this index in grid
                int indexInGrid = coordToIndex((int)startCoords.x + x, (int)startCoords.y + y);

                if (indexInGrid == INVALID_CELL_IDX)
                    continue;

                int indexInBuilding = building.coordToIndex(x, y);

                // if this index inside the building is reserved, check if the underlying grid cell is reserved
                if (buildingCells[indexInBuilding])
                {
                    _Grid[indexInGrid]._GO.GetComponent<MeshRenderer>().material.color = _Grid[indexInGrid]._Reserved ? Color.red : Color.magenta;
                }
            }
        }
    }

    void resetHighlighting()
    {
        for (int i = 0; i < _Grid.Length; i++)
        {
            // default coloring for reserved and free grid tiles
            _Grid[i]._GO.GetComponent<MeshRenderer>().material.color = _Grid[i]._Reserved ? Color.black : Color.white;
        }
    }

    // simple class to represent a cell in the grid. 
    // _Reserved status and a _GameObject to show status visually
    public class GridCell
    {
        public bool _Reserved;
        public GameObject _GO;

        public GridCell(GameObject cube)
        {
            _GO = cube;
        }
    }

    public class Building
    {
        private const int ROT_0 = 0;
        private const int ROT_90 = 1;
        private const int ROT_190 = 2;
        private const int ROT_270 = 3;
        private const int ROT_COUNT = 4;

        public const int WIDTH = 2;
        public const int HEIGHT = 2;

        private int _Rotation;

        // reserved status of building's cells in it's local space from bottom left to top right
        private bool[][] _MyCells = new bool[][]{
             //         bl   br   tl   tr
             new bool[]{true,true,true,false},
             new bool[]{true,false,true,true},
             new bool[]{false,true,true,true},
             new bool[]{true,true,false,true}
         };

        public bool[] rotatedCells
        {
            get
            {
                return _MyCells[_Rotation];
            }
        }

        public Building()
        {
        }

        public void rotateRight()
        {
            _Rotation = ++_Rotation % ROT_COUNT;
        }

        public void rotateLeft()
        {
            if (--_Rotation < 0)
                _Rotation += ROT_COUNT;
        }

        // calculate index based on given x,y -coordinates
        public int coordToIndex(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < WIDTH && y < HEIGHT)
                return x + (y * WIDTH);
            else
                return INVALID_CELL_IDX;
        }
    }
}
