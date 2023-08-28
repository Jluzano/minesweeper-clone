using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap {get; private set;} // Game logic script might need to read tilemap, but doesn't need to change anything

    // Declaring tiles
    public Tile tileUnknown;
    public Tile tileEmpty;
    public Tile tileMine;
    public Tile tileExploded;
    public Tile tileFlag;
    public Tile tileNum1;
    public Tile tileNum2;
    public Tile tileNum3;
    public Tile tileNum4;
    public Tile tileNum5;
    public Tile tileNum6;
    public Tile tileNum7;
    public Tile tileNum8;

    private void Awake() // In-built function that is called when the GameObject loads
    {
        tilemap = GetComponent<Tilemap>(); //Grabbing tilemap component in script's parent
    }

    // Loop thrugh all cell data and determine what cell to render
    public void Draw(Cell[,] state) // Public because it will be called from core game logic script
    {
        int width = state.GetLength(0);
        int height = state.GetLength(1);

        // Looping through state (2D array)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    }

    //Renders the tile
    private Tile GetTile(Cell cell)
    {
        //Empty, mine, or number tile
        if(cell.revealed)
        {
            return GetRevealedTile(cell);
        }
        //Flagged tile
        else if (cell.flagged)
        {
            return tileFlag;
        }
        //Unknown tile
        else
        {
            return tileUnknown;
        }
    }

    // Check if tile is empty, a mine, or a number
    private Tile GetRevealedTile(Cell cell)
    {
        switch(cell.type)
        {
            case Cell.Type.Empty: return tileEmpty;
            //If cell type is exploded return tileExploded otherwise return tileMine
            case Cell.Type.Mine: return cell.exploded ? tileExploded : tileMine;
            case Cell.Type.Number: return GetNumberTile(cell);
            default: return null;
        }
    }

    //Getting tile's number
    private Tile GetNumberTile(Cell cell)
    {
        switch(cell.number)
        {
            case 1: return tileNum1;
            case 2: return tileNum2;
            case 3: return tileNum3;
            case 4: return tileNum4;
            case 5: return tileNum5;
            case 6: return tileNum6;
            case 7: return tileNum7;
            case 8: return tileNum8;
            default: return null;
        }
    }
}
