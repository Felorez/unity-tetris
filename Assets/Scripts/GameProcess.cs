using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameProcess : MonoBehaviour
{
    [Header("Board")]
    public int width = 10;
    public int height = 20;
    public float cellSize = 1f;

    [Header("Fall")]
    public float fallSpeed = 1f;

    [Header("Render")]
    public Sprite cellSpriteBlue;
    public Sprite cellSpriteGreen;
    public Sprite cellSpriteRed;
    public Sprite cellSpriteYellow;
    public Sprite cellSpritePurple;
    public Sprite cellSpriteCyan;
    public Sprite cellSpritePink;

    private int[,] board;
    public TextMeshProUGUI scoreText;
    private Vector2 boardOffset;
    private Transform boardRoot;

    private Tetromino current;
    private float timer;

    private Transform[,] visualGrid;

    public static int score = 0;
    public static event System.Action OnGameOver;
    public static event System.Action OnLineClear;
    public static event System.Action OnDrop;
    public static event System.Action OnRotate;
    public static event System.Action OnClick;

    public static event System.Action OnRestart;

    public TextMeshProUGUI gameOverText;
    public Transform nextPreviewRoot;
    private GameObject[] nextPreviewBlocks;
    private Vector2Int[] nextShape;

    private struct Tetromino
    {
        public Vector2Int pos;
        public Vector2Int[] cells;
        public GameObject[] visual;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"{score}";
    }

    void UpdateNextUI()
    {
        if (nextPreviewBlocks != null)
        {
            foreach (var go in nextPreviewBlocks)
                Destroy(go);
        }

        nextPreviewBlocks = new GameObject[nextShape.Length];

        Sprite randomCellSprite = Random.Range(0, 7) switch
            {
                0 => cellSpriteBlue,
                1 => cellSpriteGreen,
                2 => cellSpriteRed,
                3 => cellSpriteYellow,
                4 => cellSpritePurple,
                5 => cellSpriteCyan,
                6 => cellSpritePink,
                _ => cellSpriteBlue
            };

        for (int i = 0; i < nextShape.Length; i++)
        {
            Vector2Int c = nextShape[i];
            Vector2Int p = new Vector2Int(17, 11) + c;

            GameObject go = new GameObject($"Cell_{i}");
            go.transform.SetParent(boardRoot);

            go.transform.position = new Vector3(
                p.x * cellSize + boardOffset.x,
                p.y * cellSize + boardOffset.y,
                0
            );

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = randomCellSprite;
            sr.sortingOrder = 10;

            nextPreviewBlocks[i] = go;
        }
    }

    void Start()
    {
        board = new int[width, height];
        boardRoot = new GameObject("BoardRoot").transform;
        boardOffset = new Vector2(
            -(width - 1) * cellSize * 0.5f,
            -(height - 1) * cellSize * 0.5f
        );

        visualGrid = new Transform[width, height];

        Spawn();
    }

    void Update()
    {
        if (current.cells == null) return;

        HandleInput();
        HandleFall();
    }

    void HandleInput()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            TryMove(Vector2Int.left);
            OnClick?.Invoke();
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            TryMove(Vector2Int.right);
            OnClick?.Invoke();
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            TryMove(Vector2Int.down);
            OnClick?.Invoke();
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            TryRotate();
            OnRotate?.Invoke();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            OnRestart?.Invoke();
            RestartGame();
        }
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HardDrop();
            return;
        }

    }

    void HardDrop()
    {
        OnDrop?.Invoke();

        while (TryMove(Vector2Int.down))
        {
            // просто летим вниз
        }

        LockPiece();
        SyncLockedPieceVisual();
        ClearLines();
        Spawn();
    }

    void SyncLockedPieceVisual()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (visualGrid[x, y] != null)
                {
                    visualGrid[x, y].position = new Vector3(
                        x * cellSize + boardOffset.x,
                        y * cellSize + boardOffset.y,
                        0
                    );
                }
            }
        }
    }

    void HandleFall()
    {
        timer += Time.deltaTime;
        if (timer < 1f / fallSpeed) return;
        timer = 0;

        if (!TryMove(Vector2Int.down))
        {
            LockPiece();
            ClearLines();
            Spawn();
        }
    }

    void Spawn()
    {
        if (nextShape == null)
            nextShape = TetrominoType();

        var shape = nextShape;
        nextShape = TetrominoType();

        Vector2Int spawnPos = new Vector2Int(width / 2, height - 2);

        if (!IsValid(spawnPos, shape))
        {
            GameOver();
            return;
        }

        current = new Tetromino
        {
            pos = spawnPos,
            cells = shape,
            visual = null
        };

        CreateObjectCells();

        UpdateNextUI();
    }

    void GameOver()
    {
        OnGameOver?.Invoke();
        gameOverText.text = "GAME OVER";
        gameOverText.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    Vector2Int[] TetrominoType() {
        return Random.Range(0, 7) switch
        {
            0 => new Vector2Int[] { new(0,0), new(-1,0), new(1,0), new(0,1) }, // T
            1 => new Vector2Int[] { new(0,0), new(-1,0), new(1,0), new(2,0) }, // I
            2 => new Vector2Int[] { new(0,0), new(-1,0), new(-1,1), new(0,1) }, // O
            3 => new Vector2Int[] { new(0,0), new(-1,0), new(1,0), new(-1,1) }, // J
            4 => new Vector2Int[] { new(0,0), new(-1,0), new(1,0), new(1,1) }, // L
            5 => new Vector2Int[] { new(0,0), new(-1,0), new(0,1), new(1,1) }, // S
            6 => new Vector2Int[] { new(0,0), new(1,0), new(0,1), new(-1,1) }, // Z
            _ => new Vector2Int[] { }
        };
    }

    bool TryMove(Vector2Int dir)
    {
        Vector2Int newPos = current.pos + dir;

        if (!IsValid(newPos, current.cells))
            return false;

        current.pos = newPos;
        return true;
    }

    void TryRotate()
    {
        Vector2Int[] rotated = new Vector2Int[current.cells.Length];

        for (int i = 0; i < current.cells.Length; i++)
        {
            Vector2Int p = current.cells[i];
            rotated[i] = new Vector2Int(-p.y, p.x);
        }

        if (IsValid(current.pos, rotated))
            current.cells = rotated;
    }

    bool IsValid(Vector2Int pos, Vector2Int[] shape)
    {
        foreach (var c in shape)
        {
            int x = pos.x + c.x;
            int y = pos.y + c.y;

            if (x < 0 || x >= width || y < 0)
                return false;

            if (y < height && board[x, y] == 1)
                return false;
        }
        return true;
    }

    void LockPiece()
    {
        for (int i = 0; i < current.cells.Length; i++)
        {
            Vector2Int c = current.cells[i];
            int x = current.pos.x + c.x;
            int y = current.pos.y + c.y;

            if (current.visual[i] != null)
                visualGrid[x, y] = current.visual[i].transform;

            if (y < height)
                board[x, y] = 1;
        }
        OnDrop?.Invoke();
    }

    void ClearLines()
    {
        int linesCleared = 0;

        for (int y = 0; y < height; y++)
        {
            bool full = true;

            for (int x = 0; x < width; x++)
            {
                if (board[x, y] == 0)
                {
                    full = false;
                    break;
                }
            }

            if (full)
            {
                linesCleared++;

                for (int x = 0; x < width; x++)
                {
                    if (visualGrid[x, y] != null)
                    {
                        Destroy(visualGrid[x, y].gameObject);
                        visualGrid[x, y] = null;
                    }
                }

                for (int yy = y; yy < height - 1; yy++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        board[x, yy] = board[x, yy + 1];

                        visualGrid[x, yy] = visualGrid[x, yy + 1];

                        if (visualGrid[x, yy] != null)
                        {
                            visualGrid[x, yy].position += Vector3.down * cellSize;
                        }
                    }
                }

                for (int x = 0; x < width; x++)
                {
                    board[x, height - 1] = 0;
                    visualGrid[x, height - 1] = null;
                }

                y--;
            }
        }

        AddScore(linesCleared);

        UpdateScoreUI();
    }
    
    void AddScore(int lines)
    {
        switch (lines)
        {
            case 1: score += 100; break;
            case 2: score += 300; break;
            case 3: score += 500; break;
            case 4: score += 800; break; // Тетрис
        }

        OnLineClear?.Invoke();
    }

    void LateUpdate()
    {
        Render();
    }

    void Render()
    {
        for (int i = 0; i < current.cells.Length; i++)
        {
            Vector2Int c = current.cells[i];
            Vector2Int p = current.pos + c;

            if (current.visual[i] != null)
                current.visual[i].transform.position = new Vector3(
                    p.x * cellSize + boardOffset.x,
                    p.y * cellSize + boardOffset.y,
                    0
                );
        }
    }

    void CreateObjectCells()
    {
        GameObject[] cells = new GameObject[current.cells.Length];

        Sprite randomCellSprite = Random.Range(0, 7) switch
            {
                0 => cellSpriteBlue,
                1 => cellSpriteGreen,
                2 => cellSpriteRed,
                3 => cellSpriteYellow,
                4 => cellSpritePurple,
                5 => cellSpriteCyan,
                6 => cellSpritePink,
                _ => cellSpriteBlue
            };

        for (int i = 0; i < current.cells.Length; i++)
        {
            Vector2Int c = current.cells[i];
            Vector2Int p = current.pos + c;

            GameObject go = new GameObject($"Cell_{i}");
            go.transform.SetParent(boardRoot);

            go.transform.position = new Vector3(
                p.x * cellSize + boardOffset.x,
                p.y * cellSize + boardOffset.y,
                0
            );

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = randomCellSprite;
            sr.sortingOrder = 10;

            cells[i] = go;
        }

        current.visual = cells;
    }
}