using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_CubeBackground : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position = Script_EasyLevel.mapCenterVect;
    }
}
