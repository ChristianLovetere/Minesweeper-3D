using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using static UnityEditor.GridPalette;

public class Script_CellPrefab : MonoBehaviour
{
    public event Action<Vector3Int> OnCellClicked;
    public event Action<Vector3Int> OnCellRightClicked;
    public event Action<Vector3Int> OnCellLeftClickHeld;
    public event Action<Vector3Int> OnCellExit;

    // Reference to the Renderer component
    private Renderer cellRenderer;

    private Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    // Create a material instance for each cell
    private Material cellMaterialInstance;
    public Vector3 spawnLocation;
    private void Start()
    {
        // Get the Renderer component attached to this cell
        cellRenderer = GetComponent<Renderer>();

        // Create an instance of the base material
        cellMaterialInstance = new Material(cellRenderer.sharedMaterial);

        // Assign the material instance to the cell
        cellRenderer.material = cellMaterialInstance;
        spawnLocation = transform.position;
    }

    public void UpdateMaterial(string materialIndex)
    {

        Dictionary<string, Material> materialsDict = new Dictionary<string, Material>
        {
            { "0", Resources.Load<Material>("Materials/Material_Cell0") }, {"1", Resources.Load<Material>("Materials/Material_Cell1") },
            { "2", Resources.Load<Material>("Materials/Material_Cell2") }, {"3", Resources.Load<Material>("Materials/Material_Cell3") },
            { "4", Resources.Load<Material>("Materials/Material_Cell4") }, {"5", Resources.Load<Material>("Materials/Material_Cell5") },
            { "6", Resources.Load<Material>("Materials/Material_Cell6") }, {"7", Resources.Load<Material>("Materials/Material_Cell7") },
            { "8", Resources.Load<Material>("Materials/Material_Cell8") }, { "9", Resources.Load<Material>("Materials/Material_Cell9") },
            { "10", Resources.Load<Material>("Materials/Material_Cell10") }, { "11", Resources.Load<Material>("Materials/Material_Cell11") },
            { "12", Resources.Load<Material>("Materials/Material_Cell12") }, { "13", Resources.Load<Material>("Materials/Material_Cell13") },
            { "14", Resources.Load<Material>("Materials/Material_Cell14") }, { "15", Resources.Load<Material>("Materials/Material_Cell15") },
            { "16", Resources.Load<Material>("Materials/Material_Cell16") }, { "17", Resources.Load<Material>("Materials/Material_Cell17") },
            { "18", Resources.Load<Material>("Materials/Material_Cell18") }, { "19", Resources.Load<Material>("Materials/Material_Cell19") },
            { "20", Resources.Load<Material>("Materials/Material_Cell20") }, { "21", Resources.Load<Material>("Materials/Material_Cell21") },
            { "22", Resources.Load<Material>("Materials/Material_Cell22") }, { "23", Resources.Load<Material>("Materials/Material_Cell23") },
            { "24", Resources.Load<Material>("Materials/Material_Cell24") }, { "25", Resources.Load<Material>("Materials/Material_Cell25") },
            { "26", Resources.Load<Material>("Materials/Material_Cell26") },
            {"bomb", Resources.Load<Material>("Materials/Material_CellBomb") }, { "flag", Resources.Load<Material>("Materials/Material_CellFlag") },
            {"blank", Resources.Load<Material>("Materials/Material_CellBlank") }, { "start", Resources.Load<Material>("Materials/Material_CellStart") }

        };

        /*if (materialsList != null && materialIndex >= 0 && materialIndex < materialsList.Length)
        {
            cellMaterialInstance = materialsList[materialIndex];
            cellRenderer.material = cellMaterialInstance;
        }*/

        if (materialsDict != null)
        {
            cellMaterialInstance = materialsDict[materialIndex];
            cellRenderer.material = cellMaterialInstance;
            Debug.Log($"Setting material to {cellMaterialInstance}");
        }
    }

    private void OnMouseOver()
    {
        if (!Script_PauseMenu.isPaused)
        {
            GetComponent<MeshRenderer>().material.color = hoverColor;

            if (Input.GetMouseButtonUp(0)) //let go of left click:
            {
                /*Vector3 cellPosition = transform.position;
                int row = (int)cellPosition.y;
                int col = (int)cellPosition.x;
                int dep = (int)cellPosition.z;*/
                string cellName = gameObject.name;
                string[] parts = cellName.Split('_');

                int y = int.Parse(parts[1]);
                int x = int.Parse(parts[2]);
                int z = int.Parse(parts[3]);

                OnCellClicked?.Invoke(new Vector3Int(x, y, z));
                //OnCellClicked?.Invoke(new Vector3Int(col, row, dep));
            }
            if (Input.GetMouseButtonUp(1)) //let go of right click:
            {
                /*Vector3 cellPosition = transform.position;
                int row = (int)cellPosition.y;
                int col = (int)cellPosition.x;
                int dep = (int)cellPosition.z;

                OnCellRightClicked?.Invoke(new Vector3Int(col, row, dep));*/

                string cellName = gameObject.name;
                string[] parts = cellName.Split('_');

                int y = int.Parse(parts[1]);
                int x = int.Parse(parts[2]);
                int z = int.Parse(parts[3]);

                OnCellRightClicked?.Invoke(new Vector3Int(x, y, z));
            }
            if (Input.GetMouseButton(0))
            {
                /*Vector3 cellPosition = transform.position;
                int row = (int)cellPosition.y;
                int col = (int)cellPosition.x;
                int dep = (int)cellPosition.z;

                OnCellLeftClickHeld?.Invoke(new Vector3Int(col, row, dep));*/

                string cellName = gameObject.name;
                string[] parts = cellName.Split('_');

                int y = int.Parse(parts[1]);
                int x = int.Parse(parts[2]);
                int z = int.Parse(parts[3]);

                OnCellLeftClickHeld?.Invoke(new Vector3Int(x, y, z));
            }
        }
    }

    private void OnMouseExit()
    {
        GetComponent<MeshRenderer>().material.color = Color.white;

        /*Vector3 cellPosition = transform.position;
        int row = (int)cellPosition.y;
        int col = (int)cellPosition.x;
        int dep = (int)cellPosition.z;

        OnCellExit?.Invoke(new Vector3Int(col, row, dep));*/
        string cellName = gameObject.name;
        string[] parts = cellName.Split('_');

        int y = int.Parse(parts[1]);
        int x = int.Parse(parts[2]);
        int z = int.Parse(parts[3]);

        OnCellExit?.Invoke(new Vector3Int(x, y, z));
    }
}
