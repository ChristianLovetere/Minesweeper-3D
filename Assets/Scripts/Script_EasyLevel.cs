using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

public class Script_EasyLevel : MonoBehaviour
{
    public static event Action OnFlagAdded;
    public static event Action OnFlagRemoved;
    public static event Action<int> OnBombsAllPlaced;

    public static int mapHeight = 11; //actual height of the play area, +2, one for each edge
    public static int mapWidth = 11; //actual width of the play area, +2, one for each edge
    public static int mapDepth = 11;

    public static Vector3 mapCenterVect = new Vector3((mapWidth - 1f) / 2f, (mapHeight - 1f) / 2f, (mapDepth - 1f) / 2f);
    public int numberOfBombs = 0;

    private int nonBombsRevealed = 0;
    private TMP_Text seedText;
    [SerializeField] private GameObject screenTintingPanel;
    public Script_ScreenShake screenShaker;
    public Script_GameOverManager gameOverScript;
    private Vector3Int startLocation;
    private bool hasWon = false;

    [SerializeField] private AudioClip[] UIPopSFX;
    [SerializeField] private AudioClip[] explosionSFX;
    [SerializeField] private AudioClip[] winSFX;

    [SerializeField] private Canvas UICanvas;
    [SerializeField] private GameObject GameEndingScreen;
    [SerializeField] private Script_PauseMenu PauseManager;
    [SerializeField] private Script_CornerButton CornerButton;
    [SerializeField] private Script_SceneSwitcher SceneSwitcher;
    [SerializeField] private Camera gameCamera;

    private bool isSpaceOutRunning = false;
    public class Cell
    {
        public bool IsBomb { get; set; }
        public bool Seen { get; set; }
        public bool IsEdge { get; set; }
        public bool IsFlagged { get; set; }
        public Cell(bool IsBomb, bool IsEdge)
        {
            this.IsBomb = IsBomb;
            Seen = false;
            this.IsEdge = IsEdge;
        }
    }

    Cell[,,] board = new Cell[mapHeight, mapWidth, mapDepth];
    int[,,] nearbyBoard = new int[mapHeight, mapWidth, mapDepth];

    public GameObject cellPrefab;
    public GameObject edgePrefab;
    //public GameObject EdgeCellPrefab;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = mapCenterVect;
        Debug.Log(mapCenterVect);

        SetPanelOpacity(screenTintingPanel, 0);

        int levelSeed = UnityEngine.Random.Range(0, 999999);
        UnityEngine.Random.InitState(levelSeed);

        seedText = GameObject.Find("Text (TMP)_LevelSeed").GetComponent<TMP_Text>();
        seedText.text = $"Level Seed: {levelSeed}";
        for (int m = 0; m < mapDepth; m++)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (i == 0 || j == 0 || m == 0 || i == mapWidth - 1 || j == mapHeight - 1 || m == mapDepth - 1) //if outside of map
                    {
                        board[j, i, m] = new Cell(false, true); //construct with IsBomb false, IsEdge true
                    }
                    else //if inside map
                    {
                        int bombOrNot = UnityEngine.Random.Range(0, 40); //bomb by random
                        if (bombOrNot > 38) //if bomb
                        {
                            numberOfBombs++;
                            Debug.Log($"Placing bomb at {i}, {j}, {m}");

                            board[j, i, m] = new Cell(true, false); //construct with IsBomb true, IsEdge false
                            for (int n = m - 1; n < m + 2; n++)
                            {
                                for (int k = i - 1; k < i + 2; k++)
                                {  //for 3x3x3 of adjacent locations centered around bomb,
                                    for (int l = j - 1; l < j + 2; l++)
                                    {
                                        if (l == 0 || k == 0 || n == 0 || l == mapHeight - 1 || k == mapWidth - 1 || n == mapDepth - 1 || ((l == j && k == i) && n == m)) //if its an edge or the center of the 3x3,
                                            continue;
                                        else
                                        {
                                            nearbyBoard[l, k, n]++; //add 1 to all locations next to the center (bomb)
                                            //Debug.Log($"adding 1 nearbyBomb to {k},{l},{n} for a total of {nearbyBoard[l, k, n]}, because of the bomb at {i}, {j}, {m}");
                                        }
                                    }
                                }
                            }
                        }
                        else
                            board[j, i, m] = new Cell(false, false); //construct with IsBomb false, IsEdge false

                        GameObject newCell = Instantiate(cellPrefab, new Vector3(i, j, m), Quaternion.Euler(-90, -90, 0));
                        newCell.name = $"Cell_{j}_{i}_{m}";

                        Script_CellPrefab cellComponent = newCell.GetComponent<Script_CellPrefab>();
                        if (cellComponent != null)
                        {
                            cellComponent.OnCellClicked += HandleCellClicked;
                            cellComponent.OnCellRightClicked += HandleCellRightClicked;
                            cellComponent.OnCellLeftClickHeld += HandleCellLeftClickHeld;
                            cellComponent.OnCellExit += HandleCellExited;
                        }
                    }
                }
            }
        }
        Debug.Log($"Number of bombs is {numberOfBombs}");
        OnBombsAllPlaced?.Invoke(numberOfBombs);
        startLocation = FindStartSpot();
        Debug.Log($"start pos is {startLocation}");
        GameObject startCell = GetCellInstance(startLocation);
        if (startCell != null)
        {
            MeshRenderer startCellComponent = startCell.GetComponent<MeshRenderer>();
            if (startCellComponent != null)
            {
                startCellComponent.material = Resources.Load<Material>("Materials/Material_CellStart");
            }
        }
        for (int z = 0; z < mapDepth; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Debug.LogFormat("Value at ({0}, {1}, {2}): {3}", x, y, z, nearbyBoard[y, x, z]);
                }
            }
        }
    }
    Vector3Int FindStartSpot()
    {
        int iterations = 0;
        while (true)
        {
            int xPos = UnityEngine.Random.Range(1, mapWidth - 2);
            int yPos = UnityEngine.Random.Range(1, mapHeight - 2);
            int zPos = UnityEngine.Random.Range(1, mapDepth - 2);

            if (nearbyBoard[yPos, xPos, zPos] == 0 && !board[yPos, xPos, zPos].IsBomb)
                return new Vector3Int(xPos, yPos, zPos);

            iterations++;
            Debug.Log($"Finding start pos took {iterations} tries");
        }
    }

    void HandleCellClicked(Vector3Int gridPosition)
    {

        //If its flagged, return
        if (board[gridPosition.y, gridPosition.x, gridPosition.z].IsFlagged)
            return;

        if (nearbyBoard[gridPosition.y, gridPosition.x, gridPosition.z] != 0 || gridPosition == startLocation)
            Script_SFXManager.instance.PlayRandomSFXClip(UIPopSFX, transform, 1f);

        //If its seen, player is trying to quick reveal nearby flags, or holding leftclick to view unrevealed nearby cells
        if (board[gridPosition.y, gridPosition.x, gridPosition.z].Seen && nearbyBoard[gridPosition.y, gridPosition.x, gridPosition.z] != 0)
        {
            if (JudgeQuickFlags(gridPosition) == 0)
                RevealNearby(gridPosition);
            else if (JudgeQuickFlags(gridPosition) == 1)
            {
                RevealNearby(gridPosition);
                GameLost();
            }
            RevertColor(gridPosition);
            return;
        }

        // If it's a mine, go back to the main menu
        if (board[gridPosition.y, gridPosition.x, gridPosition.z].IsBomb)
        {
            Reveal(gridPosition);
            Script_SFXManager.instance.PlayRandomSFXClip(explosionSFX, transform, 1f);
            GameLost();
        }

        //if its a safe space, recursively reveal all nearby safe spaces and their nearby numbered spaces
        else if (nearbyBoard[gridPosition.y, gridPosition.x, gridPosition.z] == 0)
        {
            RecursiveReveal(gridPosition);
        }
        //if its a number spot
        else
        {
            Reveal(gridPosition);
        }
    }

    void HandleCellRightClicked(Vector3Int gridPosition)
    {
        GameObject cellToFlag = GetCellInstance(gridPosition);
        if (cellToFlag != null)
        {
            Script_CellPrefab cellComponent = cellToFlag.GetComponent<Script_CellPrefab>();
            if (cellComponent != null)
            {
                if (board[gridPosition.y, gridPosition.x, gridPosition.z].IsFlagged)
                {
                    board[gridPosition.y, gridPosition.x, gridPosition.z].IsFlagged = false;
                    cellComponent.UpdateMaterial("blank");

                    OnFlagRemoved?.Invoke();
                }
                else if (!board[gridPosition.y, gridPosition.x, gridPosition.z].Seen)
                {
                    board[gridPosition.y, gridPosition.x, gridPosition.z].IsFlagged = true;
                    cellComponent.UpdateMaterial("flag");

                    OnFlagAdded?.Invoke();
                }
            }
        }
    }

    private void RevertColor(Vector3Int gridPosition)
    {
        for (int z = gridPosition.z - 1; z <= gridPosition.z + 1; z++)
        {
            for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
            {
                for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
                {
                    if (board[y, x, z].IsEdge || (y == gridPosition.y && x == gridPosition.x && z == gridPosition.z) || board[y, x, z].Seen)
                        continue;
                    GameObject cellHere = GetCellInstance(new Vector3Int(x, y, z));
                    cellHere.GetComponent<MeshRenderer>().material.color = Color.white;
                }
            }
        }
    }
    private void HandleCellLeftClickHeld(Vector3Int gridPosition)
    {
        if (!board[gridPosition.y, gridPosition.x, gridPosition.z].Seen)
            return;
        for (int z = gridPosition.z - 1; z <= gridPosition.z + 1; z++)
        {
            for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
            {
                for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
                {
                    if (board[y, x, z].IsEdge || (y == gridPosition.y && x == gridPosition.x && z == gridPosition.z) || board[y, x, z].Seen)
                        continue;
                    GameObject cellHere = GetCellInstance(new Vector3Int(x, y, z));
                    cellHere.GetComponent<MeshRenderer>().material.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                }
            }
        }
    }

    private void HandleCellExited(Vector3Int gridPosition)
    {
        if (!Script_PauseMenu.isPaused)
            RevertColor(gridPosition);
    }

    private void RecursiveReveal(Vector3Int gridPosition)
    {
        if (board[gridPosition.y, gridPosition.x, gridPosition.z].IsEdge)
        {
            Debug.Log($"{gridPosition} was an edge, returning.");
            return;
        }
        for (int m = gridPosition.z - 1; m < gridPosition.z + 2; m++)
        {
            for (int i = gridPosition.x - 1; i < gridPosition.x + 2; i++)
            {
                for (int j = gridPosition.y - 1; j < gridPosition.y + 2; j++)
                {
                    if (!board[j, i, m].IsEdge && !board[j, i, m].Seen && !board[j, i, m].IsBomb)
                    {
                        Reveal(new Vector3Int(i, j, m));
                        if (FindNearbyMines(j, i, m) == 0)
                        {
                            RecursiveReveal(new Vector3Int(i, j, m));
                        }
                    }
                }
            }
        }
    }

    private void Reveal(Vector3Int gridPosition)
    {
        GameObject cellToReveal = GetCellInstance(gridPosition);
        board[gridPosition.y, gridPosition.x, gridPosition.z].Seen = true;

        if (!board[gridPosition.y, gridPosition.x, gridPosition.z].IsBomb)
            nonBombsRevealed++;

        Debug.Log($"goal: {(mapWidth - 2) * (mapHeight - 2) * (mapDepth - 2) - numberOfBombs}, current: {nonBombsRevealed}, total bombs: {numberOfBombs}");
        if (cellToReveal != null)
        {
            Script_CellPrefab cellComponent = cellToReveal.GetComponent<Script_CellPrefab>();
            if (cellComponent != null)
            {
                if (board[gridPosition.y, gridPosition.x, gridPosition.z].IsBomb)
                    cellComponent.UpdateMaterial("bomb");
                else {
                    int nearbyMines = FindNearbyMines(gridPosition.y, gridPosition.x, gridPosition.z);
                    //Debug.Log($"There were {nearbyMines} adjacent to {gridPosition}.");
                    cellComponent.UpdateMaterial(nearbyMines.ToString());
                    //cellComponent.UpdateMaterial(nearbyMines);
                }
            }
        }
        else
            Debug.Log("Reveal: Cell could not be found");
    }

    private void RevealNearby(Vector3Int gridPosition)
    {
        for (int z = gridPosition.z - 1; z <= gridPosition.z + 1; z++)
        {
            for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
            {
                for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
                {
                    if (board[y, x, z].IsEdge || (y == gridPosition.y && x == gridPosition.x && z == gridPosition.z) || board[y, x, z].Seen || board[y, x, z].IsFlagged)
                        continue;

                    if (nearbyBoard[y, x, z] != 0)
                        Reveal(new Vector3Int(x, y, z));
                    else
                        RecursiveReveal(new Vector3Int(x, y, z));
                }
            }
        }
    }

    private int JudgeQuickFlags(Vector3Int gridPosition)
    {
        byte badFlags = 0;
        byte numFlags = 0;
        for (int z = gridPosition.z - 1; z <= gridPosition.z + 1; z++)
        {
            for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
            {
                for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
                {
                    if (board[y, x, z].IsEdge || (y == gridPosition.y && x == gridPosition.x && z == gridPosition.z) || board[y, x, z].Seen)
                        continue;

                    if (board[y, x, z].IsFlagged)
                        numFlags++;
                    if (board[y, x, z].IsBomb != board[y, x, z].IsFlagged)
                        badFlags = 1;
                }
            }
        }
        if (numFlags != nearbyBoard[gridPosition.y, gridPosition.x, gridPosition.z])
            return -1; //dont do anything
        else
            return badFlags;
    }
    GameObject GetCellInstance(Vector3Int gridPosition)
    {
        string cellName = $"Cell_{gridPosition.y}_{gridPosition.x}_{gridPosition.z}";
        return GameObject.Find(cellName);
    }

    private int FindNearbyMines(int y, int x, int z)
    {
        if (board[y, x, z].IsBomb)
            return -1;
        else return nearbyBoard[y, x, z];
    }

    private void SetPanelOpacity(GameObject panel, float opacity)
    {
        // Ensure the panel has a CanvasRenderer component
        CanvasRenderer canvasRenderer = panel.GetComponent<CanvasRenderer>();
        if (canvasRenderer != null)
        {
            // Set the alpha value (opacity)
            canvasRenderer.SetAlpha(opacity);
        }
        else
        {
            Debug.LogError("Panel does not have a CanvasRenderer component.");
        }
    }

    private void GameEnds(string endText)
    {
        Script_PauseMenu.gameOver = true;
        Script_PauseMenu.isPaused = true;

        Vector3 endScreenSpawnPoint = new Vector3(960, 540, 0f);
        GameObject GameEndingScreenObj = Instantiate(GameEndingScreen, endScreenSpawnPoint, Quaternion.identity);
        GameEndingScreenObj.transform.parent = UICanvas.transform;

        Script_GameOverManager gameOverScript = GameEndingScreenObj.GetComponent<Script_GameOverManager>();
        gameOverScript.tintingPanel = screenTintingPanel;

        UnityEngine.UI.Button[] GameEndingScreenButtons = GameEndingScreenObj.GetComponentsInChildren<UnityEngine.UI.Button>();

        GameEndingScreenButtons[0].onClick.AddListener(PauseManager.StartReviewing);
        GameEndingScreenButtons[0].onClick.AddListener(CornerButton.Show);
        GameEndingScreenButtons[1].onClick.AddListener(PauseManager.Resume);
        GameEndingScreenButtons[1].onClick.AddListener(SceneSwitcher.ChangeToMainMenu);
        GameEndingScreenButtons[1].onClick.AddListener(PauseManager.StopReviewing);
        GameEndingScreenButtons[1].onClick.AddListener(PauseManager.ResetBools);

        TextMeshProUGUI[] GameEndingScreenTMPs = GameEndingScreenObj.GetComponentsInChildren<TextMeshProUGUI>();

        GameEndingScreenTMPs[2].text = endText;
    }
    private void GameWon()
    {
        GameEnds("VICTORY!");
    }
    private void GameLost()
    {
        GameEnds("GAME OVER");
        screenShaker.StartShake();
        SetPanelOpacity(screenTintingPanel, 1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ZSlices();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            XSlices();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            YSlices();
        }

        if (Input.GetKey(KeyCode.LeftShift) && !isSpaceOutRunning)
        {
            StartCoroutine(SpaceOutCells());
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ResetCellsPosition();
        }

        if (hasWon) return; //EVERYTHING BELOW DOES NOT GET CHECKED ONCE THE GAME HAS BEEN WON

        if ((mapWidth - 2) * (mapHeight - 2) * (mapDepth - 2) - numberOfBombs == nonBombsRevealed)
        {
            hasWon = true;
            Script_SFXManager.instance.PlayRandomSFXClip(winSFX, transform, 1.75f);
            GameWon();
        }
    }

    private void XSlices()
    {
        ClearEdges();

        for (int z = 1; z < mapDepth - 1; z++)
        {
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    GameObject currentCell = GetCellInstance(new Vector3Int(x, y, z));
                    float alignmentVal = FindParticularValue(mapWidth-2);

                    float newYValue;
                    float newZValue;

                    newYValue = y - (x - 1) / (int)alignmentVal * mapHeight + (alignmentVal - 1) / 2f * mapHeight;

                    newZValue = z + (x - 1) % alignmentVal * mapDepth - (alignmentVal - 1) / 2f * mapDepth;

                    currentCell.transform.position = new Vector3((mapWidth - 1) / 2, newYValue, newZValue);
                    currentCell.transform.rotation = Quaternion.Euler(-90, -90, 0);
                }
            }
        }
        MakeEdges('x');
    }

    private void YSlices()
    {
        ClearEdges();

        for (int z = 1; z < mapDepth - 1; z++)
        {
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    GameObject currentCell = GetCellInstance(new Vector3Int(x, y, z));
                    float alignmentVal = FindParticularValue(mapHeight-2);

                    float newZValue;
                    float newXValue;

                    newZValue = z + (y - 1) / (int)alignmentVal * mapDepth - (alignmentVal - 1) / 2f * mapDepth;

                    newXValue = x + (y - 1) % alignmentVal * mapWidth - (alignmentVal - 1) / 2f * mapWidth;

                    currentCell.transform.position = new Vector3(newXValue,(mapHeight - 1) / 2, newZValue);
                    currentCell.transform.rotation = Quaternion.Euler(-90, -90, 0);
                }
            }
        }
        MakeEdges('y');
    }

    private void ZSlices()
    {
        ClearEdges();

        for (int z = 1; z < mapDepth - 1; z++)
        {
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    float alignmentVal = FindParticularValue(mapDepth - 2);

                    float newYValue = y - (z - 1) / (int)alignmentVal * mapHeight + (alignmentVal - 1) / 2f * mapHeight;

                    float newXValue = x + (z - 1) % alignmentVal * mapWidth - (alignmentVal - 1) / 2f * mapWidth;

                    Debug.Log($"not an edge at {x}, {y}, {z}");
                    GameObject currentCell = GetCellInstance(new Vector3Int(x, y, z));

                    currentCell.transform.position = new Vector3(newXValue, newYValue, (mapDepth - 1) / 2);
                    currentCell.transform.rotation = Quaternion.Euler(-90, -90, 0);
                }
            }
        }
        MakeEdges('z');
    }

    private void MakeEdges(char config)
    {
        GameObject LeftAnchorCell;
        GameObject RightAnchorCell;

        if (config == 'x' || config == 'z') //for x and z: tfl bbr
        {
            LeftAnchorCell = GetCellInstance(new Vector3Int(1, mapHeight - 2, 1));
            RightAnchorCell = GetCellInstance(new Vector3Int(mapWidth - 2, 1, mapDepth - 2));
        }
        else //for y: front bottom l top back r
        {
            LeftAnchorCell = GetCellInstance(new Vector3Int(1, 1, 1));
            RightAnchorCell = GetCellInstance(new Vector3Int(mapWidth - 2, mapHeight - 2, mapDepth - 2));
        }

        int LeftAnchorCellX = (int)LeftAnchorCell.transform.position.x;
        int LeftAnchorCellY = (int)LeftAnchorCell.transform.position.y;
        int LeftAnchorCellZ = (int)LeftAnchorCell.transform.position.z;
        int RightAnchorCellX = (int)RightAnchorCell.transform.position.x;
        int RightAnchorCellY = (int)RightAnchorCell.transform.position.y;
        int RightAnchorCellZ = (int)RightAnchorCell.transform.position.z;


        switch (config) {

            case 'x':
                for (int y = LeftAnchorCellY + 1; y > LeftAnchorCellY + 1 - mapHeight + 2 - 1; y--)
                {
                    for(int z = LeftAnchorCellZ - 1; z < LeftAnchorCellZ - 1 + mapDepth - 2 + 1; z++)
                    {
                        if (y <= LeftAnchorCellY && z >= LeftAnchorCellZ)
                            continue;
                        Instantiate(edgePrefab, new Vector3((mapWidth - 1) / 2, y, z), Quaternion.Euler(-90, -90, 0));
                    }
                }
                for (int y = RightAnchorCellY - 1; y < RightAnchorCellY - 1 + mapHeight - 2 + 1; y++)
                {
                    for (int z = RightAnchorCellZ + 1; z > RightAnchorCellZ + 1 - mapDepth + 2 - 1; z--)
                    {
                        if (y >= RightAnchorCellY && z <= RightAnchorCellZ)
                            continue;
                        Instantiate(edgePrefab, new Vector3((mapWidth - 1) / 2, y, z), Quaternion.Euler(-90, -90, 0));
                    }
                }
                break;
            case 'y':
                for (int x = LeftAnchorCellX - 1; x < LeftAnchorCellX - 1 + mapWidth - 2 + 1; x++)
                {
                    for(int z = LeftAnchorCellZ - 1; z < LeftAnchorCellZ - 1 + mapDepth - 2 + 1; z++)
                    {
                        if (x >= LeftAnchorCellX && z >= LeftAnchorCellZ)
                            continue;
                        Instantiate(edgePrefab, new Vector3(x, (mapHeight - 1) / 2, z), Quaternion.Euler(-90, -90, 0));
                    }
                }
                for (int x = RightAnchorCellX + 1; x > RightAnchorCellX + 1 - mapWidth + 2 - 1; x--)
                {
                    for (int z = RightAnchorCellZ + 1; z > RightAnchorCellZ + 1 - mapDepth + 2 - 1; z--)
                    {
                        if (x <= RightAnchorCellX && z <= RightAnchorCellZ)
                            continue;
                        Instantiate(edgePrefab, new Vector3(x, (mapHeight - 1) / 2, z), Quaternion.Euler(-90, -90, 0));
                    }
                }
                break;
            case 'z':
                for (int x = LeftAnchorCellX - 1; x < LeftAnchorCellX - 1 + mapWidth - 2 + 1; x++)
                {
                    for (int y = LeftAnchorCellY + 1; y > LeftAnchorCellY + 1 - mapHeight + 2 - 1; y--)
                    {
                        if (x >= LeftAnchorCellX && y <= LeftAnchorCellY)
                            continue;
                        Instantiate(edgePrefab, new Vector3(x, y, (mapDepth - 1) / 2), Quaternion.Euler(-90, -90, 0));
                    }
                }

                for (int x = RightAnchorCellX + 1; x > RightAnchorCellX + 1 - mapWidth + 2 - 1; x--)
                {
                    for (int y = RightAnchorCellY - 1; y < RightAnchorCellY - 1 + mapHeight - 2 + 1; y++)
                    {
                        if (x <= RightAnchorCellX && y >= RightAnchorCellY)
                            continue;
                        Instantiate(edgePrefab, new Vector3(x, y, (mapDepth - 1) / 2), Quaternion.Euler(-90, -90, 0));
                    }
                }
                break;
            default:
                break;
        }
    }

    private float FindParticularValue(int limitToCheck)
    {
        for (int i = limitToCheck; ; i++)
        {
            if(Mathf.Sqrt(i) % 1 == 0)
                return Mathf.Sqrt(i);
        }
    }

    private void ClearEdges()
    {
        foreach (var gameObj in GameObject.FindGameObjectsWithTag("Edge Cell"))
        {
            Destroy(gameObj);
        }
    }

    private char GetCameraClosestAxis()
    {
        float camY = gameCamera.gameObject.transform.rotation.eulerAngles.y;
        float camX = gameCamera.gameObject.transform.rotation.eulerAngles.x;

        if ((camX >= 45 && camX < 135) ||
            (camX >= 225 && camX < 315))
        {
            return 'y';
        }
        else if ((camY <= 135 && camY > 45) ||
                 (camY > 225 && camY <= 315))
        {
            return 'x';
        }
        else if ((camY <= 45 || camY > 315) ||
                 (camY <= 225 && camY > 135))
        {
            return 'z';
        }
        else return '!';
    }

    private IEnumerator SpaceOutCells()
    {
        isSpaceOutRunning = true;

        if (GetCameraClosestAxis() == 'y')
        {
            SpaceOutHelperY();
        }
        else if (GetCameraClosestAxis() == 'x')
        {
            SpaceOutHelperX();
        }
        else if (GetCameraClosestAxis() == 'z')
        {
            SpaceOutHelperZ();
        }

        yield return new WaitForSeconds(0.25f);
        isSpaceOutRunning = false;
    }

    private void SpaceOutHelperX()
    {
        bool twoIterationsHavePassed = true;
        for (int x = (mapWidth - 1) / 2 + 1, q = 1, its = 0; its < mapWidth - 3; q *= -1, its++)
        {
            //Debug.Log($"Evaluating z layer {z}. q is {q}");
            for (int z = 1; z < mapDepth - 1; z++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    //Debug.Log($"Finding cell at {x}, {y},{z}");
                    GameObject currentCell = GetCellInstance(new Vector3Int(x, y, z));
                    currentCell.transform.position = new Vector3Int((int)currentCell.transform.position.x + q, (int)currentCell.transform.position.y, (int)currentCell.transform.position.z);
                    //Debug.Log($"{currentCell.name}'s new location is {currentCell.transform.position}");
                }
            }
            if (its % 2 == 0) //if q is odd,
                x -= (its + 2);
            else if (its % 2 != 0) // if q is even,
                x += (its + 2);
            twoIterationsHavePassed = !twoIterationsHavePassed;
            if (twoIterationsHavePassed)
            {
                if (q > 0)
                    q++;
                else
                    q--;
            }
        }
    }
    private void SpaceOutHelperY()
    {
        bool twoIterationsHavePassed = true;
        for (int y = (mapHeight - 1) / 2 + 1, q = 1, its = 0; its < mapHeight - 3; q *= -1, its++)
        {
            //Debug.Log($"Evaluating z layer {z}. q is {q}");
            for (int z = 1; z < mapDepth - 1; z++)
            {
                for (int x = 1; x < mapWidth - 1; x++)
                {
                    //Debug.Log($"Finding cell at {x}, {y},{z}");
                    GameObject currentCell = GetCellInstance(new Vector3Int(x, y, z));
                    currentCell.transform.position = new Vector3Int((int)currentCell.transform.position.x, (int)currentCell.transform.position.y + q, (int)currentCell.transform.position.z);
                    //Debug.Log($"{currentCell.name}'s new location is {currentCell.transform.position}");
                }
            }
            if (its % 2 == 0) //if q is odd,
                y -= (its + 2);
            else if (its % 2 != 0) // if q is even,
                y += (its + 2);
            twoIterationsHavePassed = !twoIterationsHavePassed;
            if (twoIterationsHavePassed)
            {
                if (q > 0)
                    q++;
                else
                    q--;
            }
        }
    }
    private void SpaceOutHelperZ()
    {
        bool twoIterationsHavePassed = true;
        for (int z = (mapDepth - 1) / 2 + 1, q = 1, its = 0; its < mapDepth - 3; q *= -1, its++)
        {
            //Debug.Log($"Evaluating z layer {z}. q is {q}");
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    //Debug.Log($"Finding cell at {x}, {y},{z}");
                    GameObject currentCell = GetCellInstance(new Vector3Int(x, y, z));
                    currentCell.transform.position = new Vector3Int((int)currentCell.transform.position.x, (int)currentCell.transform.position.y, (int)currentCell.transform.position.z + q);
                    //Debug.Log($"{currentCell.name}'s new location is {currentCell.transform.position}");
                }
            }
            if (its % 2 == 0) //if q is odd,
                z -= (its + 2);
            else if (its % 2 != 0) // if q is even,
                z += (its + 2);
            twoIterationsHavePassed = !twoIterationsHavePassed;
            if (twoIterationsHavePassed)
            {
                if (q > 0)
                    q++;
                else
                    q--;
            }
        }
    }
    private void ResetCellsPosition()
    {
        ClearEdges();
        for (int z = 1; z < mapDepth - 1; z++)
        {
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    GameObject currentCell = GetCellInstance(new Vector3Int(x, y, z));
                    Script_CellPrefab cellScript = currentCell.GetComponent<Script_CellPrefab>();
                    currentCell.transform.position = cellScript.spawnLocation;
                }
            }
        }
        /*for (int y = mapHeight - 2; y >= 1; y--)
        {
            string row = "";
            for (int x = 1; x < mapWidth - 1; x++)
            {
                row += nearbyBoard[y, x, 1] + " ";
            }
            Debug.Log(row);
        }*/
    }
}