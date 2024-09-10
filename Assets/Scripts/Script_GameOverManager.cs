using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class Script_GameOverManager : MonoBehaviour
{
    public Vector3 targetPosition; 
    public float movementTime = 1f; 

    private Vector3 originalPosition;
    private float elapsedTime = 0f;

    public GameObject tintingPanel;
    private void Start()
    {
        originalPosition = transform.position;
        targetPosition = new Vector3(transform.position.x, transform.position.y - 1220, transform.position.z);
        LowerAssets();
    }

    private IEnumerator MoveObject(Vector3 start, Vector3 end)
    {
        while (elapsedTime < movementTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / movementTime);
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }
    public void LowerAssets()
    {
        StartCoroutine(MoveObject(originalPosition, targetPosition));
    }

    public void MinimalizeMenu()
    {
        gameObject.SetActive(false);
        tintingPanel.SetActive(false);
    }
}
