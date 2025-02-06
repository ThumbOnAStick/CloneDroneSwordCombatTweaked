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
using System.Reflection;


namespace SwordCombatTweaked.HarmonyPatches
{

    [HarmonyPatch(typeof(AISwordsmanController), "ShouldBeMovingForward")]
    public static class ShouldMoveForwardPatch
    {
        [HarmonyPostfix]
        public static void Postfix(float distanceToTarget, ref bool __result, ref AISwordsmanController __instance)
        {
            Transform targetTransform = __instance.GetPrivateField<Transform>("_targetOpponentTransform");
            if (targetTransform == null)
            {
                return;
            }
            if (__instance != null && targetTransform != null &&
           distanceToTarget < 4f && __instance.EquippedWeapon == WeaponType.Sword)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(AISwordsmanController), "ShouldBeMovingBack")]
    public static class ShouldMoveBackPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, ref AISwordsmanController __instance)
        {
            Transform targetTransform = __instance.GetPrivateField<Transform>("_targetOpponentTransform");
            if (targetTransform == null)
            {
                return;
            }
            FirstPersonMover mover = __instance.GetPrivateField<FirstPersonMover>("_firstPersonMover");
            if (mover == null)
            {
                return;
            }
            Character character = __instance.GetCharacter();
            if (character == null)
            {
                return;
            }
            int spawnCost = character.SpawnCost;
            float retreatAmount = 0f;
            if (spawnCost > 200)
            {
                retreatAmount = 3.75f;
            }
            else if (spawnCost > 100)
            {
                retreatAmount = 3f;
            }

            if (__instance != null && targetTransform != null &&
               Vector3.Distance(__instance.transform.position, targetTransform.position)
               < retreatAmount && __instance.EquippedWeapon == WeaponType.Sword)
            {
                __result = true;
            }
        }
    }


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
                    float num = Settings.EnableEasierParry ? 10f : 1f;
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

    [HarmonyPatch(typeof(FirstPersonMover), "Awake")]
    public static class AwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref FirstPersonMover __instance)
        {
            PassiveBlockManager.TryToRegister(__instance);
            Debug.Log($"{__instance.gameObject.name} registered!");
        }
    }

    [HarmonyPatch(typeof(FirstPersonMover), "OnDestroy")]
    public static class DestroyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref FirstPersonMover __instance)
        {
            PassiveBlockManager.RemoveFromTable(__instance);
        }
    }

    [HarmonyPatch(typeof(MeleeImpactArea), "onMechBodyPartTriggerEnter")]
    public static class TriggerEnterPatch1
    {
        [HarmonyPostfix]
        public static void Postfix(BaseBodyPart bodyPart, ref MeleeImpactArea __instance)
        {
            if (!Settings.EnablePassiveBlock)
            {
                return;
            }
            if (bodyPart == null)
            {
                return;
            }
            var sword = __instance as SwordHitArea;
            if (sword == null)
            {
                return;
            }
            var area = bodyPart.GetSwordBlockArea();
            if (area != null && area.gameObject.activeInHierarchy)
            {
                return;
            }
            var owner = bodyPart.GetOwner() as FirstPersonMover;

            if (owner == null || owner == __instance.Owner)
            {
                return;
            }

            if (!__instance.IsDamageActive())
            {
                return;
            }
            if (owner.GetEquippedWeaponType() != WeaponType.Sword)
            {
                return;
            }
            if (PassiveBlockManager.TryToUse(owner, out int remains))
            {
                PassiveBlockManager.BounceOpponent(owner);
                PassiveBlockManager.BounceSelf(ref sword, owner);
                debug.Log($"Passive attack used! client: {owner.gameObject.name}, remaining usage: {remains}");
            }
        }
    }

    [HarmonyPatch(typeof(SwordBlockArea), "OnTriggerEnter")]
    public static class TriggerEnterPatch2
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>() { new CodeInstruction(OpCodes.Ret) };
            return codes.AsEnumerable();
        }
    }


    [HarmonyPatch(typeof(SwordBlockArea), "TryProcessBlockCollision")]
    [HarmonyPatch(new Type[] { typeof(MeleeImpactArea), typeof(Vector3) })]
    public static class SwordCollisionPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref SwordBlockArea __instance)
        {
            __instance.BlockHammers = Settings.EnableHammerBlock;
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


    [HarmonyPatch(typeof(FirstPersonMover), "OnSwordHitSword")]
    public static class SwordHitSwordPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref FirstPersonMover __instance, FirstPersonMover opponent)
        {
            __instance.ResetKnockbackVelocity();
            Vector3 dir = (__instance.transform.position - opponent.transform.position).normalized;

            if (!opponent.GetPrivateField<UpgradeCollection>("_upgradeCollection").HasUpgrade(UpgradeType.BlockSwords) ||
                !__instance.GetPrivateField<bool>("_isUnbalanced"))
            {
                __instance.SetVelocity(dir * 10);
                return;
            }

            dir.y = 0;
            __instance.SetVelocity(dir * 20);
            Singleton<AudioManager>.Instance.PlayClipAtTransform(Singleton<AudioLibrary>.Instance.SwordBlocks,
                __instance.transform, 0f, false, 1f, .8f);
        }
    }


    [HarmonyPatch(typeof(FirstPersonMover), "GetAttackSpeed")]
    public static class AttackSpeedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref FirstPersonMover __instance, ref float __result)
        {
            if (__instance.GetEquippedWeaponType() == WeaponType.Sword)
            {
                __result *= Settings.SwordSwinSpeed;
            }
        }
    }

}
