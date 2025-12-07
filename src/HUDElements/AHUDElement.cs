using System;
using System.Collections;
using DistantGreensCharms.Helper;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace DistantGreensCharms.HUDElements;

public abstract class AHUDElement
{
    // CREATE STATIC INSTANCE OF HUD IN DERIVATIVE
    public abstract string Name { get; }
    public virtual string DataName => Name.Replace(" ", "_") ?? default;
    public abstract string DefaultSpritePath { get; }
    public virtual bool Visible => SpriteRenderer.enabled;

    public virtual GameObject OverrideParent { get; } = null;

    public abstract float X { get; }
    public abstract float Y { get; }
    public virtual float Z { get; private set; } = 0f; //No need to change this.
    public virtual float Scale { get; private set; } = 1f; //Relative Scale of Sprite
    //public virtual int SortingOrder { get; private set; } = 5; //Standard for all HUD elements in HK is 5 //Seems irrelevant
    
    public GameObject GameObject { get; set; } //Assigned at Runtime
    public SpriteRenderer SpriteRenderer { get; set; }

    public virtual void Hook()
    {
        HUDManager.Add(this);
    }

    public virtual void SetVisibility(bool visibility)
    {
        if(SpriteRenderer is null) SpriteRenderer = GameObject.GetComponent<SpriteRenderer>();
        DistantGreensCharms.Instance.Log("SetVisbility: "+(SpriteRenderer is null).ToString());
        SpriteRenderer.enabled = visibility;
    }
}