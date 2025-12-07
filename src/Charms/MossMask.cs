using System.Collections;
using DistantGreensCharms.Helper;
using DistantGreensCharms.HUDElements;
using DistantGreensCharms.Settings;
using IL.HutongGames.PlayMaker.Actions;
using ItemChanger.Items;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DistantGreensCharms.Charms;

public class MossMask : ACharm
{
    public static readonly MossMask Instance = new();
    private bool _charged = true;
    private bool Useable => _charged && Equipped();
    
    public override string SpritePath  => "CharmIcons.MossMask"; 
    public override string Name => "Moss-Mask"; 
    public override string Description => "Appears like a Mask protecting its wearer.\nShields against critical damage,but shatters on impact.\n\nIf shattered, the mask will repair itself while resting at a bench.";
    public override int DefaultCost => 3;
    public override string SceneName => "GG_Workshop"; //todo 
    public override float X => 17.65f; //todo 
    public override float Y => 6.41f; //todo 
    
    public override CharmState State(LocalSettings s) => s.MossMask;

    private MossMaskHUD HUD => MossMaskHUD.Instance;

    public override void Hook()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        ModHooks.TakeHealthHook += CheckMaskActivation;
        ModHooks.SetPlayerBoolHook += OnSetPlayerBool;
        On.GameManager.EquipCharm += OnEquipCharm;
        On.GameManager.UnequipCharm += OnUnequipCharm;
    }
    
    private void OnSceneChange(Scene from, Scene to)
    {
        if (!Equipped()) return;
        if (to.name.Contains("Dream") || to.name.Contains("GG_Atrium") || to.name.Equals("GG_Workshop")) SetCharged(true);
    }
    private int CheckMaskActivation(int damage)
    {
        if (!Useable) return damage;
        bool JonisBlessing = PlayerData.instance.equippedCharm_27;
        if (JonisBlessing && PlayerData.instance.healthBlue+1 - damage <= 0) // Jonis gives Player 12 blueHealth BUT keeps 1 health -> +1
        {
            PlayerData.instance.health = 1;
            PlayerData.instance.healthBlue = 0;
            SetCharged(false);
            return 0;
        }
        if (!JonisBlessing && PlayerData.instance.health - damage <= 0)
        {
            PlayerData.instance.health = 1; 
            SetCharged(false); 
            return 0;
        }
        return damage;
    }

    private bool OnSetPlayerBool(string target, bool value)
    {
        if (target == "atBench" && value && Equipped())
        {
            SetCharged(true);
        }
        return value;
    }
    
    private void OnEquipCharm(On.GameManager.orig_EquipCharm orig, GameManager self, int charmnum)
    {
        orig(self, charmnum);
        if(charmnum == Num) HUD.SetVisibility(true);
    }
    
    private void OnUnequipCharm(On.GameManager.orig_UnequipCharm orig, GameManager self, int charmnum)
    {
        orig(self, charmnum);
        if(charmnum == Num) HUD.SetVisibility(false);
    }

    private void SetCharged(bool charged)
    {
        _charged = charged;
        HUD.UpdateSpriteState(charged);
    }
}