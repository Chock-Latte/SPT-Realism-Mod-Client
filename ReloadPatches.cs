﻿using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static EFT.Player;

namespace RealismMod
{

    public class SetLauncherPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetLauncher", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(bool isLauncherEnabled)
        {
            Plugin.LauncherIsActive = isLauncherEnabled;
        }
    }

    public class SetWeaponLevelPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetWeaponLevel", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance, float weaponLevel)
        {
            if (WeaponProperties._WeapClass == "shotgun")
            {
                if (weaponLevel < 3)
                {
                    weaponLevel += 1;
                }
                WeaponAnimationSpeedControllerClass.SetWeaponLevel(__instance.Animator, weaponLevel);
            }

        }
    }

    public class SetHammerArmedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetHammerArmed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {

            if (Plugin.IsFiring != true && Helper.IsInReloadOpertation)
            {
                __instance.SetAnimationSpeed(Mathf.Clamp(WeaponProperties.TotalChamberSpeed * WeaponProperties.ArmHammerSpeedMulti * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti, 0.5f, 1.5f));
            }
        }
    }

    public class CheckAmmoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("CheckAmmo", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            float bonus = WeaponProperties.CheckAmmoMulti;
            if (WeaponProperties._WeapClass == "pistol")
            {
                bonus = WeaponProperties.CheckAmmoPistolSpeedMulti;
            }
            __instance.SetAnimationSpeed(Mathf.Clamp(WeaponProperties.CurrentMagReloadSpeed * bonus * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti, 0.5f, 1.5f));
        }
    }

    public class CheckChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("CheckChamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("///////////////////Check Chamber///////////////");
            float chamberSpeed = WeaponProperties.TotalChamberSpeed;
            if (WeaponProperties._WeapClass == "pistol")
            {
                chamberSpeed *= WeaponProperties.CheckChamberPistolSpeedMulti;
            }
            else if (WeaponProperties._WeapClass == "shotgun")
            {
                chamberSpeed *= WeaponProperties.CheckChamberShotgunSpeedMulti;
            }
            else
            {
                chamberSpeed *= WeaponProperties.CheckChamberSpeedMulti;
            }

            Logger.LogWarning("chamberSpeed = " + chamberSpeed);
            Logger.LogWarning("total chamber speed = " + chamberSpeed * PlayerProperties.FixSkillMulti * PlayerProperties.ReloadInjuryMulti);
            Logger.LogWarning("///////////////////Check Chamber///////////////");

            __instance.SetAnimationSpeed(Mathf.Clamp(chamberSpeed * PlayerProperties.FixSkillMulti * PlayerProperties.ReloadInjuryMulti, 0.5f, 1.5f));
        }
    }

    public class SetBoltActionReloadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetBoltActionReload", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {

            if (WeaponProperties._IsManuallyOperated == true || Plugin.LauncherIsActive == true)
            {
                Logger.LogWarning("///////////////////Bolt Action Reload////////////////");
                float chamberSpeed = WeaponProperties.TotalChamberSpeed;
                if (WeaponProperties._WeapClass == "shotgun")
                {
                    chamberSpeed *= WeaponProperties.ShotgunRackSpeedFactor;
                }
                if (Plugin.LauncherIsActive == true)
                {
                    chamberSpeed *= WeaponProperties.GlobalUBGLReloadMulti;
                }
                if (WeaponProperties._WeapClass == "sniperRifle")
                {
                    chamberSpeed *= WeaponProperties.GlobalBoltSpeedMulti;
                }
                Logger.LogWarning("Injury Multi = " + PlayerProperties.ReloadInjuryMulti);
                Logger.LogWarning("Reload Skill = " + PlayerProperties.ReloadSkillMulti);
                Logger.LogWarning("Base Chamber Speed = " + WeaponProperties.TotalChamberSpeed);
                Logger.LogWarning("Total Chamber Speed = " + (chamberSpeed * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti));
                Logger.LogWarning("///////////////////////////////////");
                __instance.SetAnimationSpeed(Mathf.Clamp(chamberSpeed * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti, 0.5f, 1.5f));
            }
        }
    }

    public class SetMalfRepairSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMalfRepairSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, float fix)
        {
            Logger.LogWarning("Fix Speed = " + fix);
            Logger.LogWarning("Total Fix Speed = " + (fix + WeaponProperties.TotalChamberSpeed) * (PlayerProperties.ReloadInjuryMulti * WeaponProperties.GlobalFixSpeedMulti));
            WeaponAnimationSpeedControllerClass.SetSpeedFix(__instance.Animator, (fix + WeaponProperties.TotalChamberSpeed) * (PlayerProperties.ReloadInjuryMulti * WeaponProperties.GlobalFixSpeedMulti));
            __instance.SetAnimationSpeed(Mathf.Clamp((fix + WeaponProperties.TotalChamberSpeed) * (PlayerProperties.ReloadInjuryMulti * WeaponProperties.GlobalFixSpeedMulti), 0.5f, 1.5f));
        }
    }

    public class RechamberSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("Rechamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            float chamberSpeed = WeaponProperties.TotalChamberSpeed;
            if (WeaponProperties._WeapClass == "pistol")
            {
                chamberSpeed *= WeaponProperties.RechamberPistolSpeedMulti;
            }

            else
            {
                chamberSpeed *= WeaponProperties.RechamberSpeedMulti;
            }
            __instance.SetAnimationSpeed(Mathf.Clamp(chamberSpeed * PlayerProperties.FixSkillMulti * PlayerProperties.ReloadInjuryMulti, 0.5f, 1.5f));
        }
    }

    public class CanStartReloadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("CanStartReload", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, bool __result)
        {
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                if (__result == true)
                {
                    if (__instance.Item.GetCurrentMagazine() == null)
                    {
                        Helper.NoMagazineReload = true;
                    }
                    else
                    {
                        Helper.NoMagazineReload = false;
                    }
                }
            }
        }
    }

    public class ReloadMagPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("ReloadMag", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, MagazineClass magazine)
        {
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                StatCalc.SetMagReloadSpeeds(Logger, __instance, magazine);
            }
        }
    }


    public class QuickReloadMagPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("QuickReloadMag", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, MagazineClass magazine)
        {
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                StatCalc.SetMagReloadSpeeds(Logger, __instance, magazine, true);
            }
        }
    }


    public class ReloadRevolverDrumPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("ReloadRevolverDrum", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                Helper.IsAttemptingToReloadInternalMag = true;
                Helper.IsAttemptingRevolverReload = true;
            }
        }
    }

    public class ReloadWithAmmoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("ReloadWithAmmo", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                Helper.IsAttemptingToReloadInternalMag = true;
            }
        }
    }

    public class ReloadBarrelsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("ReloadBarrels", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                Helper.IsAttemptingToReloadInternalMag = true;
            }
        }
    }


    public class SetMagTypeNewPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMagTypeNew", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("////////////////Set Mag Type New///////////////");
            Logger.LogWarning("CurrentMagReloadSpeed = + " + WeaponProperties.CurrentMagReloadSpeed);
            Logger.LogWarning("Total Reload speed = + " + WeaponProperties.CurrentMagReloadSpeed * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti);
            Logger.LogWarning("///////////////////////////");

            __instance.SetAnimationSpeed(Mathf.Clamp(WeaponProperties.CurrentMagReloadSpeed * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti, 0.5f, 1.5f));
        }
    }

    public class SetMagTypeCurrentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMagTypeCurrent", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("////////////////SetMagTypeCurrent///////////////");
            Logger.LogWarning("CurrentMagReloadSpeed = + " + WeaponProperties.CurrentMagReloadSpeed);
            Logger.LogWarning("Total Reload speed = + " + WeaponProperties.CurrentMagReloadSpeed * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti);
            Logger.LogWarning("///////////////////////////");
            __instance.SetAnimationSpeed(Mathf.Clamp(WeaponProperties.CurrentMagReloadSpeed * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti, 0.5f, 1.5f));
        }
    }

    public class SetMagInWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMagInWeapon", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            if (Helper.IsMagReloading == true)
            {
                Logger.LogWarning("////////////////SetMagInWeapon///////////////");
                Logger.LogWarning("CurrentMagReloadSpeed = + " + WeaponProperties.CurrentMagReloadSpeed);
                Logger.LogWarning("Total Reload speed = + " + WeaponProperties.CurrentMagReloadSpeed * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti);
                Logger.LogWarning("///////////////////////////");
                __instance.SetAnimationSpeed(Mathf.Clamp(WeaponProperties.NewMagReloadSpeed * PlayerProperties.ReloadSkillMulti * PlayerProperties.ReloadInjuryMulti * PlayerProperties.GearReloadMulti, 0.5f, 1.5f));
            }
        }
    }

    public class SetSpeedParametersPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetSpeedParameters", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            __instance.SetAnimationSpeed(1);
        }
    }


    public class OnMagInsertedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("method_47", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player.FirearmController __instance)
        {
            //to find this again, look for private void method_47(){ this.CurrentOperation.OnMagInsertedToWeapon(); }
            Player player = (Player)AccessTools.Field(typeof(EFT.Player.FirearmController), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                Helper.IsMagReloading = false;
                player.HandsAnimator.SetAnimationSpeed(1);
            }

        }
    }


    /*    public class SetBoltCatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("SetBoltCatch", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogWarning("SetBoltCatch");
            }
        }
    */

    /*    public class SetFireModePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("SetFireMode", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogInfo("SetFireMode");
                __instance.SetAnimationSpeed(2);
            }
        }*/


    /*    public class SetAnimatorAndProceduralValuesPatch : ModulePatch
{
   protected override MethodBase GetTargetMethod()
   {
       return typeof(Player.FirearmController).GetMethod("SetAnimatorAndProceduralValues", BindingFlags.Instance | BindingFlags.Public);
   }

   [PatchPostfix]
   private static void PatchPostfix(FirearmsAnimator __instance)
   {
       Logger.LogInfo("SetSpeedReload");
   }
}
*/


    /*    public class SetAmmoCompatiblePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("SetAmmoCompatible", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogInfo("SetAmmoCompatible");
            }
        }
    */
    /*    public class SetPatronInWeaponVisiblePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("SetPatronInWeaponVisible", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogInfo("SetPatronInWeaponVisible");
            }
        }

    */

    /*    public class SetAmmoInChamberPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("SetAmmoInChamber", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogInfo("SetAmmoInChamber");
            }
        }
    */
    /*    public class SetChamberIndexWithShellPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("SetChamberIndexWithShell", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogInfo("SetChamberIndexWithShell");
            }
        }
    */
    /*    public class SetShellsInWeapon : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("SetShellsInWeapon", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogInfo("SetShellsInWeapon");
            }
        }*/

    /*    public class ReloadPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("Reload", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogInfo("Reload");

            }
        }

        public class SetAmmoOnMagPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("SetAmmoOnMag", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                if (Helper.IsAttemptingToReloadInternalMag == true && Helper.IsInReloadOpertation)
                {
                   *//* __instance.SetAnimationSpeed(5);*//*
                    Logger.LogInfo("SetAmmoOnMag");
                }
            }
        }*/

    /*    public class ResetReloadPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(FirearmsAnimator).GetMethod("ResetReload", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(FirearmsAnimator __instance)
            {
                Logger.LogInfo("ResetReload");
            }
        }*/

    /*    public class DischargePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("Discharge", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Logger.LogInfo("Discharge");
        }
    }*/

    /*    public class SetSpeedReloadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WeaponAnimationSpeedControllerClass).GetMethod("SetSpeedReload", BindingFlags.Static | BindingFlags.Public);

        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            Logger.LogError("=====================================SetSpeedReload===================================");

        }
    }

    public class PullOutMagInInventoryModePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("PullOutMagInInventoryMode", BindingFlags.Instance | BindingFlags.Public);

        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("=====================================PullOutMagInInventoryMode===================================");

        }
    }

    public class ResetInsertMagInInventoryModePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("ResetInsertMagInInventoryMode", BindingFlags.Instance | BindingFlags.Public);

        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("=====================================ResetInsertMagInInventoryMode===================================");

        }
    }*/

}
