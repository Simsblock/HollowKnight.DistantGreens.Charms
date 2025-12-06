using System.Reflection;
using DistantGreensCharms.Charms;
using RandomizerCore;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace DistantGreensCharms.Randomizer;

public static class LogicManager
{
    internal static void Hook()
    {
        RCData.RuntimeLogicOverride.Subscribe(50, ApplyLogic);
    }

    private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb)
    {
        foreach (ACharm charm in DistantGreensCharms.Charms)
        {
            // Cost Term Logic
            lmb.GetOrAddTerm(charm.DataName + "_COST");
            
            // Charm Term & Charm Logic
            Term term = lmb.GetOrAddTerm(charm.DataName);
            TermValue termCap = new(term, 1); //1 because Charms exist only once in game
            TermValue[] termEffects = new[] { termCap, new TermValue(lmb.GetTerm("CHARMS"), 1) };
            lmb.AddItem(new CappedItem(charm.DataName, termEffects, termCap));
        }
        
        // Location Logic -> easier to add through json -> because more locations than charms might exist, eg.: overrides
        Assembly asm = Assembly.GetExecutingAssembly();
        JsonLogicFormat jsonFmt = new();
        
        using (var s = asm.GetManifestResourceStream("DistantGreensCharms.Resources.LogicJsons.Locations.json"))
            lmb.DeserializeFile(LogicFileType.Locations, jsonFmt, s);
        
  

    }
    
    // If make this into library then this is a nescessary callable method
    
}