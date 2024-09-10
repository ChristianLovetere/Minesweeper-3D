using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;

public class Script_EasyLevelBackground : MonoBehaviour
{
    int topLeftX = -940;
    int topLeftY = 640;
    public GameObject BackgroundTilePrefab;

    private bool EN = true;
    int setNumber = 0;
    private void Start()
    {
        GameObject firstTile = Instantiate(BackgroundTilePrefab, new Vector3(topLeftX, topLeftY, transform.position.z), Quaternion.identity);
        firstTile.transform.SetParent(transform);
        firstTile.name = "topLeftTile";

        Script_BackgroundTile BackgroundTileScript = firstTile.GetComponent<Script_BackgroundTile>();
        if (BackgroundTileScript != null)
        {
            BackgroundTileScript.persistent = true;
        }
        for (int i = 0; i < 22; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                GameObject newTile = Instantiate(BackgroundTilePrefab, new Vector3(topLeftX + (100 * i), topLeftY + (-100 * j), transform.position.z), Quaternion.identity);
                newTile.transform.SetParent(transform);
            }
        }
    }
    private void Update()
    {
        GameObject firstTile = GameObject.Find("topLeftTile");

        if (((firstTile.transform.position.y > 0 && (int)firstTile.transform.position.y % 100 == 40) ||
             (firstTile.transform.position.y < 0 && Mathf.Abs((int)firstTile.transform.position.y) % 100 == 60)) &&
             (int)firstTile.transform.position.y != topLeftY && EN)
        {
            SpawnNewRow();
            EN = false;
        }

        if ((firstTile.transform.position.y > 0 && (int)firstTile.transform.position.y % 100 == 90) ||
            (firstTile.transform.position.y < 0 && Mathf.Abs((int)firstTile.transform.position.y) % 100 == 10))
        {
            EN = true;
        }
    }
    private void SpawnNewRow()
    {
        int instanceNumber = 0;
        for (int i = 0; i < 22; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                if ((i == 0 && j == 0) || (j > 0 && i < 21))
                    continue;
                GameObject newTile = Instantiate(BackgroundTilePrefab, new Vector3(topLeftX + (100 * i), topLeftY + (-100 * j), transform.position.z), Quaternion.identity);
                newTile.transform.SetParent(transform);
                newTile.name = $"BackgroundTile{setNumber}_{instanceNumber}";
            }
            instanceNumber++;
        }
        setNumber++;
    }
}
