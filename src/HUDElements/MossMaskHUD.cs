using DistantGreensCharms.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace DistantGreensCharms.HUDElements;

public class MossMaskHUD : AHUDElement
{
    public override string Name => "Moss-Mask";
    public override string SpritePath => "HUDIcons.MossMask_0";
    
    public override float X => -5f;
    public override float Y => 1f;

    public string ChargedSpritePath => "HUDIcons.MossMask_0";
    public string BrokenSpritePath => "HUDIcons.MossMask_1";

    public void UpdateSpriteState(bool charged)
    {
        if (charged) SpriteRenderer.sprite = SpriteManager.Get(ChargedSpritePath);
        else SpriteRenderer.sprite = SpriteManager.Get(BrokenSpritePath);
    }

    public void Hook()
    {
        HUDManager.Add(this);
    }
}