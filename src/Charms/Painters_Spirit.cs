using System;
using System.Collections.Generic;
using DebugMod;
using DistantGreensCharms.Helper;
using DistantGreensCharms.Settings;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Locations.SpecialLocations;
using Modding;
using MonoMod.RuntimeDetour;
using RandomizerCore.Extensions;
using SFCore.Utils;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using Bounds = UnityEngine.Bounds;
using Random = System.Random;

namespace DistantGreensCharms.Charms;

public class Painters_Spirit : ACharm
{
    public static readonly Painters_Spirit Instance = new();
    public override string SpritePath => "CharmIcons.MossMask";
    public override string Name => "Painters Spirit";
    public override string Description => "PaintSplat on Nail";
    public override int DefaultCost => 0;
    public override string SceneName => "GG_Workshop"; //todo 
    public override float X => 17.65f; //todo 
    public override float Y => 6.41f; //todo 

    public override CharmState State(LocalSettings s) => s.Painters_Spirit;

    // PaintSplat, PainBullet, PlainContext
    //IL.PaintSplat
    //TerrainPaintUtility

    public override void Hook()
    {
        Create_paintDecalPrefab();
        
        On.NailSlash.StartSlash += OnNailSlash;
        //ModHooks.AttackHook += OnNailSlash;
    }

    private void OnNailSlash(On.NailSlash.orig_StartSlash orig, global::NailSlash self)
    {
        orig(self);
        
        DistantGreensCharms.Instance.Log("nailslashnames: " + self.name + " " + self.animName + " " + self.gameObject.name);

        DistantGreensCharms.Instance.Log($"OnNailSlash called. Prefab null? {_paintDecalPrefab == null}");

        if (_paintDecalPrefab == null)
        {
            DistantGreensCharms.Instance.Log("ERROR: Prefab is null! Recreating...");
            Create_paintDecalPrefab();
        }

        SpawnPaintOnGround(self.transform.position);
    }

    private GameObject _paintDecalPrefab;
    private Sprite[] _paintDecalSprites =
        [SpriteManager.Get("Misc.paint0"), SpriteManager.Get("Misc.paint1"), SpriteManager.Get("Misc.paint2"),SpriteManager.Get("Misc.paint3"),SpriteManager.Get("Misc.paint4")];
    private Random _random = new();
    private void Create_paintDecalPrefab()
    {
        DistantGreensCharms.Instance.Log("Creating paint decal prefab...");

        _paintDecalPrefab = new GameObject("PaintDecal");
        DistantGreensCharms.Instance.Log($"Created GameObject: {_paintDecalPrefab != null}");

        var renderer = _paintDecalPrefab.AddComponent<SpriteRenderer>();
        DistantGreensCharms.Instance.Log($"Added SpriteRenderer: {renderer != null}");
        
        renderer.sortingLayerName = "Default";
        
        GameObject.DontDestroyOnLoad(_paintDecalPrefab);
        DistantGreensCharms.Instance.Log($"Marked DontDestroyOnLoad");

        _paintDecalPrefab.SetActive(false);
        DistantGreensCharms.Instance.Log($"Set inactive. Prefab null? {_paintDecalPrefab == null}");
    }
    
    private void SpawnPaintOnGround(Vector3 attackPosition)
    {
        if (!Equipped()) return;
        RaycastHit2D hit = Physics2D.Raycast(
            attackPosition,
            Vector2.down,
            5f,
            LayerMask.GetMask("Terrain")
        );
        //DistantGreensCharms.Instance.Log($"Raycast hit: {hit.collider.gameObject.name}");
        
        if (hit.collider != null && hit.collider.gameObject.tag == "Untagged")
        {
                GameObject decal = GameObject.Instantiate(_paintDecalPrefab);
                Vector3 paintPosition = new Vector3(
                    hit.point.x,
                    hit.point.y - 0.64f,
                    hit.transform.GetPositionZ() - 0.5f - (_random.Next(10)/100f) // to randomize if the new paint is above or below a previous one
                );
                Bounds hitBounds = hit.collider.bounds;
                List<Sprite> possibleSprites = new();
                foreach (var sprite in _paintDecalSprites) //maybe check y-axis too todo
                {
                    float height = sprite.bounds.size.y;
                    float width = sprite.bounds.size.x;
                    float halfWidth = width / 2f;
                    if(paintPosition.x +  halfWidth <= hitBounds.max.x &&
                       paintPosition.x -  halfWidth >= hitBounds.min.x) possibleSprites.Add(sprite);
                }

                if (possibleSprites.Count == 0) return;
                decal.GetComponent<SpriteRenderer>().sprite = possibleSprites[_random.Next(possibleSprites.Count)];
                decal.transform.position = paintPosition;
                decal.SetActive(true);
        }
    }
}
