using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DistantGreensCharms.Charms;
using DistantGreensCharms.Helper;
using DistantGreensCharms.HUDElements;
using DistantGreensCharms.ModIntegrations.DebugMod;
using DistantGreensCharms.Settings;
using IL.HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.UIDefs;
using RandomizerMod.Menu;
using SFCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using DistantGreensCharms.Randomizer;
using InControl;
using ItemChanger.Tags;

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
        //internal static GlobalSettings globalSettings { get; private set; }
        
        internal static List<AHUDElement> AHUDElements = new()
        {
            MossMaskHUD.Instance
        };
        
        internal static List<ACharm> Charms = new()
        {
            MossMask.Instance,
            Painters_Spirit.Instance
        };
        
        internal Dictionary<string, Func<bool, bool>> BoolGetters = new();
        internal Dictionary<string, Action<bool>> BoolSetters = new();
        internal Dictionary<string, Func<int, int>> IntGetters = new();
        internal Dictionary<string, Action<int>> IntSetters = new();
        internal Dictionary<(string Key, string Sheet), Func<string?>> TextEdits = new();
        
        public override void Initialize()
        {
            Log("Initializing");

            Instance = this;
            HUDManager.Hook();

            foreach (AHUDElement hudElement in AHUDElements)
            {
                //Hook HUDElement to HUDManager
                hudElement.Hook();
            }
            
            foreach (ACharm charm in Charms)
            {
                // Add charm through SFCore
                int num = CharmHelper.AddSprites(SpriteManager.Get(charm.SpritePath))[0];
                charm.Num = num;
                CharmState state = charm.State(localSettings);
                state.Cost = charm.DefaultCost; // todo needs change maybe? 
                
                // Delegates for Hooks
                // todo TO CHANGE
                BoolGetters[$"equippedCharm_{num}"] = _ => state.Equipped;
                BoolSetters[$"equippedCharm_{num}"] = value => state.Equipped = value;
                IntGetters[$"charmCost_{num}"] = _ => state.Cost;
                IntSetters[$"charmCost_{num}"] = value => state.Cost = value;
                BoolGetters[$"gotCharm_{num}"] = _ => state.Got;
                BoolSetters[$"gotCharm_{num}"] = value => state.Got = value;
                BoolGetters[$"newCharm_{num}"] = _ => state.New;
                BoolSetters[$"newCharm_{num}"] = value => state.New = value;
                TextEdits[(Key: $"CHARM_NAME_{num}", Sheet: "UI")] = () => charm.Name;
                TextEdits[(Key: $"CHARM_DESC_{num}", Sheet: "UI")] = () => charm.Description;
                    
                charm.Hook(); //Hook Charms Game Events
                
                // Update Finite State Machines
                foreach (var edit in charm.FsmEdits)
                {
                    FSMManager.AddFSMEdit(edit.obj, edit.fsm, edit.edit);
                }

                // Tickers
                // todo
                
                // Add item which gives charm through ItemChanger
                var item = new ItemChanger.Items.CharmItem()
                {
                    charmNum = num,
                    name = charm.DataName,
                    UIDef = new MsgUIDef()
                    {
                        name = new LanguageString("UI", $"CHARM_NAME_{num}"),
                        shopDesc = new LanguageString("UI", $"CHARM_DESC_{num}"),
                        sprite = SpriteManager.CastToISprite(SpriteManager.Get(charm.SpritePath))
                    }
                };
                InteropTag itemTag = item.AddTag<InteropTag>();
                itemTag.Message = charm.DataName;
                itemTag.Properties["ModSource"] = "DistantGreensCharms";
                itemTag.Properties["PoolGroup"] = "Charms";
                Finder.DefineCustomItem(item);
                
                // Add location for Item through ItemChanger
                // Item for location assigned later, because of Randomizer 4 Implementation!
                var location = new CoordinateLocation()
                {
                    name = charm.DataName,
                    sceneName = charm.SceneName,
                    x = charm.X,
                    y = charm.Y,
                    elevation = 0f
                };
                InteropTag LocationTag = item.AddTag<InteropTag>();
                itemTag.Message = $"{charm.DataName}_Location";
                itemTag.Properties["ModSource"] = "DistantGreensCharms";
                itemTag.Properties["PoolGroup"] = "Charms";
                Finder.DefineCustomLocation(location);
                Locations.Add(charm, (item, location));

                
                //Integrations
                if (ModHooks.GetMod("MenuChanger") != null && ModHooks.GetMod("Randomizer 4") != null)
                {
                    RandomizerManager.Hook();
                }
                
                if (ModHooks.GetMod("DebugMod") != null)
                {
                    DebugModHelper.Hook();
                }
            }
            // Modhooks
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
        public Dictionary<ACharm,(ItemChanger.Items.CharmItem, CoordinateLocation)> Locations = new();
        private void PlaceItem(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            bool isRandomized = false;
            if (ModHooks.GetMod("MenuChanger") != null && ModHooks.GetMod("Randomizer 4") != null)
            {
                isRandomized = RandomizerMod.RandomizerMod.RS?.GenerationSettings != null;
            }
            
            if (isRandomized)
            {
                List<AbstractPlacement> placements = new();
                foreach ((ItemChanger.Items.CharmItem, CoordinateLocation) pair in Locations.Values)
                {
                    placements.Add(
                        pair.Item2
                            .Wrap()
                    );
                }
                PlaceAtAbstractLocations(placements);
                 //maybe Add something for things that need certain locations, LIKE repairs
            }
            else if (bossRush)
            {
                IEnumerable<ACharm> gg_Charms = Charms.Where(charm => charm.Godhome);
                foreach (ACharm charm in gg_Charms)
                {
                    BoolSetters.TryGetValue($"gotCharm_{charm.Num}", out Action<bool> f);
                    f(true);
                }

                List<AbstractPlacement> placements = new();
                IEnumerable<(ItemChanger.Items.CharmItem, CoordinateLocation)> charmsToPlace = Locations.Where(_ => !gg_Charms.Contains(_.Key)).Select(_ => _.Value);
                foreach ((ItemChanger.Items.CharmItem, CoordinateLocation) pair in charmsToPlace)
                {
                    placements.Add(
                        pair.Item2
                        .Wrap()
                        .Add(pair.Item1)
                    );
                }
            }
            
            else
            {
                List<AbstractPlacement> placements = new();
                foreach ((ItemChanger.Items.CharmItem, CoordinateLocation) pair in Locations.Values)
                {
                    placements.Add(
                        pair.Item2
                            .Wrap()
                            .Add(pair.Item1)
                    );
                }
                PlaceAtAbstractLocations(placements);
            }
            orig(self, permaDeath, bossRush);
        }
        private void PlaceAtAbstractLocations(IEnumerable<AbstractPlacement> placements)
        {
            ItemChangerMod.AddPlacements(placements, conflictResolution: PlacementConflictResolution.Ignore);
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
        /*
        public void OnLoadGlobal(GlobalSettings s)
        {
            globalSettings = s;
        }

        public GlobalSettings OnSaveGlobal()
        {
            return globalSettings;
        }
        */
    }
}