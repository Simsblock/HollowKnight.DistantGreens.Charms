using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.Extensions;
using RandomizerMod.Menu;
using System;
using MenuChanger.MenuPanels;
using MenuChanger.Attributes;
using UnityEngine;

namespace DistantGreensCharms.Randomizer
{
    public class RandomizerConnectionMenu
    {
        public bool AddCharms { get; set; } = true;
        public bool UseRandomizerCosts { get; set; } = true;
        public int MaxCharmCostIncrease { get; set; } = 0; // Increase Salubra requirement

        private MenuPage connectionPage;
        private SmallButton connectionButton;

        public static RandomizerConnectionMenu Instance { get; private set; } = new();

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(Instance.BuildConnectionMenu, Instance.TryGetConnectionButton);
        }

        private bool TryGetConnectionButton(MenuPage landingPage, out SmallButton button)
        {
            button = Instance.connectionButton;
            button.OnClick += () => landingPage.TransitionTo(Instance.connectionPage);
            return true;
        }

        private void BuildConnectionMenu(MenuPage landingPage) // connection button doesnt show right!
        {
            Instance.connectionPage = new("DistantGreensCharms", landingPage);
            
            var addCharmsToggle = new ToggleButton(connectionPage, "Enabled");
            addCharmsToggle.SetValue(Instance.AddCharms);
            addCharmsToggle.ValueChanged += (val) => Instance.AddCharms = val;
            
            // todo Description
            var useRandoCostsToggle = new ToggleButton(connectionPage, "Use Randomizer Costs");
            useRandoCostsToggle.SetValue(Instance.UseRandomizerCosts);
            useRandoCostsToggle.ValueChanged += (val) => Instance.UseRandomizerCosts = val;
            
            var infoLabel = new MenuLabel(connectionPage, 
                "When enabled, adds your custom charms to the randomizer pool. " +
                "Randomizer costs allow items to have different costs than in vanilla.", 
                MenuLabel.Style.Body);
            
            new VerticalItemPanel(connectionPage, 
                new Vector2(0, 300), 
                75f, 
                true, 
                addCharmsToggle, 
                useRandoCostsToggle, 
                infoLabel
            );
            
            Instance.connectionButton = new SmallButton(landingPage, "DistantGreensCharms");
            //addCharmsToggle.SetValue(Instance.AddCharms);
        }
    }
}