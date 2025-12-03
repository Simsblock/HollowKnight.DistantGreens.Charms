using DistantGreensCharms.Helper;
using UnityEngine;

namespace DistantGreensCharms.HUDElements;

public class MossMaskHUD : AHUDElement
{
    public override string Name => "Moss-Mask";
    public override string SpritePath => "HUDIcons.MossMask_0";
    public override bool Visible { get; set; } = false;
    public override RectTransformAttributes LocationAttributes => new()
    {
        anchorMin = new Vector2(0f,0f),
        anchorMax = new Vector2(0f,0f),
        pivot = new Vector2(0f,0f),
        anchoredPosition = new Vector2(0f,0f),
        sizeDelta = new Vector2(0f,0f),
        localScale = new Vector2(0f,0f)
    };
    
    public string BrokenSpritePath => "HUDIcons.MossMask_1";
    public override void SetVisibility(bool visibility)
    {
        if(!visibility)
        {
            Icon.sprite = SpriteManager.Get(BrokenSpritePath);
            new WaitForSeconds(0.5f);
        }
        else
        {
            Icon.sprite = SpriteManager.Get(SpritePath);
        }
        base.SetVisibility(visibility);
    }
}