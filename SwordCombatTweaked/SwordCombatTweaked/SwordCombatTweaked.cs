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

namespace SwordCombatTweaked
{
    [MainModClass]
    public class SwordCombatTweaked : Mod
    {
        public SwordCombatTweaked() 
        {

        }


        protected override void OnCharacterUpdate(Character character)
        {
            base.OnCharacterUpdate(character);
            
        }

        protected override void OnModLoaded()
        {
            base.OnModLoaded();

        }

        public static Harmony harmonyInstance;

    }
}
