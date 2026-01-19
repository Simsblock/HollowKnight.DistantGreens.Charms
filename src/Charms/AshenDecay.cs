using DistantGreensCharms.Helper;
using DistantGreensCharms.Settings;
using HutongGames.PlayMaker;
using Modding;
using RandomizerMod.RC;
using UnityEngine;
using ItemChanger.Extensions;

namespace DistantGreensCharms.Charms;

public class AshenDecay : ACharm
{
    /*TODO
     
     Silksongsong healing Integration
     Deep Focus Integration
     DOT apply
     Description
     Location (in Kingdoms Edge)
     HUD & Sprite
     */
    
    public static readonly AshenDecay Instance = new();
    //private bool _charged = true;
    private const float _damage_multiplier=1f; // based of nail damage
    private const float _chargeTime = 5f;
    private bool _charged = false;
    private float _remainingChargeTime;
    // private bool Useable => _charged && Equipped();
    
    public override string SpritePath  => "CharmIcons.MossMask"; //todo
    public override string Name => "Ashen Decay"; 
    public override string Description => "Apply DOT after focus";
    public override int DefaultCost => 2;
    public override string SceneName => "Fungus3_50"; //todo
    public override float X => 22.07f; //todo
    public override float Y => 115.40f; //todo
    
    public override CharmState State(LocalSettings s) => s.MossMask;

    public override void Hook()
    {
        //On.GameManager.EquipCharm += OnEquipCharm;
        //On.GameManager.UnequipCharm += OnUnequipCharm;
        //On.HealthManager.Hit += OnEnemyHit;
        //On.HeroController.StopMPDrain += OnFocusEnd;
        On.HeroController.Start += ModifySpellControlFsm;
        ModHooks.HeroUpdateHook += ModHooksOnHeroUpdateHook;
    }

    private void ModifySpellControlFsm(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        PlayMakerFSM spellControlFSM = self.gameObject.LocateMyFSM("Spell Control");
        if (spellControlFSM == null)
        {
            DistantGreensCharms.Instance.LogError("Could not find Spell Control FSM!");
            return;
        }

        FsmState focusGetFinish = spellControlFSM.GetState("Focus Get Finish"); // maybe needs change
        if (focusGetFinish == null)
        {
            DistantGreensCharms.Instance.LogError("Could not find Focus Get Finish State!");
        }
        focusGetFinish.AddFirstAction(new ExecuteLambda(
            () => OnFocusFinished()
            )
        );
    }
    
    private void OnFocusFinished()
    {
        if(!Equipped()) return;
        _remainingChargeTime = _chargeTime;
        _charged = true;
    }

    private void ModHooksOnHeroUpdateHook()
    {
        if (!_charged) return;
        _remainingChargeTime -= Time.deltaTime;
        if(_remainingChargeTime <= 0f) _charged = false;
    }
    
    private void OnEnemyHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
    {
        orig(self, hitInstance);
        if(!_charged) return;

        //GameObject enemyObject = self.gameObject;
        //int enemyHP = self.hp;
    }
    /*

    private void OnEquipCharm(On.GameManager.orig_EquipCharm orig, GameManager self, int charmnum)
    {
        orig(self, charmnum);
        //if(charmnum == Num) HUD.SetVisibility(true);
    }

    private void OnUnequipCharm(On.GameManager.orig_UnequipCharm orig, GameManager self, int charmnum)
    {
        orig(self, charmnum);
        //if(charmnum == Num) HUD.SetVisibility(false);
    }*/
}