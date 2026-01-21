using System.Collections;
using System.Collections.Generic;
using DistantGreensCharms.HUDElements;
using HutongGames.Utility;
using ItemChanger.Extensions;
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
    
    public static void Hook()
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
        List<AHUDElement> elementsToRecreate = new List<AHUDElement>(HUDElements.Values);
        
        foreach (var element in elementsToRecreate)
        {
            element.GameObject = null;
        }
        
        foreach (var element in elementsToRecreate)
        {
            Add(element, isRecreation: true);
        }
    }
    
    public static void Add(AHUDElement hudElement, bool isRecreation = false)
    {
        //yield return WaitForUI();
        GameObject gameObject = new(hudElement.Name);
        gameObject.layer = 5;
        
        hudElement.GameObject = gameObject;
        
        hudElement.SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        hudElement.SpriteRenderer.sortingLayerName = "HUD";
        //spriteRenderer.sortingOrder = hudElement.SortingOrder; //Seems irrelevant
        hudElement.SpriteRenderer.enabled = false;
        hudElement.SpriteRenderer.sprite = SpriteManager.Get(hudElement.DefaultSpritePath);

        GameObject gameObjectParent = hudElement.OverrideParent ?? GameCameras.instance.hudCanvas;
        gameObject.transform.SetParent(gameObjectParent.transform);

        gameObject.transform.localPosition =
            new Vector3(hudElement.X, hudElement.Y, hudElement.Z);
        gameObject.transform.localScale = Vector3.one * hudElement.Scale;
        
        gameObject.transform.SetParent(gameObjectParent.transform);

        if (!isRecreation && !HUDElements.ContainsKey(hudElement.Name)) HUDElements.Add(hudElement.Name, hudElement);
        else Get(hudElement.Name).GameObject = gameObject;
    }
    
    public static AHUDElement Get(string key)
    {
        if (HUDElements.TryGetValue(key, out AHUDElement hudElement)) return hudElement;
        return null;
    }
}