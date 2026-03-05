using DistantGreensCharms.Helper;
using UnityEngine;

namespace DistantGreensCharms.HUDElements;

public abstract class AHUDRowElement
{
    public abstract string Name { get; }
    public virtual string DataName => Name.Replace(" ", "_") ?? default;
    public abstract string DefaultSpritePath { get; }
    public virtual bool Visible => SpriteRenderer.enabled;
    
    public virtual void Hook()
    {
        HUDManager.Add(this);
    }
    
    public GameObject GameObject { get; set; } //Assigned at Runtime
    public SpriteRenderer SpriteRenderer { get; set; }

    public virtual void SetVisibility(bool visibility)
    {
        if(SpriteRenderer is null) SpriteRenderer = GameObject.GetComponent<SpriteRenderer>();
        SpriteRenderer.enabled = visibility;
        HUDManager.DisplayHUDRowElements();
    }
}