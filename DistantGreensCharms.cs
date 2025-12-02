using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DistantGreensCharms.Charms;
using DistantGreensCharms.Helper;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.UIDefs;
using SFCore;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace DistantGreensCharms
{
    internal class DistantGreensCharms() : Mod("DistantGreens.Charms") //name
    {
        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        
        
        internal static DistantGreensCharms Instance { get; private set; }
        
        internal static List<ACharm> Charms = new()
        {
            MossMask.Instance
        };
        
        public override void Initialize()
        {
            Log("Initializing");
            Instance = this;

            foreach (ACharm charm in Charms)
            {
                // Add charm through SFCore
                charm.Num = CharmHelper.AddSprites(SpriteManager.Get(charm.Sprite))[0];
                CharmState state = charm.State;
                // ...
                charm.Hook(); //Hook Charms Game Events
                
                // Update Finite State Machines
                foreach (var edit in charm.FsmEdits)
                {
                    FSMManager.AddFSMEdit(edit.obj, edit.fsm, edit.edit);
                }
                On.PlayMakerFSM.OnEnable += FSMManager.EditFSMs;

                // Tickers
                // todo
                
                // Add item which gives charm through ItemChanger
                string replacedCharmName = charm.Name.Replace('_', ' ');
                var item = new ItemChanger.Items.CharmItem()
                {
                    charmNum = charm.Num,
                    name = replacedCharmName,
                    UIDef = new MsgUIDef()
                    {
                        name = new LanguageString("UI", $"CHARM_NAME_{charm.Num}"),
                        shopDesc = new LanguageString("UI", $"CHARM_DESC_{charm.Num}"),
                        sprite = SpriteManager.CastToISprite(SpriteManager.Get(charm.Sprite))
                    }
                };
                Finder.DefineCustomItem(item);
                
                // Add location for Item through ItemChanger
                var location = new CoordinateLocation()
                {
                    x = charm.X,
                    y = charm.Y,
                    elevation = 0,
                    sceneName = charm.SceneName,
                    name = replacedCharmName
                };
                Finder.DefineCustomLocation(location);
                
                // add pins for item and location for map mods
                // todo
                
                // modhooks
                // todo
                
                // randomizer
                // todo
                
                //fixed location charms
                AbstractPlacement placement =
                    location
                        .Wrap()
                        .Add(item);
                ItemChangerMod.AddPlacements([placement], conflictResolution: PlacementConflictResolution.Ignore);
                
                //godhome mode assignment
                // todo
            }
        
            Log("Initialized");
        }
    }
}