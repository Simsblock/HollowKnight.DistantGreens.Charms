using System;
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
    private static Dictionary<string, AHUDRowElement> HUDRowElements = new();
    private static GameObject RowRoot = new GameObject();
    private static bool baldursShellEquipped = false;
    
    public static void Hook()
    {
        if (_isInitialized) return;
        
        ModHooks.AfterSavegameLoadHook += OnSaveLoaded;
        On.GameManager.EquipCharm += BaldurShellEquip;
        On.GameManager.UnequipCharm += BaldurShellUnEquip;

        RowRoot.transform.localPosition = new(0, 0, 0);
        
        _isInitialized = true;
        DistantGreensCharms.Instance.Log("[HUDHelper] Initialized");
    }

    private static void BaldurShellEquip(On.GameManager.orig_EquipCharm orig, GameManager self, int charmnum)
    {
        if(charmnum!=5) return;
        baldursShellEquipped = true;
        DisplayHUDRowElements();
        orig(self, charmnum);
    }
    
    private static void BaldurShellUnEquip(On.GameManager.orig_UnequipCharm orig, GameManager self, int charmnum)
    {
        if(charmnum!=5) return;
        baldursShellEquipped = false;
        DisplayHUDRowElements();
        orig(self, charmnum);
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

        gameObject.transform.localPosition =
            new Vector3(hudElement.X, hudElement.Y, hudElement.Z);
        gameObject.transform.localScale = Vector3.one * hudElement.Scale;
        
        GameObject gameObjectParent = hudElement.OverrideParent ?? GameCameras.instance.hudCanvas;
        gameObject.transform.SetParent(gameObjectParent.transform);

        if (!isRecreation && !HUDElements.ContainsKey(hudElement.Name)) HUDElements.Add(hudElement.Name, hudElement);
        else GetElement(hudElement.Name).GameObject = gameObject;
    }
    
    public static AHUDElement GetElement(string key)
    {
        if (HUDElements.TryGetValue(key, out AHUDElement hudElement)) return hudElement;
        return null;
    }

    public static void Add(AHUDRowElement hudElement, bool isRecreation = false)
    {
        GameObject gameObject = new(hudElement.Name);
        gameObject.transform.SetParent(RowRoot.transform);
        gameObject.layer = 5;
        
        hudElement.GameObject = gameObject;
        
        hudElement.SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        hudElement.SpriteRenderer.sortingLayerName = "HUD";
        
        hudElement.SpriteRenderer.enabled = false;
        hudElement.SpriteRenderer.sprite = SpriteManager.Get(hudElement.DefaultSpritePath);
        
        float spriteSize = hudElement.SpriteRenderer.sprite.bounds.size.x; // only X!!!!
        float scale = 1f / spriteSize;
        gameObject.transform.localScale = Vector3.one * scale;
        
        gameObject.transform.SetParent(RowRoot.transform);
        
        if (!isRecreation && !HUDRowElements.ContainsKey(hudElement.Name)) HUDRowElements.Add(hudElement.Name, hudElement);
        else GetElement(hudElement.Name).GameObject = gameObject;
    }
    
    public static AHUDRowElement GetRowElement(string key)
    {
        if (HUDRowElements.TryGetValue(key, out AHUDRowElement hudElement)) return hudElement;
        return null;
    }

    public static void DisplayHUDRowElements()
    {
        int i = 1;
        if (baldursShellEquipped) i++;
        foreach (var pair in HUDRowElements)
        {
            if(!pair.Value.Visible) continue;
            i++;
            pair.Value.GameObject.transform.localPosition = new Vector3(1 * i, 1, 1);
        }
    }
    
}