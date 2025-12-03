using System.Collections;
using System.Collections.Generic;
using DistantGreensCharms.HUDElements;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DistantGreensCharms.Helper;

public static class HUDHelper
{
    private static bool _isInitialized = false;
    private static Dictionary<string, AHUDElement> HUDElements = new();
    
    public static void Initialize()
    {
        if (_isInitialized) return;
        
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        ModHooks.AfterSavegameLoadHook += OnSaveLoaded;
        
        _isInitialized = true;
        DistantGreensCharms.Instance.Log("[HUDHelper] Initialized");
    }
    
    private static void OnSceneChanged(Scene from, Scene to)
    {
        DistantGreensCharms.Instance.Log($"[HUDHelper] Scene changed: {from.name} -> {to.name}");
        RecreateAllElements();
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
        //foreach (var element in elementsToRecreate)
        //{
        //    GameManager.instance.StartCoroutine(AddWhenReady(element, isRecreation: true));
        //}
    }

    public static void Add(AHUDElement hudElement)
    {
        GameManager.instance.StartCoroutine(AddWhenReady(hudElement));
    }
    
    private static IEnumerator AddWhenReady(AHUDElement hudElement)
    {
        yield return WaitForUI();
        GameObject gameObject = new(hudElement.Name);
        GameObject gameObjectParent = GameObject.Find($"_GameCameras/HudCamera/Hud Canvas{hudElement.ParentRoute}"); //default if ParentRoute == null -> Hud Canvas
        gameObject.transform.SetParent(gameObjectParent.transform);
        
        RectTransform rt = gameObject.AddComponent<RectTransform>();
        rt.anchorMin = hudElement.LocationAttributes.anchorMin;
        rt.anchorMax = hudElement.LocationAttributes.anchorMax;
        rt.pivot = hudElement.LocationAttributes.pivot;
        rt.anchoredPosition = hudElement.LocationAttributes.anchoredPosition;
        rt.sizeDelta = hudElement.LocationAttributes.sizeDelta;
        rt.localScale = hudElement.LocationAttributes.localScale;
        
        Image image = gameObject.AddComponent<Image>();
        
        if (hudElement.SpritePath != null)
        {
            image.sprite = SpriteManager.Get(hudElement.SpritePath);
            image.preserveAspect = true;
        }
        
        hudElement.GameObject = gameObject;
        
        if(!HUDElements.ContainsKey(hudElement.Name)) HUDElements.Add(hudElement.Name, hudElement);
        //Log ERROR
    }
    
    public static AHUDElement Get(string key)
    {
        if (HUDElements.TryGetValue(key, out AHUDElement hudElement)) return hudElement;
        return null;
    }
    
    public static void UpdateVisibility(string key, bool visibility)
    {
        AHUDElement hudElement = Get(key);
        if (hudElement == null) return;
        hudElement.SetVisibility(visibility);
    }
    
    public static void UpdateIcon(string key, string spritePath)
    {
        AHUDElement hudElement = Get(key);
        if (hudElement == null) return;
        Sprite sprite = SpriteManager.Get(spritePath);
        if(sprite == null) return;
        hudElement.Icon.sprite = sprite;
    }
    
    private static IEnumerator WaitForUI()
    {
        yield return new WaitWhile(()=> 
            GameManager.instance is null ||
            GameManager.instance.ui is null ||
            GameManager.instance.ui.gameObject is null);
    }
}