﻿using System;

namespace MiscBitsAndBobs
{
    public class Config
    {
        private static Options _options;
        private static ConfigReader _con;

        [Serializable]
        public class Options
        {
            public int TavernInvIncrease;
            public bool EnableToolAndPrayerStacking;
            public bool ShowOnlyPersonalInventory;
            public bool DontShowEmptyRowsInInventory;
            public bool QuietMusicInGui;
            public bool AllowHandToolDestroy;
            public bool HalloweenNow;
            public bool HideCreditsButtonOnMainMenu;
            public bool CondenseXpBar;
        }

        public static Options GetOptions()
        {
            _options = new Options();
            _con = new ConfigReader();

            int.TryParse(_con.Value("TavernInvIncrease", "30"), out var tavernInvIncrease);
            _options.TavernInvIncrease = tavernInvIncrease;

            bool.TryParse(_con.Value("EnableToolAndPrayerStacking", "true"), out var enableToolAndPrayerStacking);
            _options.EnableToolAndPrayerStacking = enableToolAndPrayerStacking;

            bool.TryParse(_con.Value("QuietMusicInGUI", "true"), out var quietMusicInGUI);
            _options.QuietMusicInGui = quietMusicInGUI;

            bool.TryParse(_con.Value("CondenseXpBar", "true"), out var condenseXpBar);
            _options.CondenseXpBar = condenseXpBar;

            bool.TryParse(_con.Value("ShowOnlyPersonalInventory", "false"), out var showOnlyPersonalInventory);
            _options.ShowOnlyPersonalInventory = showOnlyPersonalInventory;

            bool.TryParse(_con.Value("DontShowEmptyRowsInInventory", "true"), out var dontShowEmptyRowsInInventory);
            _options.DontShowEmptyRowsInInventory = dontShowEmptyRowsInInventory;

            bool.TryParse(_con.Value("AllowHandToolDestroy", "true"), out var allowHandToolDestroy);
            _options.AllowHandToolDestroy = allowHandToolDestroy;

            bool.TryParse(_con.Value("HalloweenNow", "false"), out var halloweenNow);
            _options.HalloweenNow = halloweenNow;

            bool.TryParse(_con.Value("HideCreditsButtonOnMainMenu", "true"), out var hideCreditsButtonOnMainMenu);
            _options.HideCreditsButtonOnMainMenu = hideCreditsButtonOnMainMenu;

            _con.ConfigWrite();

            return _options;
        }
    }
}