using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_SimpleMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maximumDrift;
    [SerializeField] private string moveDir;
    private float moveSpeedDeltaTime;
    private Vector3 initialPosition;
    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 2f;
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
        else MoveSome(moveDir);
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
}
