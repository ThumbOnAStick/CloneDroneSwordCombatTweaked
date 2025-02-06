using ModLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwordCombatTweaked
{
    internal static class Settings
    {
        public static float speedCache;
        public static bool enablePassiveBlockCache;
        public static bool enableEasierParryCache;
        public static bool enableHammerBlockCache;

        public static readonly string blockSettingId = "EnablePassiveBlocks";
        public static readonly string swingSettingId = "SwingFactor";
        public static readonly string parrySettingId = "ParryFactor";
        public static readonly string hammerSettingId = "HammerBlockFactor";


        public static bool EnablePassiveBlock => enablePassiveBlockCache;
        public static bool EnableEasierParry => enableEasierParryCache;
        public static bool EnableHammerBlock => enableHammerBlockCache;
        public static float SwordSwinSpeed => speedCache;

    }
}

