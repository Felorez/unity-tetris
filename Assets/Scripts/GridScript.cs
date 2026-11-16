// Код был написан для создания сетки и рамки в игре, но был удалён, так как не использовался и не был необходим для текущей реализации. Если в будущем потребуется функциональность сетки, её можно будет восстановить из истории версий.


// using UnityEngine;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif

// public class GridScript : MonoBehaviour
// {
//     [Header("Sprites")]
//     public Sprite gridSprite;
//     public Sprite borderSprite;
    

//     [Header("Grid Size")]
//     [Range(1, 20)] public int width = 10;
//     [Range(1, 30)] public int height = 20;
//     [Tooltip("Размер одной ячейки в юнитах сцены.")]
//     public float cellSize = 1f;

//     [Header("Border")]
//     [Tooltip("Толщина рамки в долях размера ячейки.")]
//     [Range(0.05f, 0.5f)] public float borderThicknessRatio = 0.2f;

//     [Header("Behavior")]
//     public bool buildOnStart = true;
//     public bool autoRebuildInEditor = true;
//     [Header("Placement")]
//     [Tooltip("Если true, позиция объекта будет выровнена по главной камере.")]
//     public bool centerOnCamera = true;

//     [Tooltip("Если true, сетка будет создана один раз и повторно не пересоздаваться, пока явно не попросить.")]
//     public bool buildOnce = true;
//     [Tooltip("Принудительно пересобрать при следующем вызове BuildGrid().")]
//     public bool forceRebuild = false;

//     private const string GridContainerName = "GridTiles";
//     private const string BorderContainerName = "GridBorders";

//     // Internal cached references and last-known parameters to avoid repeated recreation
//     private GameObject gridContainerObj;
//     private GameObject borderContainerObj;
//     private int lastWidth = -1;
//     private int lastHeight = -1;
//     private float lastCellSize = -1f;
//     private Sprite lastGridSprite;
//     private Sprite lastBorderSprite;

//     private void Reset()
//     {
//         LoadDefaultSprites();
//         BuildGrid();
//     }

//     private void Awake()
//     {
//         if (buildOnStart)
//             BuildGrid();
//     }

//     private void Start()
//     {
//         BuildGrid();
//     }

//     public void BuildGrid()
//     {
// #if UNITY_EDITOR
//         LoadDefaultSprites();
// #endif
//         if (gridSprite == null || borderSprite == null)
//         {
//             Debug.LogWarning("GridScript: sprite не назначен. Установите gridSprite и borderSprite в инспекторе.");
//             return;
//         }

//         // Try to find existing containers (in case they were created previously)
//         gridContainerObj = transform.Find(GridContainerName)?.gameObject;
//         borderContainerObj = transform.Find(BorderContainerName)?.gameObject;

//         bool paramsChanged = lastWidth != width || lastHeight != height || !Mathf.Approximately(lastCellSize, cellSize)
//                              || lastGridSprite != gridSprite || lastBorderSprite != borderSprite || forceRebuild;

//         if (buildOnce && gridContainerObj != null && borderContainerObj != null && !paramsChanged)
//         {
//             forceRebuild = false;
//             return; // already built and nothing changed
//         }

//         if (gridContainerObj != null && paramsChanged)
//             DestroyGenerated(gridContainerObj);
//         if (borderContainerObj != null && paramsChanged)
//             DestroyGenerated(borderContainerObj);

//         CreateGridTiles();
//         CreateBorders();

//         // Cache last parameters
//         lastWidth = width;
//         lastHeight = height;
//         lastCellSize = cellSize;
//         lastGridSprite = gridSprite;
//         lastBorderSprite = borderSprite;
//         forceRebuild = false;

//         // Optionally center this GameObject on the main camera
//         FitToCamera();
//     }

//     private void FitToCamera()
//     {
//         Camera cam = Camera.main;

//         if (cam == null || !cam.orthographic)
//             return;

//         float gridWidth = width * cellSize;
//         float gridHeight = height * cellSize;

//         float screenRatio = (float)Screen.width / Screen.height;
//         float targetRatio = gridWidth / gridHeight;

//         if (screenRatio >= targetRatio)
//         {
//             cam.orthographicSize = gridHeight * 0.5f + 1f;
//         }
//         else
//         {
//             float difference = targetRatio / screenRatio;
//             cam.orthographicSize = gridHeight * 0.5f * difference + 1f;
//         }

//         transform.position = new Vector3(
//             cam.transform.position.x,
//             cam.transform.position.y,
//             transform.position.z
//         );
//     }

//     private void ClearGeneratedObjects()
//     {
//         var gridContainer = transform.Find(GridContainerName);
//         if (gridContainer != null)
//         {
//             if (Application.isPlaying)
//                 Destroy(gridContainer.gameObject);
//             else
//                 DestroyImmediate(gridContainer.gameObject);
//         }

//         var borderContainer = transform.Find(BorderContainerName);
//         if (borderContainer != null)
//         {
//             if (Application.isPlaying)
//                 Destroy(borderContainer.gameObject);
//             else
//                 DestroyImmediate(borderContainer.gameObject);
//         }
//     }

//     private void DestroyGenerated(GameObject go)
//     {
//         if (go == null) return;
//         if (Application.isPlaying) Destroy(go);
//         else DestroyImmediate(go);
//     }

//     private void CreateGridTiles()
//     {
//         gridContainerObj = new GameObject(GridContainerName);
//         gridContainerObj.transform.SetParent(transform, false);
//         gridContainerObj.transform.localPosition = Vector3.zero;

//         Vector2 origin = new Vector2(-width * cellSize * 0.5f + cellSize * 0.5f, -height * cellSize * 0.5f + cellSize * 0.5f);

//         for (int y = 0; y < height; y++)
//         {
//             for (int x = 0; x < width; x++)
//             {
//                 GameObject tile = new GameObject($"Cell_{x}_{y}");
//                 tile.transform.SetParent(gridContainerObj.transform, false);
//                 tile.transform.localPosition = new Vector3(origin.x + x * cellSize, origin.y + y * cellSize, 0f);

//                 var renderer = tile.AddComponent<SpriteRenderer>();
//                 renderer.sprite = gridSprite;
//                 renderer.drawMode = SpriteDrawMode.Simple;

//                 float spriteWidth = gridSprite.rect.width / gridSprite.pixelsPerUnit;
//                 float spriteHeight = gridSprite.rect.height / gridSprite.pixelsPerUnit;
//                 renderer.transform.localScale = new Vector3(cellSize / spriteWidth, cellSize / spriteHeight, 1f);
//             }
//         }
//     }

//     private void CreateBorders()
//     {
//         borderContainerObj = new GameObject(BorderContainerName);
//         borderContainerObj.transform.SetParent(transform, false);
//         borderContainerObj.transform.localPosition = Vector3.zero;

//         float totalWidth = width * cellSize + 0.5f;
//         float totalHeight = height * cellSize + 0.5f;
//         float padding = cellSize * borderThicknessRatio;

//         GameObject border = new GameObject("Border");
//         border.transform.SetParent(borderContainerObj.transform, false);
//         border.transform.localPosition = Vector3.zero;

//         var renderer = border.AddComponent<SpriteRenderer>();
//         renderer.sprite = borderSprite;
//         renderer.drawMode = SpriteDrawMode.Simple;

//         float spriteWidth = borderSprite.rect.width / borderSprite.pixelsPerUnit;
//         float spriteHeight = borderSprite.rect.height / borderSprite.pixelsPerUnit;

//         float desiredWidth = totalWidth + padding * 2f;
//         float desiredHeight = totalHeight + padding * 2f;
//         renderer.transform.localScale = new Vector3(desiredWidth / spriteWidth, desiredHeight / spriteHeight, 1f);
//     }

//     private float GetSpritePixelSize(Sprite sprite)
//     {
//         return sprite.rect.width / sprite.pixelsPerUnit;
//     }

// #if UNITY_EDITOR
//     private void LoadDefaultSprites()
//     {
//         if (gridSprite == null)
//             gridSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tetris_Asset_Pack/UI/grid/grid.png");

//         if (borderSprite == null)
//             borderSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tetris_Asset_Pack/UI/border/border.png");
//     }
// #endif
// }
