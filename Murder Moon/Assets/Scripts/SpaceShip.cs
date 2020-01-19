using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    public string SpriteSheetName;
    public SpriteRenderer Sprite;
    Dictionary<string, Sprite> spriteSheet;

    private void Start()
    {
        Sprite = GetComponent<SpriteRenderer>();


        if(Sprite == null)
        {
            Sprite = GetComponentInChildren<SpriteRenderer>();
        }

        SetUpSpriteSheet();
    }



    // Runs after the animation has done its work
    private void LateUpdate()
    {
        try
        {
            Sprite.sprite = spriteSheet[Sprite.sprite.name];
        }
        catch
        {
            Debug.LogError("ERROR - Missing frame - " + Sprite.sprite.name);
        }
    }


    private void SetUpSpriteSheet()
    {
        if (SpriteSheetName.Count() != 0 && SpriteSheetName != null)
        {
            var sprites = Resources.LoadAll<Sprite>(this.SpriteSheetName);

            try
            {
                spriteSheet = sprites.ToDictionary(x => x.name, x => x);
                Sprite.sprite = spriteSheet[Sprite.sprite.name];
            }
            catch
            {
                Debug.LogError(gameObject.name + " - ERROR - Missing frame - " + Sprite.sprite.name);
            }

        }
    }



}
