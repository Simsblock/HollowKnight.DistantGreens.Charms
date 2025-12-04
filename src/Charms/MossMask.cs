using DistantGreensCharms.Helper;
using DistantGreensCharms.HUDElements;
using DistantGreensCharms.Settings;
using ItemChanger.Items;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DistantGreensCharms.Charms;

public class MossMask : ACharm
{
    public static readonly MossMask Instance = new();
    
    public override string SpritePath  => "CharmIcons.MossMask"; //test
    public override string Name => "Moss-Mask"; 
    public override string Description => "Moss-Mask description"; //todo
    public override int DefaultCost => 3;
    public override string SceneName => "GG_Workshop"; //todo
    public override float X => 17.65f; //todo
    public override float Y => 6.41f; //todo
    
    public override CharmState State(LocalSettings s) => s.MossMask;
    public override void MarkAsEncountered(GlobalSettings s) => s.EncounteredMossMask = true;

    private MossMaskHUD HUD { get; set; } = new();

    public override void Hook() //Pantheon error, doesnt reset after failing a pantheon todo
    {
        //HUDManager.Add(HUD);
        ModHooks.TakeHealthHook += CheckMaskActivation;
        ModHooks.SetPlayerBoolHook += OnSetPlayerBool;
        On.GameManager.EquipCharm += OnEquipCharm;
        On.GameManager.UnequipCharm += DeEquipCharm;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        HUD.Hook();
    }

    private void OnSceneChange(Scene from, Scene to)
    {
        if (!Equipped()) return;
        else if (to.name.Contains("Dream")) SetCharged(true);
        else if (to.name.Contains("GG_Atrium") || to.name.Equals("GG_Workshop")) SetCharged(true);
        //else if (to.name.Contains("GG_") && !BossSequenceController.IsInSequence ) SetCharged(true); //IsInSequence tells us if in Pantheon, but is still true when getting returned from it -> Atrium or Workshop
    }
    
    private bool _charged = true;
    private bool Useable => _charged && Equipped();
    private int CheckMaskActivation(int damage)
    {
        if (!Useable || PlayerData.instance.health - damage > 0) return damage;
        if (PlayerData.instance.health - damage > 0) return damage;
        SetCharged(false);
        PlayerData.instance.health = 1;
        return 0;
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
    
    private void DeEquipCharm(On.GameManager.orig_UnequipCharm orig, GameManager self, int charmnum)
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