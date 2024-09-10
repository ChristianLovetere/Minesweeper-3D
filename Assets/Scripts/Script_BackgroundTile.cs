using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;
public class Script_BackgroundTile : MonoBehaviour
{
    private float moveSpeed = 50f;
    private Image imageComponent;
    public bool persistent = false;
    // Start is called before the first frame update
    void Start()
    {
         Sprite[] spriteList = { Resources.Load<Sprite>("Materials/Texture_Cell0"),    Resources.Load<Sprite>("Materials/Texture_Cell1"),
                                 Resources.Load<Sprite>("Materials/Texture_Cell2"),    Resources.Load<Sprite>("Materials/Texture_Cell3"),
                                 Resources.Load<Sprite>("Materials/Texture_Cell4"),    Resources.Load<Sprite>("Materials/Texture_Cell5"),
                                 Resources.Load<Sprite>("Materials/Texture_Cell6"),    Resources.Load<Sprite>("Materials/Texture_Cell7"),
                                 Resources.Load<Sprite>("Materials/Texture_Cell8"),    Resources.Load<Sprite>("Materials/Texture_CellBomb"),
                                 Resources.Load<Sprite>("Materials/Texture_CellFlag"), Resources.Load<Sprite>("Materials/Texture_CellBlank")};

        imageComponent = GetComponent<Image>();
        imageComponent.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        PickSprite(spriteList);
    }

    private void PickSprite(Sprite[] spriteArr)
    {
        int randomIndex = Random.Range(0, spriteArr.Length*2);
        if (randomIndex >= spriteArr.Length)
            randomIndex = 0;
        imageComponent.sprite = spriteArr[randomIndex];
    }
    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < -100 && !persistent)
        {
            Destroy(gameObject);
            return;
        }
        // Calculate the movement vector (down and left)
        Vector3 moveDirection = new Vector3(-1f, -1f, 0f);

        // Normalize the vector to maintain consistent speed
        moveDirection.Normalize();

        // Move the GameObject
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
}
