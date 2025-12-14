using System;
using System.Collections.Generic;
using DebugMod;
using DistantGreensCharms.Helper;
using DistantGreensCharms.Settings;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Locations.SpecialLocations;
using Modding;
using MonoMod.RuntimeDetour;
using RandomizerCore.Extensions;
using SFCore.Utils;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using Bounds = UnityEngine.Bounds;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace DistantGreensCharms.Charms;

public class Painters_Spirit : ACharm
{
    public static readonly Painters_Spirit Instance = new();
    public override string SpritePath => "CharmIcons.Painters_Spirit";
    public override string Name => "Painters Spirit";
    public override string Description => "Masterful artists poured their ideas and experience into this charm, \nin the process enlightening the users nail arts.";
    public override int DefaultCost => 2;
    public override string SceneName => "GG_Workshop"; //todo 
    public override float X => 17.65f; //todo 
    public override float Y => 6.41f; //todo 

    public override CharmState State(LocalSettings s) => s.Painters_Spirit;
    
    //private GameObject _cycloneSlashPrefab;
    private GameObject _dashSlashPrefab;
    private GameObject _greatSlashPrefab;
    private PlayMakerFSM gSlash_damageFSM;
    private PlayMakerFSM dSlash_damageFSM;
    
    private GameObject _paintDecalPrefab;
    private Sprite[] _paintDecalSprites =
        [SpriteManager.Get("Misc.paint0"), SpriteManager.Get("Misc.paint1"), SpriteManager.Get("Misc.paint2"),SpriteManager.Get("Misc.paint3"),SpriteManager.Get("Misc.paint4")];
    private Random _random = new();

    // PaintSplat, PainBullet, PlainContext
    //IL.PaintSplat
    //TerrainPaintUtility

    public override void Hook()
    {
        Create_paintDecalPrefab();
        On.HeroController.Start += ModifyNailArtFsm;
        On.GameManager.EquipCharm += OnEquipCharm;
        On.GameManager.UnequipCharm += OnUnequipCharm;
    }

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
    
    private void ModifyNailArtFsm(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        PlayMakerFSM nailArtFSM = self.gameObject.LocateMyFSM("Nail Arts");
        _dashSlashPrefab =self.transform.Find("Attacks/Dash Slash").gameObject;
        _greatSlashPrefab = self.transform.Find("Attacks/Great Slash").gameObject;
        gSlash_damageFSM = _dashSlashPrefab.gameObject.LocateMyFSM("nailart_damage");
        dSlash_damageFSM = _greatSlashPrefab.gameObject.LocateMyFSM("nailart_damage");

        FsmState CSlash = nailArtFSM.GetState("Cyclone Spin");
        CSlash.InsertAction(new ExecuteLambda(() => { if (Equipped()) SpawnPaintOnGround(self); }),3);
        
        FsmState DSlash = nailArtFSM.GetState("Dash Slash");
        DSlash.InsertAction(new ExecuteLambda(() => { if (Equipped()) SpawnPaintOnGround(self); }),3);
        
        FsmState GSlash = nailArtFSM.GetState("G Slash");
        GSlash.InsertAction(new ExecuteLambda(() => { if (Equipped()) SpawnPaintOnGround(self); }),3);
    }
    
    private void OnEquipCharm(On.GameManager.orig_EquipCharm orig, GameManager self, int charmnum)
    {
        orig(self, charmnum);
        if(charmnum != Num) return;
        // No dmg update for cyclone since its already strong + I cant figure out where its damage is calculated! Could increase Cyclone Timer, need to change sprite on thingi
        gSlash_damageFSM.FsmVariables.FindFsmFloat("Multiplier").Value = 3.5f; //maybe raise to 4 since Unbreakable Strength with Quick Slash is SO op
        dSlash_damageFSM.FsmVariables.FindFsmFloat("Multiplier").Value = 3.5f;
    }
    private void OnUnequipCharm(On.GameManager.orig_UnequipCharm orig, GameManager self, int charmnum)
    {
        orig(self, charmnum);
        if(charmnum != Num) return;
        // No dmg update for cyclone since its already strong + I cant figure out where its damage is calculated! Could increase Cyclone Timer, need to change sprite on thingi
        gSlash_damageFSM.FsmVariables.FindFsmFloat("Multiplier").Value = 2.5f;
        dSlash_damageFSM.FsmVariables.FindFsmFloat("Multiplier").Value = 2.5f;
    }
    
    private void SpawnPaintOnGround(HeroController self)
    {
        if (!Equipped()) return;
        
        RaycastHit2D hit = Physics2D.Raycast(
            self.transform.position,
            Vector2.down,
            5f,
            LayerMask.GetMask("Terrain")
        );
        
        if (hit.collider != null && hit.collider.gameObject.tag == "Untagged")
        {
            int directionModifier = 1;
            if(!self.cState.facingRight) directionModifier = -1;
            for (int i = 0; i < _random.Next(5)+1; i++)
            {
                GameObject decal = GameObject.Instantiate(_paintDecalPrefab);
                Vector3 paintPosition = new Vector3(
                    hit.point.x + (_random.Next(256*i)/100f*directionModifier),
                    hit.point.y - 0.64f,
                    hit.transform.GetPositionZ() - 0.5f -
                    (_random.Next(10) / 100f) // to randomize if the new paint is above or below a previous one
                );
                Bounds hitBounds = hit.collider.bounds;
                List<Sprite> possibleSprites = new();
                foreach (var sprite in _paintDecalSprites)
                {
                    float height = sprite.bounds.size.y;
                    float width = sprite.bounds.size.x;
                    float halfWidth = width / 2f;
                    if (paintPosition.x + halfWidth <= hitBounds.max.x &&
                        paintPosition.x - halfWidth >= hitBounds.min.x) possibleSprites.Add(sprite);
                }

                if (possibleSprites.Count == 0) return;
                decal.GetComponent<SpriteRenderer>().sprite = possibleSprites[_random.Next(possibleSprites.Count)];
                decal.transform.position = paintPosition;
                decal.SetActive(true);
            }
        }
    }
}
