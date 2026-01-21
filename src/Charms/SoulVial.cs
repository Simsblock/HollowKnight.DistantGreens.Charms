using System.Collections.Generic;
using System.Linq;
using DistantGreensCharms.Helper;
using DistantGreensCharms.Settings;
using HutongGames.PlayMaker;
using ItemChanger;
using Modding;
using RandomizerMod.RC;
using UnityEngine;
using ItemChanger.Extensions;
using SFCore.Utils;

namespace DistantGreensCharms.Charms;

public class SoulVial : ACharm
{
    public static SoulVial Instance = new();
    public override string SpritePath => "Charms.AshenDecay"; //Todo
    public override string Name => "Soul Vial";
    public override string Description => "Extra soul!"; //TODO
    public override int DefaultCost => 1;
    public override string SceneName => "Fungus3_50"; //TODO
    public override float X => 22.07f; //TODO
    public override float Y => 115.40f; //TODO
    public override CharmState State(LocalSettings s) => s.SoulVial;

    private int _focusMpAmount=33; //should be 33?
    private int _remainingMp=99;

    public override void Hook()
    {
        On.HeroController.Start += ModifySpellControlFsm;
        ModHooks.SetPlayerBoolHook += OnSetPlayerBool;
    }

    private void ModifySpellControlFsm(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        PlayMakerFSM spellControlFSM = self.gameObject.LocateMyFSM("Spell Control");
        FsmInt focusMpFsmInt = spellControlFSM.GetIntVariable("Focus MP amount");
        _focusMpAmount = focusMpFsmInt.Value;
        FsmState focusGetFinish = PlayMakerExtensions.GetState(spellControlFSM, "Can Focus?");
        //change bool Return bool, bool test to lead to my check if wrong instead and only if that also fails move on to Event(Cancel)
        
        //change still missing
        //index TODO
        PlayMakerExtensions.InsertAction(
            focusGetFinish, 
            new ExecuteLambda(() => {
            bool result = TryHeal();
            if (result) spellControlFSM.SendEvent("");
            else spellControlFSM.SendEvent("CANCEL"); }), 
            0
        );
    }
    
    private bool OnSetPlayerBool(string target, bool value)
    {
        if (target == "atBench" && value && Equipped())
        {
            SetCharged(true);
        }
        return value;
    }
    
    private void SetCharged(bool charged)
    {
        _remainingMp = 99;
        //HUD.UpdateSpriteState(charged);
    }

    private bool TryHeal()
    {
        if (!Equipped()) return false;
        bool val = _remainingMp - _focusMpAmount < 0;
        if (val) _remainingMp -= _focusMpAmount;
        return val;
    }
}