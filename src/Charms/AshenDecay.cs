using System.Collections.Generic;
using System.Linq;
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
     HUD & Sprite
     */
    
    public static readonly AshenDecay Instance = new();
    //private bool _charged = true;
    protected class AfflictionData
    {
        public AfflictionData(float time_multiplier=1f)
        {
            ResetTime(time_multiplier);
            ResetTicker();
        }
        public float Affliction_time;
        public float Affliction_ticker;

        public void ResetTime(float time_multiplier=1f) { Affliction_time = 10.3f*time_multiplier; } //How long status applies
        public void ResetTicker() { Affliction_ticker = 1f; } //Time for damage ticks -> with affliction time = 10.3 -> 10 hits
    }
    private Dictionary<HealthManager, AfflictionData> _afflictedEnemies = new();
    private const float _damage_multiplier=0.5f; // based of nail damage
    private const float _deepfocus_damage_multiplier=0.77f;
    private const float _chargeTime = 10f;
    
    private bool _charged = false;
    private float _remainingChargeTime;
    
    public override string SpritePath  => "CharmIcons.MossMask"; //todo
    public override string Name => "Ashen Decay"; 
    public override string Description => "A charm manifested from the rotting remains of a higher being.\nWhen focusing it imbues the wielder's blade with a curse, which lets enemies become ash.";
    public override int DefaultCost => 3;
    public override string SceneName => "Deepnest_East_04";
    public override float X => 5.43f;
    public override float Y => 146.40f;
    
    public override CharmState State(LocalSettings s) => s.MossMask;

    public override void Hook()
    {
        //On.GameManager.EquipCharm += OnEquipCharm;
        //On.GameManager.UnequipCharm += OnUnequipCharm;
        On.HealthManager.Hit += OnEnemyHit;
        On.HeroController.Start += ModifySpellControlFsm;
        ModHooks.HeroUpdateHook += ModHooksOnHeroUpdateHook;
    }

    private void ModifySpellControlFsm(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        PlayMakerFSM spellControlFSM = self.gameObject.LocateMyFSM("Spell Control");
        FsmState focusGetFinish = spellControlFSM.GetState("Focus Get Finish");
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
        var toRemove = _afflictedEnemies
            .Where(e => e.Value.Affliction_time <= 0 || e.Key.isDead)
            .Select(e => e.Key)
            .ToList();
        foreach(var key in toRemove) _afflictedEnemies.Remove(key);
        foreach (var e in _afflictedEnemies)
        {
            e.Value.Affliction_time -= Time.deltaTime;
            e.Value.Affliction_ticker -= Time.deltaTime;
            if (e.Value.Affliction_ticker <= 0)
            {
                e.Value.ResetTicker();
                e.Key.Hit(new HitInstance()
                {
                    DamageDealt = PlayerData.instance.nailDamage,
                    Multiplier = PlayerData.instance.equippedCharm_34
                        ? _deepfocus_damage_multiplier
                        : _damage_multiplier,
                    AttackType = AttackTypes.Spell
                });
            }
        }
        
        if (_charged) _remainingChargeTime -= Time.deltaTime;
        if (_remainingChargeTime <= 0f) _charged = false;
        
    }
    
    private void OnEnemyHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
    {
        orig(self, hitInstance);
        if(!_charged || 
           !Equipped() || 
           !hitInstance.Source.gameObject.name.Contains("Slash")) return;
        if (_afflictedEnemies.TryGetValue(self, out AfflictionData data)) data.ResetTime(); 
        else _afflictedEnemies.Add(self, new ());
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