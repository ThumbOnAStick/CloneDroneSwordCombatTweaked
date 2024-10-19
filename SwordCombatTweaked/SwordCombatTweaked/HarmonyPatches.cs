using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InternalModBot;
using HarmonyLib;
using System.Reflection.Emit;
using ModLibrary;
using Bolt;
using UnityEngine;


namespace SwordCombatTweaked.HarmonyPatches
{
    [HarmonyPatch(typeof(FirstPersonMover), "RefreshUpgrades")]
    public static class SwordCutAreaPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();
            bool found = false;
            for (var i = 0; i < instructions.Count(); i++)
            {
                var opc = instructions.ElementAt(i).opcode;
                codes.Add(instructions.ElementAt(i));
                bool match = opc == OpCodes.Ldfld && instructions.ElementAt(i).operand.Equals(AccessTools.Field(typeof(BlockSwordsUpgrade), "BlockAreaScale"));
                if (match)
                {
                    found = true;
                }
                bool isStartingIndex = found && instructions.ElementAt(i).opcode == OpCodes.Mul;
                if (isStartingIndex)
                {
                    codes.Add(new CodeInstruction(OpCodes.Ldc_R4, 10f));
                    codes.Add(new CodeInstruction(OpCodes.Mul));
                    found = false;
                }
            }

            return codes.AsEnumerable();
        }
    }


    [HarmonyPatch(typeof(FirstPersonMover), "shouldCurrentWeaponBounceOffEnvironment")]
    public static class SwordBounceOffPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, ref FirstPersonMover __instance)
        {
            if (__instance.GetPrivateField<WeaponType>("_currentWeapon") == WeaponType.Sword)
                __result = false;
        }
    }



    [HarmonyPatch(typeof(SwordBlockArea), "OnTriggerEnter")]
    public static class TriggerEnterPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>() { new CodeInstruction(OpCodes.Ret) };

            return codes.AsEnumerable();
        }
    }


    [HarmonyPatch(typeof(SwordBlockArea), "TryProcessBlockCollision")]
    [HarmonyPatch(new Type[] {typeof(MeleeImpactArea), typeof(Vector3) })]
    public static class SwordCollisionPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref SwordBlockArea __instance)
        {
            __instance.BlockHammers = true;
        }
    }

    [HarmonyPatch(typeof(FirstPersonMover), "setUnbalanced")]
    public static class UnbalencePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            for (var i = 0; i < instructions.Count(); i++)
            {
                codes.Add(instructions.ElementAt(i));
                bool isStartingIndex = instructions.ElementAt(i).opcode == OpCodes.Mul;
                if (isStartingIndex)
                {
                    codes.Add(new CodeInstruction(OpCodes.Ldc_R4, 5f));
                    codes.Add(new CodeInstruction(OpCodes.Mul));
                }
            }

            return codes.AsEnumerable();
        }
    }


    //[HarmonyPatch(typeof(FirstPersonMover), "OnSwordHitSword")]
    //public static class SwordCollisionPatch
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(ref FirstPersonMover __instance, FirstPersonMover opponent)
    //    {
    //        if (!opponent.GetPrivateField<UpgradeCollection>("_upgradeCollection").HasUpgrade(UpgradeType.BlockSwords) || opponent.GetPrivateField<bool>("_isUnbalanced"))
    //        {
    //            return;
    //        }

    //        __instance.ResetKnockbackVelocity();
    //        Vector3 dir = (__instance.transform.position - opponent.transform.position).normalized;
    //        __instance.KnockBackCharacter(dir, 50f);

    //    }
    //}


}
