using System.Collections;
using System.Collections.Generic;
using DistantGreensCharms.Helper;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace DistantGreensCharms.HUDElements;

public class MossMaskHUD : AHUDElement
{
    public static MossMaskHUD Instance = new();
    public override string Name => "Moss-Mask";
    public override string DefaultSpritePath => "HUDIcons.MossMask_0";
    
    public override float X => -2.15f; 
    public override float Y => 0.3f; 
    public override float Scale => 1f;
    public HUDAnimation BreakSpriteAnimation {get; set;}

    public void UpdateSpriteState(bool charged)
    {
        if (BreakSpriteAnimation is null) 
            BreakSpriteAnimation = new(["HUDIcons.MossMask_1", "HUDIcons.MossMask_2", "HUDIcons.MossMask_3"], GameObject);
    
        if (charged)
        {
            SpriteRenderer.sprite = SpriteManager.Get(DefaultSpritePath);
            SetVisibility(true);
        }
        else
        {
            BreakSpriteAnimation.StartAnimation();
        }
    }
}
