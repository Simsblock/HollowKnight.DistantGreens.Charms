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
        DistantGreensCharms.Instance.Log("Add started");
        GameObject gameObject = new(hudElement.Name);
        gameObject.layer = 5;
        DistantGreensCharms.Instance.Log("gameobject created: "+(gameObject != null).ToString());

        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteManager.Get(hudElement.SpritePath);
        spriteRenderer.sortingLayerName = "HUD";
        // spriteRenderer.sortingOrder = 5;
        DistantGreensCharms.Instance.Log("spriterenderer created: "+(spriteRenderer != null).ToString());
        
        //GameObject gameObjectParent = GameObject.Find(hudElement.ParentName ?? "Hud Canvas");
        //GameObject gameObjectParent = GameObject.Find("_GameCameras/HudCamera/Hud Canvas");
        GameObject gameObjectParent = GameCameras.instance.hudCanvas;
        DistantGreensCharms.Instance.Log("Parent getted: "+(gameObjectParent != null).ToString());
        gameObject.transform.SetParent(gameObjectParent.transform);

        gameObject.transform.localPosition =
            new Vector3(hudElement.X, hudElement.Y, hudElement.Z);
        // gameObject.transform.localScale = Vector3.one * 0.7f;
        
        DistantGreensCharms.Instance.Log("gameobjectParent create: "+ (gameObjectParent==null).ToString());
        gameObject.transform.SetParent(gameObjectParent.transform);
        
        hudElement.GameObject = gameObject;
        
        if(!isRecreation && !HUDElements.ContainsKey(hudElement.Name)) HUDElements.Add(hudElement.Name, hudElement);
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
        hudElement.SpriteRenderer.sprite = sprite;
    }
    
    private static IEnumerator WaitForUI()
    {
        DistantGreensCharms.Instance.Log("GameManager is null: "+ (GameManager.instance==null).ToString());
        DistantGreensCharms.Instance.Log("GameManager UI is null: "+ (GameManager.instance.ui==null).ToString());
        DistantGreensCharms.Instance.Log("GameManager UI Gameobject is null: "+ (GameManager.instance.ui.gameObject==null).ToString());
        yield return new WaitWhile(()=> 
            GameManager.instance is null ||
            GameManager.instance.ui is null ||
            GameManager.instance.ui.gameObject is null);
    }
}