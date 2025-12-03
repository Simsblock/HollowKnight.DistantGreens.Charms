using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace DistantGreensCharms.HUDElements;

public abstract class AHUDElement
{
    public abstract string Name { get; }
    
    public abstract string SpritePath { get; }
    public abstract bool Visible { get; set; }
    
    public abstract RectTransformAttributes LocationAttributes { get; }
    
    [CanBeNull] public string ParentRoute { get; set; } //Exmaple: "/Soul Orb" for "_GameCameras/HudCamera/Hud Canvas/Soul Orb"
    public GameObject GameObject { get; set; } //Assigned at Runtime
    public Image Icon  => GameObject.GetComponent<Image>(); //Assigned at Runtime

    public virtual void SetVisibility(bool visibility)
    {
        Visible = visibility;
        Icon.enabled = visibility;
    }
}

public struct RectTransformAttributes
{
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
    public Vector2 anchoredPosition;
    public Vector2 sizeDelta;
    public Vector2 localScale;
}