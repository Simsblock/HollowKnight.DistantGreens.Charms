using System;
using Modding;
using DebugMod;
using DistantGreensCharms.Charms;

namespace DistantGreensCharms.ModIntegrations.DebugMod;

public static class DebugModHelper
{
    public static void Hook()
    {
        if(ModHooks.GetMod("DebugMod") is null) return;
        BindableFunctions.OnGiveAllCharms += GiveAllCharms;
        BindableFunctions.OnRemoveAllCharms += RemoveAllCharms;
    }
    
    private static void GiveAllCharms()
    {
        foreach (ACharm charm in DistantGreensCharms.Charms)
        {
            DistantGreensCharms.Instance.BoolSetters.TryGetValue($"gotCharm_{charm.Num}", out Action<bool> f);
            f(true);
        }
    }
    private static void RemoveAllCharms()
    {
        foreach (ACharm charm in DistantGreensCharms.Charms)
        {
            DistantGreensCharms.Instance.BoolSetters.TryGetValue($"gotCharm_{charm.Num}", out Action<bool> f);
            f(false);
        }
    }
    
}