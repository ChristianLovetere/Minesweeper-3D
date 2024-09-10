/*using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using TMPro;
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
    public static Vector3 mapCenterVect = new Vector3((mapWidth - 1f) / 2f, (mapHeight - 1f) / 2f, 0f);
    public int numberOfBombs = 0;

    private int nonBombsRevealed = 0;
    private TMP_Text seedText;
    [SerializeField] private GameObject screenTintingPanel;
    public Script_ScreenShake screenShaker;
    public Script_GameOverManager gameOverScript;
    private Vector2Int startLocation;
    private bool hasWon = false;

    [SerializeField] private AudioClip[] UIPopSFX;
    [SerializeField] private AudioClip[] explosionSFX;
    [SerializeField] private AudioClip[] winSFX;

    [SerializeField] private Canvas UICanvas;
    [SerializeField] private GameObject GameEndingScreen;
    [SerializeField] private Script_PauseMenu PauseManager;
    [SerializeField] private Script_CornerButton CornerButton;
    [SerializeField] private Script_SceneSwitcher SceneSwitcher;
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

    Cell[,] board = new Cell[mapHeight, mapWidth];
    int[,] nearbyBoard = new int[mapHeight, mapWidth];

    public GameObject cellPrefab;
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

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (i == 0 || j == 0 || i == mapWidth - 1 || j == mapHeight - 1) //if outside of map
                {
                    board[j, i] = new Cell(false, true); //construct with IsBomb false, IsEdge true

                    //GameObject NewEdgeCell = Instantiate(EdgeCellPrefab, new Vector3(i, j, 0), Quaternion.identity); for 3d maybe
                }
                else //if inside map
                {
                    int bombOrNot = UnityEngine.Random.Range(0, 9); //bomb by random
                    if (bombOrNot > 7) //if bomb
                    {
                        numberOfBombs++;

                        board[j, i] = new Cell(true, false); //construct with IsBomb true, IsEdge false
                        for (int k = i - 1; k < i + 2; k++) //for 3x3 of adjacent locations centered around bomb,
                            for (int l = j - 1; l < j + 2; l++)
                            {
                                if (l == 0 || l == mapHeight - 1 || k == 0 || k == mapWidth - 1 || (l == j && k == i)) //if its an edge or the center of the 3x3,
                                    continue;
                                else
                                    nearbyBoard[l, k]++; //add 1 to all locations next to the center (bomb)
                            }
                    }
                    else
                        board[j, i] = new Cell(false, false); //construct with IsBomb false, IsEdge false

                    GameObject newCell = Instantiate(cellPrefab, new Vector3(i, j, 0), Quaternion.Euler(-90, -90, 0));
                    newCell.name = $"Cell_{j}_{i}";

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
    }
    Vector2Int FindStartSpot()
    {
        int iterations = 0;
        while (true)
        {
            int xPos = UnityEngine.Random.Range(1, mapWidth - 2);
            int yPos = UnityEngine.Random.Range(1, mapHeight - 2);

            if (nearbyBoard[yPos, xPos] == 0 && !board[yPos, xPos].IsBomb)
                return new Vector2Int(xPos, yPos);

            iterations++;
            //Debug.Log($"Finding start pos took {iterations} tries");
        }
    }
    void HandleCellClicked(Vector2Int gridPosition)
    {

        //If its flagged, return
        if (board[gridPosition.y, gridPosition.x].IsFlagged)
            return;

        if (nearbyBoard[gridPosition.y, gridPosition.x] != 0 || gridPosition == startLocation)
            Script_SFXManager.instance.PlayRandomSFXClip(UIPopSFX, transform, 1f);

        //If its seen, player is trying to quick reveal nearby flags, or holding leftclick to view unrevealed nearby cells
        if (board[gridPosition.y, gridPosition.x].Seen && nearbyBoard[gridPosition.y, gridPosition.x] != 0)
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
        if (board[gridPosition.y, gridPosition.x].IsBomb)
        {
            Reveal(gridPosition);
            Script_SFXManager.instance.PlayRandomSFXClip(explosionSFX, transform, 1f);
            GameLost();
        }

        //if its a safe space, recursively reveal all nearby safe spaces and their nearby numbered spaces
        else if (nearbyBoard[gridPosition.y, gridPosition.x] == 0)
        {
            RecursiveReveal(gridPosition);
        }
        //if its a number spot
        else
        {
            Reveal(gridPosition);
        }
    }

    void HandleCellRightClicked(Vector2Int gridPosition)
    {
        GameObject cellToFlag = GetCellInstance(gridPosition);
        if (cellToFlag != null)
        {
            Script_CellPrefab cellComponent = cellToFlag.GetComponent<Script_CellPrefab>();
            if (cellComponent != null)
            {
                if (board[gridPosition.y, gridPosition.x].IsFlagged)
                {
                    board[gridPosition.y, gridPosition.x].IsFlagged = false;
                    cellComponent.UpdateMaterial(11);

                    OnFlagRemoved?.Invoke();
                }
                else if (!board[gridPosition.y, gridPosition.x].Seen)
                {
                    board[gridPosition.y, gridPosition.x].IsFlagged = true;
                    cellComponent.UpdateMaterial(10);

                    OnFlagAdded?.Invoke();
                }

            }
        }
    }

    private void RevertColor(Vector2Int gridPosition)
    {
        for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
        {
            for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
            {
                if (board[y, x].IsEdge || (y == gridPosition.y && x == gridPosition.x) || board[y, x].Seen)
                    continue;
                GameObject cellHere = GetCellInstance(new Vector2Int(x, y));
                cellHere.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }
    private void HandleCellLeftClickHeld(Vector2Int gridPosition)
    {
        if (!board[gridPosition.y, gridPosition.x].Seen)
            return;
        for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
        {
            for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
            {
                if (board[y, x].IsEdge || (y == gridPosition.y && x == gridPosition.x) || board[y, x].Seen)
                    continue;
                GameObject cellHere = GetCellInstance(new Vector2Int(x, y));
                cellHere.GetComponent<MeshRenderer>().material.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }
        }
    }

    private void HandleCellExited(Vector2Int gridPosition)
    {
        if (!Script_PauseMenu.isPaused)
            RevertColor(gridPosition);
    }

    private void RecursiveReveal(Vector2Int gridPosition)
    {
        if (board[gridPosition.y, gridPosition.x].IsEdge)
            return;

        for (int i = gridPosition.x - 1; i < gridPosition.x + 2; i++)
            for (int j = gridPosition.y - 1; j < gridPosition.y + 2; j++)
            {
                if (!board[j, i].IsEdge && !board[j, i].Seen && !board[j, i].IsBomb)
                {

                    Reveal(new Vector2Int(i, j));
                    if (FindNearbyMines(j, i) == 0)
                    {
                        RecursiveReveal(new Vector2Int(i, j));
                    }
                }
            }
    }

    private void Reveal(Vector2Int gridPosition)
    {
        GameObject cellToReveal = GetCellInstance(gridPosition);
        board[gridPosition.y, gridPosition.x].Seen = true;

        if (!board[gridPosition.y, gridPosition.x].IsBomb)
            nonBombsRevealed++;

        Debug.Log(nonBombsRevealed);
        if (cellToReveal != null)
        {
            Script_CellPrefab cellComponent = cellToReveal.GetComponent<Script_CellPrefab>();
            if (cellComponent != null)
            {
                if (board[gridPosition.y, gridPosition.x].IsBomb)
                    cellComponent.UpdateMaterial(9);
                else
                {
                    int nearbyMines = FindNearbyMines(gridPosition.y, gridPosition.x);
                    cellComponent.UpdateMaterial(nearbyMines);
                }
            }
        }
        else
            Debug.Log("Reveal: Cell could not be found");
    }

    private void RevealNearby(Vector2Int gridPosition)
    {
        for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
        {
            for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
            {
                if (board[y, x].IsEdge || (y == gridPosition.y && x == gridPosition.x) || board[y, x].Seen || board[y, x].IsFlagged)
                    continue;

                if (nearbyBoard[y, x] != 0)
                    Reveal(new Vector2Int(x, y));
                else
                    RecursiveReveal(new Vector2Int(x, y));
            }
        }
    }

    private int JudgeQuickFlags(Vector2Int gridPosition)
    {
        int badFlags = 0;
        int numFlags = 0;
        for (int x = gridPosition.x - 1; x <= gridPosition.x + 1; x++)
        {
            for (int y = gridPosition.y - 1; y <= gridPosition.y + 1; y++)
            {
                if (board[y, x].IsEdge || (y == gridPosition.y && x == gridPosition.x) || board[y, x].Seen)
                    continue;

                if (board[y, x].IsFlagged)
                    numFlags++;
                if (board[y, x].IsBomb != board[y, x].IsFlagged)
                    badFlags = 1;
            }
        }
        if (numFlags != nearbyBoard[gridPosition.y, gridPosition.x])
            return -1; //dont do anything
        else
            return badFlags;
    }
    GameObject GetCellInstance(Vector2Int gridPosition)
    {
        string cellName = $"Cell_{gridPosition.y}_{gridPosition.x}";
        return GameObject.Find(cellName);
    }

    private int FindNearbyMines(int y, int x)
    {
        if (board[y, x].IsBomb)
            return 9;
        else return nearbyBoard[y, x];
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
        if (hasWon) return;

        if ((mapWidth - 2) * (mapHeight - 2) - numberOfBombs == nonBombsRevealed)
        {
            hasWon = true;
            Script_SFXManager.instance.PlayRandomSFXClip(winSFX, transform, 1.75f);
            GameWon();
        }
    }
}*/