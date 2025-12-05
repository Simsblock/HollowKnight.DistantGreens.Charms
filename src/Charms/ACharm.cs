using System;
using System.Collections;
using System.Collections.Generic;
using DistantGreensCharms.Settings;

namespace DistantGreensCharms.Charms;

public abstract class ACharm
{
    // CREATE STATIC INSTANCE OF CHARM IN DERIVATIVE
    public abstract string SpritePath { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract int DefaultCost { get; }
    public abstract string SceneName { get; }
    public abstract float X { get; } //Coordinates in Scene
    public abstract float Y { get; } //Coordinates in Scene

    public virtual bool Godhome { get; } = true; //If Player receives Charm automatically in GodHome mode

    public abstract CharmState State(LocalSettings s);
    //public abstract void MarkAsEncountered(GlobalSettings s);
    
    // assigned at runtime by SFCore's CharmHelper
    public int Num { get; set; }
    
    public bool Equipped() => PlayerData.instance.GetBool($"equippedCharm_{Num}");

    public virtual void Hook() {}

    public virtual List<(string obj, string fsm, Action<PlayMakerFSM> edit)> FsmEdits => new(); //List for all FSM Edits a Charm needs to do, applied at Runtime
    public virtual List<(int Period, Action Func)> Tickers => new(); //List for all Function Calls which need to executed reguarly, 
}

public struct CharmState
{
    public bool Got; // If player has charm
    public bool Equipped;
    public bool New; // Newly aquired for New Notification in Inventory
    public int Cost; // Notches
}