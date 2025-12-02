using System;
using System.Collections.Generic;

namespace DistantGreensCharms.Helper;

public static class FSMManager
{
    //For the non-enlightened FSM = Finite State Machine
    private static Dictionary<(string, string), Action<PlayMakerFSM>> FSMEdits = new();
    
    public static void AddFSMEdit(string obj, string fsm, Action<PlayMakerFSM> edit)
    {
        var key = (obj, fsm);
        var newEdit = edit;
        if (FSMEdits.TryGetValue(key, out var orig))
        {
            newEdit = fsm =>
            {
                orig(fsm);
                edit(fsm);
            };
        }
        FSMEdits[key] = newEdit;
    }

    public static void EditFSMs(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM fsm)
    {
        orig(fsm);
        if (FSMEdits.TryGetValue((fsm.gameObject.name, fsm.FsmName), out var edit))
        {
            edit(fsm);
        }
    }
}