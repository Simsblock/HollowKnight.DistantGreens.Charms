using DistantGreensCharms.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace DistantGreensCharms.HUDElements;

public class MossMaskHUD : AHUDElement
{
    public override string Name => "Moss-Mask";
    public override string SpritePath => "CharmIcons.MossMask";
    
    public override float X => 0f;
    public override float Y => 0f;

    //public override string ParentName => "Health";

    public string BrokenSpritePath => "CharmIcons.MossMask";
    public override void SetVisibility(bool visibility)
    {
        DistantGreensCharms.Instance.Log("GameObject is null: "+(GameObject == null).ToString());
        DistantGreensCharms.Instance.Log("Icon is null: "+(SpriteRenderer == null).ToString());
        DistantGreensCharms.Instance.Log("Icon in go is null: "+(GameObject.GetComponent<Image>() == null).ToString());
        if(!visibility)
        {
            SpriteRenderer.sprite = SpriteManager.Get(BrokenSpritePath);
            new WaitForSeconds(0.5f);
        }
        else
        {
            SpriteRenderer.sprite = SpriteManager.Get(SpritePath);
        }
        base.SetVisibility(visibility);
    }

    public void Hook()
    {
        HUDManager.Add(this);
    }
}