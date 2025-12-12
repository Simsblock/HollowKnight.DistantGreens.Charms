using DistantGreensCharms.Helper;
using DistantGreensCharms.Settings;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

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

    private GameObject _paintDecalPrefab;

    private void Create_paintDecalPrefab()
    {
        DistantGreensCharms.Instance.Log("Creating paint decal prefab...");

        _paintDecalPrefab = new GameObject("PaintDecal");
        DistantGreensCharms.Instance.Log($"Created GameObject: {_paintDecalPrefab != null}");

        var renderer = _paintDecalPrefab.AddComponent<SpriteRenderer>();
        DistantGreensCharms.Instance.Log($"Added SpriteRenderer: {renderer != null}");

        Sprite sprite = SpriteManager.Get("paint");
        DistantGreensCharms.Instance.Log($"Created Sprite: {sprite != null}");

        renderer.sprite = sprite;
        renderer.sortingLayerName = "Default";
        renderer.sortingOrder = 10;
        
        GameObject.DontDestroyOnLoad(_paintDecalPrefab);
        DistantGreensCharms.Instance.Log($"Marked DontDestroyOnLoad");

        _paintDecalPrefab.SetActive(false);
        DistantGreensCharms.Instance.Log($"Set inactive. Prefab null? {_paintDecalPrefab == null}");
    }

    private void OnNailSlash(On.NailSlash.orig_StartSlash orig, global::NailSlash self)
    {
        orig(self);

        DistantGreensCharms.Instance.Log($"OnNailSlash called. Prefab null? {_paintDecalPrefab == null}");

        if (_paintDecalPrefab == null)
        {
            DistantGreensCharms.Instance.Log("ERROR: Prefab is null! Recreating...");
            Create_paintDecalPrefab();
        }

        SpawnPaintOnGround(self.transform.position);
    }

    private void SpawnPaintOnGround(Vector3 attackPosition)
    {
        if (!Equipped()) return;
        DistantGreensCharms.Instance.Log($"SpawnPaintOnGround called. Prefab null? {_paintDecalPrefab == null}");

        RaycastHit2D hit = Physics2D.Raycast(
            attackPosition,
            Vector2.down,
            5f,
            LayerMask.GetMask("Terrain")
        );

        DistantGreensCharms.Instance.Log($"Raycast hit: {hit.collider != null}");

        if (hit.collider != null && hit.collider.gameObject.tag == "Untagged")
        {
            //DistantGreensCharms.Instance.Log($"Hit object: {hit.collider.gameObject.name}");
            //DistantGreensCharms.Instance.Log($"Layer index: {hit.collider.gameObject.layer}");
            //DistantGreensCharms.Instance.Log($"Layer name: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            //DistantGreensCharms.Instance.Log($"Tag: {hit.collider.gameObject.tag}");

            //DistantGreensCharms.Instance.Log($"Hit point: {hit.point}");
            //DistantGreensCharms.Instance.Log($"Hit normal: {hit.normal}");
            
            GameObject ground = hit.collider.gameObject;
            //DistantGreensCharms.Instance.Log($"Hit ground: {ground.name}");
            
            var meshRenderer = ground.GetComponent<MeshRenderer>();
            //if (meshRenderer != null)
            //{
            //    DistantGreensCharms.Instance.Log("Ground uses MeshRenderer!");
            //} 
            
            var layer = ground.layer;
            //DistantGreensCharms.Instance.Log("Ground uses Layer: "+layer.ToString());

            //DistantGreensCharms.Instance.Log($"About to instantiate. Prefab null? {_paintDecalPrefab == null}");

            // This is the line that's throwing the error
                GameObject decal = GameObject.Instantiate(_paintDecalPrefab);

                DistantGreensCharms.Instance.Log("Instantiated successfully!");

                decal.transform.position = new Vector3(
                    hit.point.x,
                    hit.point.y - 0.64f //,
                    //groundRenderer.transform.position.z
                );
                //decal.transform.rotation = Quaternion.identity;
                //decal.transform.Rotate(0, 0, Random.Range(0f, 360f));
                decal.SetActive(true);
        }
    }
}
