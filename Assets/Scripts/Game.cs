using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Game : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    public int mineCount = 32;
    public GameObject LoseScreen;

    private Board board;
    private Cell[,] state;
    private bool gameover;

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    private void Awake()
    {
        board = GetComponentInChildren<Board>(); // Since script is in board's parent, we use this function to get the board
    }

    private void Start() // Unity calls automatically the first frame the scrip tis enabled
    {
        NewGame();
    }
    
    private void NewGame()
    {
        LoseScreen.SetActive(false);
        state = new Cell[width, height];
        gameover = false;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        //Repositioning the camera
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);

        board.Draw(state);
    }

    //Generates a grid full of empty tiles
    private void GenerateCells()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        //Generating mines in random places through the grid
        for(int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            //Checks if tiles is already a mine
            while(state[x, y].type == Cell.Type.Mine)
            {
                x++;
                //If you reach the end of a row
                if(x >= width)
                {
                    x = 0;
                    y++;
                    //If you reach the end of a column
                    if(y >= height)
                    {
                        y = 0;
                    }
                }
            }
            // Changing state of tile to mine
            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                //Skips if tile is a mine
                if(cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                cell.number = CountMines(x, y);

                //Changes cell to number if it is adjacent to a mine
                if(cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                //Reassign cell to state since changes were made
                state[x, y] = cell;
            }
        }
    }

    //Changes cell number depending on adjacent mines
    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        //Starting from the top left diagonal tile
        for(int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for(int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                //Already know that center tiles is not a mine, so skip
                if(adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                //Changes number accordingly
                if(GetCell(x, y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void Update() //Built-in function that is called every frame
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
        }
        //Won't allow player to click if they get game over
        else if(!gameover)
        {
            //Checking for right click
            if(Input.GetMouseButtonDown(1))
            {
                Flag();
            }
            //Checking for left click
            else if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }
        }
    }

    //Function to flag a square
    private void Flag()
    {
        //Converting from screen space to world space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        //Can't flag if tile is invalid or already revealed
        if(cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        //Otherwise, set cell flagged to the opposite of whatever it is currently
        cell.flagged = !cell.flagged;
        //Now that our state has changed, reassign back to cell
        state[cellPosition.x, cellPosition.y] = cell;
        //Also redraw the board since we changed state
        board.Draw(state);
    }

    //Function to reveal tile at cursor position
    private void Reveal()
    {
        //Converting from screen space to world space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        //Can't flag if tile is invalid, already revealed, or flagged
        if(cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        switch(cell.type)
        {
            case Cell.Type.Mine: 
                Explode(cell);
                break;

            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                //Else, reveal cell
                cell.revealed = true;
                //Now that our state has changed, reassign back to cell
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }
        //Also redraw the board since we changed state
        board.Draw(state);
    }


    //Function to flood to adjacent empty tiles
    private void Flood(Cell cell)
    {
        //Skips if cell is already revealed, a mine, invalid, or flagged
        if(cell.revealed) return;
        if(cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid || cell.flagged) return;

        //Reveals the cell and then sets state back to cell
        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        //Moves to adjacent cells and recursively calls the Flood function
        if(cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
        }
    }

    //Function for clicking on a mine
    private void Explode(Cell cell)
    {
        Debug.Log("Game Over!");
        gameover = true;
        LoseScreen.SetActive(true);

        cell.revealed = true;
        cell.exploded = true;

        state[cell.position.x, cell.position.y] = cell;

        //Reveal other mines on the board
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                cell = state[x, y];
                if(cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    private void CheckWinCondition()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if(cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        Debug.Log("Winner!");
        gameover = true;
        
        //Reveal other mines on the board
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if(cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    //Get cell at position
    private Cell GetCell(int x, int y)
    {
        if(IsValid(x, y))
        {
            return state[x, y];
        }
        else
        {
            //Since cells are marked as Empty when they are created, they are not marked as Invalid
            //However, new cells marked as invalid by default
            return new Cell();
        }
    }

    //Helper function to check if location is within the board
    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

}
