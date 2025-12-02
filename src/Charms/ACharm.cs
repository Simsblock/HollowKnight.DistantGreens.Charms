using System;
using System.Collections.Generic;

namespace DistantGreensCharms.Charms;

public abstract class ACharm
{
    public abstract string Sprite { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract int DefaultCost { get; }
    public abstract string Scene { get; }
    public abstract float X { get; } //Coordinates in Scene
    public abstract float Y { get; } //Coordinates in Scene
    
    public CharmState State { get; set; }
    
    // assigned at runtime by SFCore's CharmHelper
    public int Num { get; set; }
    
    public bool Equipped() => PlayerData.instance.GetBool($"equippedCharm_{Num}");
    
    public virtual void Hook() {}
    public virtual List<(string obj, string fsm, Action<PlayMakerFSM> edit)> FsmEdits => new(); //List for all FSM Edits a Charm needs to do, applied at Runtime
    public virtual List<(int Period, Action Func)> Tickers => new(); //List for all Function Calls which need to executed reguarly, 
}

public struct CharmState
{
    public bool Obtained;
    public bool Equipped;
    public bool New; // Newly aquired for New Notification in Inventory
    public int Cost; // Notches
}