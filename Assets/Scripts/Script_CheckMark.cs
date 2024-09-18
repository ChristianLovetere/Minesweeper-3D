using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Script_CheckMark : MonoBehaviour
{
    public event Action OnCheckMarkFadeFinished;
    public float fadeDuration = 2.0f; // Duration of the fade in seconds
    private CanvasRenderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<CanvasRenderer>();
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            Debug.Log(alpha);
            objectRenderer.SetAlpha(alpha);
            yield return null;
        }
        Destroy(transform.parent.gameObject);
        OnCheckMarkFadeFinished?.Invoke();
    }
}
