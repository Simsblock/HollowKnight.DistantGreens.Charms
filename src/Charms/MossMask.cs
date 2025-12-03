using DistantGreensCharms.Settings;
using ItemChanger.Items;
using Modding;
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

    public override void Hook()
    {
        ModHooks.TakeHealthHook += CheckMaskActivation;
        ModHooks.SetPlayerBoolHook += ChargeCharmAtBench;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;

    }
    
    private void OnSceneChange(Scene from, Scene to)
    {
        if (!Equipped()) return;
        if (to.name.Contains("Dream")) _charged = true;
        if (to.name.Contains("GG_") && !BossSequenceController.IsInSequence) _charged = true;
    }
    
    private bool _charged = true;
    private bool Useable => _charged && Equipped();
    private int CheckMaskActivation(int damage)
    {
        if (!Useable || PlayerData.instance.health - damage > 0) return damage;
        if (PlayerData.instance.health - damage > 0) return damage;
        _charged = false;
        PlayerData.instance.health = 1;
        return 0;
    }
    private bool ChargeCharmAtBench(string targetBool, bool value)
    {
        if(targetBool == "atBench" && value && Equipped()) _charged = true;
        return value;
    }
}