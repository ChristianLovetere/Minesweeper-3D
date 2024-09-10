using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Script_CameraControl : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float sensitivity = 10.0f;
    public float rotationSpeed = 5.0f;
    public float easingFactor = 10f;
    public float zoomSpeed = 1000f;
    public Vector3 rotationCenter;

    private Vector3 targetPosition;
    private Vector3 initialPosition;
    private Quaternion initialRotation = Quaternion.Euler(0, 0, 0);

    private Vector3 previousPosition;

    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;
    void Start()
    {
        rotationCenter = Script_EasyLevel.mapCenterVect;
        initialPosition = new Vector3(rotationCenter.x, rotationCenter.y, -10);


        transform.position = initialPosition;
        transform.rotation = initialRotation;

    }

    void Update()
    {
        if (Script_PauseMenu.isPaused && !Script_PauseMenu.isReviewing)
            return;

        // Rotate camera around the specified center 
        if (Input.GetMouseButtonDown(2)) // Middle mouse button
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 cameraPosOnPrevFrame = cam.transform.position;
            Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

            cam.transform.position = target.position; //origin position

            cam.transform.Rotate(new Vector3(x: 1, y: 0, z: 0), angle: direction.y * 180);
            cam.transform.Rotate(new Vector3(x: 0, y: 1, z: 0), angle: -direction.x * 180, relativeTo: Space.World);
            cam.transform.Translate(new Vector3(x: 0, y: 0, z: -Vector3.Distance(target.position, cameraPosOnPrevFrame)));

            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        if (Vector3.Distance(target.position, transform.position) > 2 || zoomInput < 0) 
            transform.Translate(Vector3.forward * zoomInput * zoomSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
        float horizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Translate(horizontal, vertical, 0);

        /*if (Input.GetMouseButton(2) || Input.GetMouseButtonDown(2))
        {
            target.transform.parent = null;
        }
        else if (Input.GetAxis("Horizontal") != 0)
        {
            target.transform.parent = transform;
        }
        else if (Input.GetAxis("Vertical") != 0)
        {
            target.transform.parent = transform;
        }
        else if (Input.GetAxis("Horizontal") == 0)
        {
            target.transform.parent = null;
        }
        else if (Input.GetAxis("Vertical") == 0)
        {
            target.transform.parent = null;
        }*/


        /*if (Input.GetKey(KeyCode.S))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        }*/
    }
}