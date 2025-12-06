using System;
using System.Collections.Generic;
using System.Linq;
using DistantGreensCharms.Charms;
using ItemChanger;
using Mono.Security.X509;
using RandomizerCore.Logic;
using RandomizerMod.Menu;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace DistantGreensCharms.Randomizer;

public static class RandomizerManager
{
    public static void Hook()
    {
        // Hook to Randomizer
        RequestBuilder.OnUpdate.Subscribe(-9999f, SetNotchCosts);
        RequestBuilder.OnUpdate.Subscribe(-498f, DefineCharmsForRandomizer);
        RequestBuilder.OnUpdate.Subscribe(50, AddCharmsToPool);
        
        RandoController.OnExportCompleted += StoreRandoNotchCosts;
        
        //SettingsLog.AfterLogSettings += LogRandoSettings; //Should add logging :3
    }

    //Notch Costs
    private static void SetNotchCosts(RequestBuilder rb)
    {
        var nc = rb.gs.MiscSettings.RandomizeNotchCosts ? RandomizeNotchCosts(rb, rb.gs.Seed) : DefaultNotchCosts();
        var pinit = (ProgressionInitializer)rb.ctx.InitialProgression;
        foreach (var charm in DistantGreensCharms.Charms)
        {
            pinit.Setters.Add(new(
                rb.lm.GetTermStrict(charm.DataName + "_COST"), nc[charm.Num]
                )
            );
        }
    }

    private static Dictionary<int,int> RandomizeNotchCosts(RequestBuilder rb, int gsSeed)
    {
        Random rng = new Random(gsSeed);
        int totalNotches = 0;
        DistantGreensCharms.Charms.ForEach(c => totalNotches += c.DefaultCost);
        int maxTotal = rb.gs.MiscSettings.MaxRandomNotchTotal += totalNotches;
        int minTotal = rb.gs.MiscSettings.MinRandomNotchTotal += totalNotches;
        int vanillaTotal = rb.ctx.notchCosts.Sum();
        int variance = maxTotal - minTotal;
        
        int minCost = Math.Max(0, (vanillaTotal - variance) / 10);
        int maxCost = Math.Min(totalNotches * 6, (vanillaTotal + variance) / 10);
        int randomNotchAmount = rng.Next(minCost, maxCost + 1); //The Amount of Notches which will be distributed
        Dictionary<int,int> charmCosts = DistantGreensCharms.Charms.ToDictionary(charm => charm.Num, charm => 0);

        for (int i = 0; i < randomNotchAmount; i++)
        {
            List<int> possiblePicks = charmCosts
                .Where(charm => charm.Value < 6)
                .Select(charm => charm.Key)
                .ToList();
            if (possiblePicks.Count == 0) break;
            int pick = rng.Next(possiblePicks.Count);
            charmCosts[possiblePicks[pick]]++;
        }
        return charmCosts;
    }

    private static Dictionary<int, int> DefaultNotchCosts()
    {
        Dictionary<int,int> charmCosts = DistantGreensCharms.Charms.ToDictionary(charm => charm.Num, charm => charm.DefaultCost);
        return charmCosts;
    }
    
    // Set Random Notch Cost for Charm sold in Shop
    private static void StoreRandoNotchCosts(RandoController rc)
    {
        var pinit = (ProgressionInitializer)rc.ctx.InitialProgression;
        var icPlayerData = ItemChangerMod.Modules.GetOrAdd<ItemChanger.Modules.PlayerDataEditModule>();
        foreach (ACharm charm in DistantGreensCharms.Charms)
        {
            var t = rc.rb.lm.GetTermStrict(charm.DataName + "_COST");
            var cost = pinit.Setters.First(s => s.Term == t).Value;
            icPlayerData.AddPDEdit($"charmCost_{charm.Num}", cost);
        }
    }
    
    //Define Charms
    private static void DefineCharmsForRandomizer(RequestBuilder rb)
    {
        if(!RandomizerConnectionMenu.Instance.AddCharms) return;
        var charmNames = new HashSet<string>();
        foreach (ACharm charm in DistantGreensCharms.Charms)
        {
            string charmName = charm.DataName;
            charmNames.Add(charmName);
            rb.EditItemRequest(charmName, info =>
                info.getItemDef = () => new()
                {
                    Name = charmName,
                    Pool = "Charm,",
                    MajorItem = false,
                    PriceCap = 666 //Should make adjustable per charm in ACHarm
                }
                );
        }
        rb.OnGetGroupFor.Subscribe(0f,
            (RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb) =>
            {
                if (charmNames.Contains(item) && 
                    (type == RequestBuilder.ElementType.Unknown || type == RequestBuilder.ElementType.Item))
                {
                    gb = rb.GetGroupFor("Shaman_Stone"); //Get Group for Charms
                    return true;
                }

                gb = default;
                return false;
            });
    }
    
    //Add charms & locations into pool
    private static void AddCharmsToPool(RequestBuilder rb)
    {
        if(!(rb.gs.PoolSettings.Charms && RandomizerConnectionMenu.Instance.AddCharms)) return;
        foreach (ACharm charm in DistantGreensCharms.Charms)
        {
            rb.AddItemByName(charm.DataName);
            rb.AddLocationByName(charm.DataName);
        }
    }
    
    private static void HandleRequest(RequestBuilder rb)
    {
        // Check if charms should be randomized
        if (!rb.gs.PoolSettings.Charms || !RandomizerConnectionMenu.Instance.AddCharms) return;
        AddCharmsToPool(rb);
    }
    
}