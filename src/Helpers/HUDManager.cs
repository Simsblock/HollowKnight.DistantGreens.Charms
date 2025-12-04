using System.Collections;
using System.Collections.Generic;
using DistantGreensCharms.HUDElements;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DistantGreensCharms.Helper;

public static class HUDManager
{
    private static bool _isInitialized = false;
    private static Dictionary<string, AHUDElement> HUDElements = new();
    
    public static void Initialize()
    {
        if (_isInitialized) return;
        
        ModHooks.AfterSavegameLoadHook += OnSaveLoaded;
        
        _isInitialized = true;
        DistantGreensCharms.Instance.Log("[HUDHelper] Initialized");
    }

    private static void OnSaveLoaded(SaveGameData data)
    {
        DistantGreensCharms.Instance.Log("[HUDHelper] Save loaded, recreating HUD");
        RecreateAllElements();
    }

    private static void RecreateAllElements()
    {
        // Store references to all elements (don't modify dict while iterating)
        List<AHUDElement> elementsToRecreate = new List<AHUDElement>(HUDElements.Values);
        
        // Clear GameObjects (they're destroyed by scene change anyway)
        foreach (var element in elementsToRecreate)
        {
            element.GameObject = null;
        }
        
        // Recreate each element
        foreach (var element in elementsToRecreate)
        {
            GameManager.instance.StartCoroutine(AddWhenReady(element, isRecreation: true));
        }
    }

    public static void Add(AHUDElement hudElement)
    {
        GameManager.instance.StartCoroutine(AddWhenReady(hudElement));
    }
    
    private static IEnumerator AddWhenReady(AHUDElement hudElement, bool isRecreation = false)
    {
        yield return WaitForUI();
        GameObject gameObject = new(hudElement.Name);
        gameObject.layer = 5;

        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "HUD";
        //spriteRenderer.sortingOrder = hudElement.SortingOrder; //Seems irrelevant
        spriteRenderer.enabled = hudElement.Visible;
        spriteRenderer.sprite = SpriteManager.Get(hudElement.DefaultSpritePath);

        GameObject gameObjectParent = GameCameras.instance.hudCanvas;
        gameObject.transform.SetParent(gameObjectParent.transform);

        gameObject.transform.localPosition =
            new Vector3(hudElement.X, hudElement.Y, hudElement.Z);
        gameObject.transform.localScale = Vector3.one * hudElement.Scale;
        
        gameObject.transform.SetParent(gameObjectParent.transform);
        
        hudElement.GameObject = gameObject;

        if (!isRecreation && !HUDElements.ContainsKey(hudElement.Name)) HUDElements.Add(hudElement.Name, hudElement);
        else Get(hudElement.Name).GameObject = gameObject;
    }
    
    public static AHUDElement Get(string key)
    {
        if (HUDElements.TryGetValue(key, out AHUDElement hudElement)) return hudElement;
        return null;
    }
    
    // Not used?
    public static void UpdateSprite(string key, string spritePath)
    {
        AHUDElement hudElement = Get(key);
        if (hudElement == null) return;
        Sprite sprite = SpriteManager.Get(spritePath);
        if(sprite == null) return;
        hudElement.SpriteRenderer.sprite = sprite;
    }
    
    private static IEnumerator WaitForUI()
    {
        yield return new WaitWhile(()=> 
            GameManager.instance is null ||
            GameManager.instance.ui is null ||
            GameManager.instance.ui.gameObject is null);
    }
}