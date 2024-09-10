using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Script_FlagCount : MonoBehaviour
{
    public TextMeshProUGUI FlagCountText;

    private int FlagCount;
    // Start is called before the first frame update
    void Start()
    {
        Script_EasyLevel.OnFlagAdded += HandleFlagAdded;
        Script_EasyLevel.OnFlagRemoved += HandleFlagRemoved;
        Script_EasyLevel.OnBombsAllPlaced += SetBombAmount;
        Debug.Log("hello 1");
    }

    // Update is called once per frame
    void Update()
    {
        FlagCountText.text = FlagCount.ToString();
    }

    void HandleFlagAdded()
    {
        FlagCount--;
    }

    void HandleFlagRemoved()
    {
        FlagCount++;
    }

    void SetBombAmount(int totalBombs)
    {
        FlagCount = totalBombs;
        
    }
}