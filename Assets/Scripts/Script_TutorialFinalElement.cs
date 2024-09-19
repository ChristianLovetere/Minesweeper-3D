using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_TutorialFinalElement : MonoBehaviour
{
    
    public GameObject[] ListOfTutorialPrefabs;
    public GameObject tip;
    private GameObject tutorialParent;
    // Start is called before the first frame update
    void Start()
    {
        tutorialParent = GameObject.Find("TUTORIAL:");
        int x;
        int y;

        for (int i = 0; i < ListOfTutorialPrefabs.Length; i++)
        {
            if (i < 5) x = 200;
            else x = Screen.width - 200;

            y = Screen.height - 100 - i*200;
            if (i >= 5) y += 1000;

            GameObject prefab = ListOfTutorialPrefabs[i];
            GameObject latestPrefab = Instantiate(prefab, new Vector3(x,y,0), Quaternion.identity, tutorialParent.transform);
            latestPrefab.transform.localScale = new Vector3(1.2f, 1.2f, 1);
            Script_TutorialElement TutElComponent = latestPrefab.GetComponent<Script_TutorialElement>();
            TutElComponent.iShouldSpawnNext = false;
        }
        Instantiate(tip, new Vector3(Screen.width / 2, 100, 0), Quaternion.identity, tutorialParent.transform);
        Destroy(gameObject);
    }
}
