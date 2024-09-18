using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class Script_TutorialElement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maximumDrift;
    [SerializeField] private string moveDir;
    [SerializeField] private KeyCode[] keysToCheck;
    [SerializeField] private GameObject nextTutorialElement;
    private GameObject tutorialParent;
    [SerializeField] private Vector3 spawnLocation;

    private GameObject checkMark;
    private float moveSpeedDeltaTime;
    private Vector3 initialPosition;
    bool tutorialElementHasBeenChecked = false;

    // Start is called before the first frame update
    void Start()
    {
        checkMark = Resources.Load<GameObject>("Prefabs/GameObject_CheckMark");
        moveSpeedDeltaTime = Time.deltaTime * moveSpeed;
        initialPosition = transform.position;
        tutorialParent = GameObject.Find("TUTORIAL:");
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(initialPosition, transform.position) > maximumDrift)
            transform.position = initialPosition;
        else if (!tutorialElementHasBeenChecked)
            MoveSome(moveDir);

        if (AnyRequestedKeyIsPressed() && tutorialElementHasBeenChecked == false)
        {
            tutorialElementHasBeenChecked = true;
            MoveTutorialForward();
        }
    }

    bool AnyRequestedKeyIsPressed()
    {
        foreach (KeyCode key in keysToCheck)
        {
            if (Input.GetKeyDown(key))
                return true;
        }
        return false;
    }

    void MoveTutorialForward()
    {
        RevealCheckMark();
        //RevealCheckMark invokes HandleCheckMarkFadeFinished at the end
    }

    void HandleCheckMarkFadeFinished()
    {
        if (nextTutorialElement != null)
        {
            GameObject NTE = Instantiate(nextTutorialElement, spawnLocation, Quaternion.identity, tutorialParent.transform);
            //NTE.transform.position = spawnLocation;
            //NTE.transform.SetParent(uiCanvas.transform);

        }
        else Debug.Log("no GameObject given for next tutorial element");
    }
    void MoveSome(string dir)
    {
        switch (dir)
        {
            case "+x":
                transform.Translate(new Vector3(moveSpeedDeltaTime, 0, 0));
                break;
            case "-x":
                transform.Translate(new Vector3(-moveSpeedDeltaTime, 0, 0));
                break;
            case "+y":
                transform.Translate(new Vector3(0, moveSpeedDeltaTime, 0));
                break;
            case "-y":
                transform.Translate(new Vector3(0, -moveSpeedDeltaTime, 0));
                break;
            case "+z":
                transform.Translate(new Vector3(0, 0, moveSpeedDeltaTime));
                break;
            case "-z":
                transform.Translate(new Vector3(0, 0, -moveSpeedDeltaTime));
                break;
            default:
                break;
        }
    }

    void RevealCheckMark()
    {
        GameObject newCheck = Instantiate(checkMark);
        newCheck.transform.position = transform.position;
        newCheck.transform.SetParent(transform);
        Script_CheckMark CmScript = newCheck.GetComponent<Script_CheckMark>();
        if(CmScript != null)
        {
            CmScript.OnCheckMarkFadeFinished += HandleCheckMarkFadeFinished;
        }

    }
}
