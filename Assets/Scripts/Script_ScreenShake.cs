using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_ScreenShake : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 0.1f;

    private Vector3 beforeShakePosition;
    private float currentShakeTime = 0f;

    private void Start()
    {
        beforeShakePosition = transform.position; // Store original camera position
    }

    private void Update()
    {
        //Debug.Log($"reference pos:{beforeShakePosition}, currentShakeTime: {currentShakeTime}");
        if (currentShakeTime <= 0)
            beforeShakePosition = transform.position; // Store original camera position
        if (currentShakeTime > 0)
        {
            Vector3 shakeOffset = UnityEngine.Random.insideUnitSphere * shakeIntensity;
            transform.position = beforeShakePosition + shakeOffset;
            currentShakeTime -= Time.deltaTime;
        }
        /*else
        {
            transform.position = beforeShakePosition; // Reset camera position
        }*/
    }

    public void StartShake()
    {
        currentShakeTime = shakeDuration; // Call this method to start screen shake
    }
}
