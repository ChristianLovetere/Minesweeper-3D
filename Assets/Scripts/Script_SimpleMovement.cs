using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_SimpleMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maximumDrift;
    [SerializeField] private string moveDir;
    [SerializeField] private GameObject checkMark;
    [SerializeField] private KeyCode keyToCheck;
    [SerializeField] private int tutorialStage;

    private float moveSpeedDeltaTime;
    private Vector3 initialPosition;
    bool tutorialElementHasBeenChecked = false;
    private GameObject[] listOfTutorialObjects = null;
    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 50f;
        moveSpeedDeltaTime = Time.deltaTime * moveSpeed;
        moveDir = "+x";
        initialPosition = transform.position;
        maximumDrift = 150f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(initialPosition, transform.position) > maximumDrift)
            transform.position = initialPosition;
        else if (!tutorialElementHasBeenChecked)
            MoveSome(moveDir);

        if (Input.GetKeyDown(keyToCheck) && tutorialElementHasBeenChecked == false)
        {
            tutorialElementHasBeenChecked = true;
            MoveTutorialForward(tutorialStage);
        }
    }

    void MoveTutorialForward(int tutorialStage)
    {
        RevealCheckMark();
        if (listOfTutorialObjects[tutorialStage] != null)
        {
            Instantiate(listOfTutorialObjects[tutorialStage]);
        }
        else Debug.Log("no GameObject at that position in the array");
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
    }
}
