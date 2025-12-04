using DistantGreensCharms.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace DistantGreensCharms.HUDElements;

public class MossMaskHUD : AHUDElement
{
    public override string Name => "Moss-Mask";
    public override string DefaultSpritePath => "HUDIcons.MossMask_0";
    
    public override float X => -5f; 
    public override float Y => 1f; 
    public override float Scale => 0.7f; 

    public string BrokenSpritePath => "HUDIcons.MossMask_1";

    public void UpdateSpriteState(bool charged)
    {
        if (charged) SpriteRenderer.sprite = SpriteManager.Get(DefaultSpritePath);
        else SpriteRenderer.sprite = SpriteManager.Get(BrokenSpritePath);
    }
}