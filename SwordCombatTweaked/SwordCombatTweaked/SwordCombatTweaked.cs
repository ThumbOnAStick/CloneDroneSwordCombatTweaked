using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using ModLibrary;
using SwordCombatTweaked.HarmonyPatches;
using UnityEngine;
using UnityEngine.UI;

namespace SwordCombatTweaked
{
    [MainModClass]
    public class SwordCombatTweaked : Mod
    {
        public SwordCombatTweaked()
        {

        }

        protected override bool ImplementsSettingsWindow()
        {
            return true;
        }
        protected override void CreateSettingsWindow(ModOptionsWindowBuilder builder)
        {
            ModOptionsWindowBuilder.Page page = builder.AddPage("Sword combat settings", null);
            page.AddSlider(
                1f,
                3f,
                1.5f,
                "sword swing speed",
                Settings.swingSettingId,
                onChange: (float f) => { Settings.speedCache = f; });

            page.AddCheckbox(
                true,
                "enable passive blocks",
                Settings.blockSettingId,
                onChange: (bool b) => { Settings.enablePassiveBlockCache = b; });

            page.AddCheckbox(
             true,
            "enable better parries",
            Settings.parrySettingId,
            onChange: (bool b) => { Settings.enableEasierParryCache = b; });

            page.AddCheckbox(
            true,
            "enable hammer blocking",
            Settings.hammerSettingId,
             onChange: (bool b) => { Settings.enableHammerBlockCache = b; });
        }

        protected override void OnModLoaded()
        {
            base.OnModLoaded();
            Settings.speedCache = ModdedSettings.GetModdedSettingsFloatValue(Settings.swingSettingId, 1.5f);
            Settings.enablePassiveBlockCache = ModdedSettings.GetModdedSettingsBoolValue(Settings.blockSettingId, true);
            Settings.enableEasierParryCache = ModdedSettings.GetModdedSettingsBoolValue(Settings.parrySettingId, true);
            Settings.enableHammerBlockCache = ModdedSettings.GetModdedSettingsBoolValue(Settings.hammerSettingId, true);

        }

        public static Harmony harmonyInstance;

    }
}
