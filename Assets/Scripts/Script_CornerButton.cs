using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_CornerButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
