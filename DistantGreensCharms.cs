using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DistantGreensCharms.Charms;
using DistantGreensCharms.Helper;
using DistantGreensCharms.HUDElements;
using DistantGreensCharms.Settings;
using IL.HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.UIDefs;
using SFCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace DistantGreensCharms
{
    internal class DistantGreensCharms() : Mod("DistantGreens.Charms"), ILocalSettings<LocalSettings>//, IGlobalSettings<GlobalSettings> //name
    {
        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        
        
        internal static DistantGreensCharms Instance { get; private set; }
        internal static LocalSettings localSettings { get; private set; }
        internal static GlobalSettings globalSettings { get; private set; }
        
        internal static List<AHUDElement> AHUDElements = new()
        {
            MossMaskHUD.Instance
        };
        
        internal static List<ACharm> Charms = new()
        {
            MossMask.Instance
        };
        
        private Dictionary<string, Func<bool, bool>> BoolGetters = new(); //todo temp
        private Dictionary<string, Action<bool>> BoolSetters = new(); //todo temp
        private Dictionary<string, Func<int, int>> IntGetters = new(); //todo temp
        private Dictionary<string, Action<int>> IntSetters = new(); //todo temp
        private Dictionary<(string Key, string Sheet), Func<string?>> TextEdits = new(); //todo temp
        
        public override void Initialize()
        {
            Log("Initializing");

            Instance = this;
            HUDManager.Initialize();

            foreach (AHUDElement hudElement in AHUDElements)
            {
                //Hook HUDElement to HUDManager
                hudElement.Hook();
            }
            
            foreach (ACharm charm in Charms)
            {
                // Add charm through SFCore
                charm.Num = CharmHelper.AddSprites(SpriteManager.Get(charm.SpritePath))[0];
                CharmState state = charm.State(localSettings);
                state.Cost = charm.DefaultCost; // todo needs change maybe? 
                
                // HOOKS
                // todo TO CHANGE
                BoolGetters[$"equippedCharm_{charm.Num}"] = _ => state.Equipped;
                BoolSetters[$"equippedCharm_{charm.Num}"] = value => state.Equipped = value;
                IntGetters[$"charmCost_{charm.Num}"] = _ => state.Cost;
                IntSetters[$"charmCost_{charm.Num}"] = value => state.Cost = value;
                BoolGetters[$"gotCharm_{charm.Num}"] = _ => state.Got;
                BoolSetters[$"gotCharm_{charm.Num}"] = value =>
                {
                    state.Got = value;
                    /*if (value)
                    {
                        charm.MarkAsEncountered(globalSettings);
                    }*/
                };
                BoolGetters[$"newCharm_{charm.Num}"] = _ => state.New;
                BoolSetters[$"newCharm_{charm.Num}"] = value => state.New = value;
                TextEdits[(Key: $"CHARM_NAME_{charm.Num}", Sheet: "UI")] = () => charm.Name;
                TextEdits[(Key: $"CHARM_DESC_{charm.Num}", Sheet: "UI")] = () => charm.Description;
                    
                charm.Hook(); //Hook Charms Game Events
                
                // Update Finite State Machines
                foreach (var edit in charm.FsmEdits)
                {
                    FSMManager.AddFSMEdit(edit.obj, edit.fsm, edit.edit);
                }

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
                        sprite = SpriteManager.CastToISprite(SpriteManager.Get(charm.SpritePath))
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
                
                // randomizer
                // todo
                
                //location fixed
                AbstractPlacements.Add(
                    location
                        .Wrap()
                        .Add(item)

                );

                //godhome mode assignment
                // todo
            }
            // modhooks
            ModHooks.GetPlayerBoolHook += ReadCharmBools;
            ModHooks.SetPlayerBoolHook += WriteCharmBools;
            ModHooks.GetPlayerIntHook += ReadCharmCosts;
                
            ModHooks.SetPlayerIntHook += WriteCharmCosts;
            ModHooks.LanguageGetHook += GetCharmStrings;
                
            On.UIManager.StartNewGame += PlaceItem;
            On.PlayMakerFSM.OnEnable += FSMManager.EditFSMs;
                
            On.PlayerData.CountCharms += CountOurCharms;
            On.PlayMakerFSM.OnEnable += FSMManager.EditFSMs;
        
            Log("Initialized");
        }

        public List<AbstractPlacement> AbstractPlacements = new();
        public void PlaceAtAbstractLocations() //temporary method, might be relocated
        {
            ItemChangerMod.AddPlacements(AbstractPlacements, conflictResolution: PlacementConflictResolution.Ignore);
        }

        private void PlaceItem(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            PlaceAtAbstractLocations();
            orig(self, permaDeath, bossRush);
        }
        
        private void CountOurCharms(On.PlayerData.orig_CountCharms orig, PlayerData self)
        {
            orig(self);
            self.SetInt("charmsOwned", self.GetInt("charmsOwned") + Charms.Count);
        }
        
        private bool ReadCharmBools(string boolName, bool value)
        {
            if (BoolGetters.TryGetValue(boolName, out var f))
            {
                return f(value);
            }
            return value;
        }

        private bool WriteCharmBools(string boolName, bool value)
        {
            if (BoolSetters.TryGetValue(boolName, out var f))
            {
                f(value);
            }
            return value;
        }

        private int ReadCharmCosts(string intName, int value)
        {
            if (IntGetters.TryGetValue(intName, out var f))
            {
                return f(value);
            }
            return value;
        }
        
        private int WriteCharmCosts(string intName, int value)
        {
            if (IntSetters.TryGetValue(intName, out var f))
            {
                f(value);
                return value;
            }
            return value;
        }
        
        private string GetCharmStrings(string key, string sheetName, string orig) =>
            TextEdits.TryGetValue((key, sheetName), out var text) ? (text() ?? orig) : orig;

        //Localsettings
        public void OnLoadLocal(LocalSettings s)
        {
            localSettings = s;
        }

        public LocalSettings OnSaveLocal()
        {
            return localSettings;
        }

        //Globalsettings
        public void OnLoadGlobal(GlobalSettings s)
        {
            globalSettings = s;
        }

        public GlobalSettings OnSaveGlobal()
        {
            return globalSettings;
        }
    }
}