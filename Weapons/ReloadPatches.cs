﻿using Aki.Reflection.Patching;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using static CW2.Animations.PhysicsSimulator.Val;
using static EFT.Player;
using MagReloadClass = EFT.Player.FirearmController.GClass1607;
using RechamberClass = EFT.Player.FirearmController.GClass1619;

namespace RealismMod
{
    public class OnAddAmmoInChamber1623Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1623).GetMethod("OnAddAmmoInChamber", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1623 __instance)
        {
            var firearmController_0 = (FirearmController)AccessTools.Field(typeof(FirearmController.GClass1623), "firearmController_0").GetValue(__instance);
            int chamberAmmoCount = firearmController_0.Weapon.ChamberAmmoCount - (int)Plugin.test1.Value;
            int currentMagazineCount = firearmController_0.Weapon.GetCurrentMagazineCount() - (int)Plugin.test2.Value;

            Logger.LogWarning("==");
            Logger.LogWarning("OnAddAmmoInChamber");
            Logger.LogWarning("chamberAmmoCount " + chamberAmmoCount);
            Logger.LogWarning("currentMagazineCount " + currentMagazineCount);
            Logger.LogWarning("==");
        }
    }

    public class Start1623Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1623).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m =>
            m.GetParameters().Length == 1
            && m.GetParameters()[0].Name == "onWeaponAppear");
        }

        [PatchPrefix]
        private static bool Prefix(FirearmController.GClass1623 __instance, Action onWeaponAppear)
        {
            var firearmController_0 = (FirearmController)AccessTools.Field(typeof(FirearmController.GClass1623), "firearmController_0").GetValue( __instance);
            var _player = (Player)AccessTools.Field(typeof(FirearmController), "_player").GetValue(firearmController_0);
/*            var action_0 = (Action)AccessTools.Field(typeof(FirearmController.GClass1623), "action_0").GetValue(__instance);
*/            var magazine = (MagazineClass)AccessTools.Field(typeof(FirearmController.GClass1623), "gclass2665_0").GetValue(__instance);
            var bool_1 = (bool)AccessTools.Field(typeof(FirearmController.GClass1623), "bool_1").GetValue(__instance);
            var bullet = (BulletClass)AccessTools.Field(typeof(FirearmController.GClass1623), "gclass2732_0").GetValue(__instance);
            var gclass1665_0 = (GClass1665)AccessTools.Field(typeof(FirearmController.GClass1623), "gclass1665_0").GetValue(__instance);


            /*  _player.Logger.LogInfo("SpawnOperation.Start()", Array.Empty<object>());*/
      /*      action_0 = onWeaponAppear;*/

            AccessTools.Field(typeof(FirearmController.GClass1623), "action_0").SetValue(__instance, onWeaponAppear);
            __instance.Start();
            firearmController_0.FirearmsAnimator.SetActiveParam(true, true);
            firearmController_0.FirearmsAnimator.SetLayerWeight(firearmController_0.FirearmsAnimator.LACTIONS_LAYER_INDEX, 0);
            _player.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, (float)firearmController_0.Weapon.CalculateCellSize().X);

            int chamberAmmoCount = firearmController_0.Weapon.ChamberAmmoCount - (int)Plugin.test1.Value;
            int currentMagazineCount = firearmController_0.Weapon.GetCurrentMagazineCount() - (int)Plugin.test2.Value;
            
            magazine = firearmController_0.Weapon.GetCurrentMagazine();

            AccessTools.Field(typeof(FirearmController.GClass1623), "gclass2665_0").SetValue(__instance, magazine);
            firearmController_0.AmmoInChamberOnSpawn = chamberAmmoCount;
     

            Logger.LogWarning("chamberAmmoCount " + chamberAmmoCount);
            Logger.LogWarning("currentMagazineCount " + currentMagazineCount);

            if (chamberAmmoCount == 0) 
            {
                Plugin.canDoCock = false;
            }

            if (firearmController_0.Weapon.HasChambers)
            {
                firearmController_0.FirearmsAnimator.SetAmmoInChamber((float)chamberAmmoCount);
            }

            firearmController_0.FirearmsAnimator.SetAmmoOnMag(currentMagazineCount);

            _player.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
            _player.Skills.OnWeaponDraw(firearmController_0.Weapon);
            bool flag = magazine == null || magazine.IsAmmoCompatible(firearmController_0.Weapon.Chambers);

            bool_1 = flag;
            AccessTools.Field(typeof(FirearmController.GClass1623), "bool_1").SetValue(__instance, flag);

            Logger.LogWarning("ammo is compatible " + flag);

            firearmController_0.FirearmsAnimator.SetAmmoCompatible(flag);

            if (Plugin.canDoCock && magazine != null && chamberAmmoCount == 0 && currentMagazineCount > 0 && flag && firearmController_0.Item.Chambers.Length != 0)
            {
                Weapon.EMalfunctionState state = firearmController_0.Item.MalfState.State;
                GStruct413<GInterface322> gstruct = magazine.Cartridges.PopTo(_player.GClass2757_0, new GClass2763(firearmController_0.Item.Chambers[0]));
                firearmController_0.Item.MalfState.ChangeStateSilent(state);
                if (gstruct.Value == null)
                {
                    Logger.LogWarning("gstruct is null ");
                    return false;
                }
                Logger.LogWarning("remove all shells ");
                gclass1665_0.RemoveAllShells();
                _player.UpdatePhones();
                bullet = (BulletClass)gstruct.Value.ResultItem;
                AccessTools.Field(typeof(FirearmController.GClass1623), "gclass2732_0").SetValue(__instance, bullet);
            }
            return false;

        }
    }


    public class Start1584Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1584).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m =>
            m.GetParameters().Length == 2
            && m.GetParameters()[0].Name == "reloadExternalMagResult"
            && m.GetParameters()[1].Name == "callback");
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1584 __instance, Player.FirearmController.GClass1574 reloadExternalMagResult)
        {
            Logger.LogWarning("StartPatch 1584 Pre");
            AccessTools.Field(typeof(FirearmController.GClass1584), "bool_6").SetValue(Plugin.EnableHybridRecoil.Value, __instance) ;

        }
        [PatchPostfix]
        private static void PostFix(FirearmController.GClass1584 __instance)
        {
            Logger.LogWarning("StartPatch 1584 Post");
            AccessTools.Field(typeof(FirearmController.GClass1584), "bool_6").SetValue(Plugin.EnableHybridRecoil.Value, __instance);
        }
    }


    public class CanStartReload2Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController).GetMethod("CanStartReload", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void Prefix(FirearmController __instance, bool __result)
        {
            Logger.LogWarning("CanStartReload " + __result);
        }
    }


    public class mmethod_11Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("method_11", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("method_11");
        }
    }

    public class method_10Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("method_10", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("method_10");
        }
    }

    public class method_12Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("method_12", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("method_12");
        }
    }

    public class method_13Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("method_13", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("method_13");
        }
    }

    public class OnFireEndEventPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("OnFireEndEvent", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("OnFireEndEvent");
        }
    }

    public class OnAddAmmoInChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("OnAddAmmoInChamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("OnAddAmmoInChamber");
        }
    }

    public class SetTriggerPressedFCPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController).GetMethod("SetTriggerPressed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(FirearmController __instance, bool pressed)
        {
            Logger.LogWarning("FirearmController SetTriggerPressed " + pressed);
            return true;
        }
    }

    public class SetTriggerPressedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("SetTriggerPressed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance, bool pressed)
        {
            Logger.LogWarning("SetTriggerPressed " + pressed);
        }
    }

    public class SetTriggerPressed1599Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1599).GetMethod("SetTriggerPressed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1599 __instance, bool pressed)
        {
            Logger.LogWarning("1599 SetTriggerPressed " + pressed);
        }
    }

    public class SetBoltActionReloadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetBoltActionReload", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, bool boltActionReload)
        {
            Logger.LogWarning("SetBoltActionReload boltActionReload " + boltActionReload);
        }
    }

    public class PrepareShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("PrepareShot", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("PrepareShot");
        }
    }

    public class OnFireEventPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("OnFireEvent", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("OnFireEvent");
        }
    }

    public class OnShellEjectEventPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("OnShellEjectEvent", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("OnShellEjectEvent");
        }
    }

    public class StartFireAnimationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("StartFireAnimation", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("StartFireAnimation");
        }
    }

    public class RemoveAmmoFromChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("RemoveAmmoFromChamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("RemoveAmmoFromChamber");
        }
    }

    public class StartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmController.GClass1598).GetMethod("Start", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmController.GClass1598 __instance)
        {
            Logger.LogWarning("Start");
        }
    }

    public class SetHammerArmedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetHammerArmed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("SetHammerArmed");
        }
    }

    public class SetAmmoInChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetAmmoInChamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, ref float count)
        {
            Logger.LogWarning("SetAmmoInChamber");
/*            if (!Plugin.canDoCock)
            {
                count = 0;
            }*/
        }
    }

    public class Rechamber2Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("Rechamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("Rechamber");
        }
    }

    public class SetShellsInWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetShellsInWeapon", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("SetShellsInWeapon");
        }
    }


    public class SetAmmoCompatiblePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetAmmoCompatible", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, ref bool compatible)
        {
            Logger.LogWarning("SetAmmoCompatible " + compatible);
            if (!Plugin.canDoCock)
            {
                Logger.LogWarning("SetAmmoCompatible canDoCock");

                compatible = false;
            }
        }
    }

    public class SetCanReloadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetCanReload", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("SetCanReload");
        }
    }

    public class SetBoltCatchPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetBoltCatch", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("SetBoltCatch");
        }
    }

    public class ReloadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("Reload", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance)
        {
            Logger.LogWarning("Reload");
        }
    }

    public class SetPatronInWeaponVisiblePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetPatronInWeaponVisible", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, bool visible)
        {
            Logger.LogWarning("SetPatronInWeaponVisible " + visible);
        }
    }

    public static class ReloadController
    {
        public static void ReloadStateCheck(Player player, EFT.Player.FirearmController fc, ManualLogSource logger)
        {
            PlayerStats.IsInReloadOpertation = fc.IsInReloadOperation();

            if (PlayerStats.IsInReloadOpertation)
            {
                StanceController.IsPatrolStance = false;
                StanceController.CancelShortStock = true;
                // StanceController.CancelPistolStance = true;
                StanceController.CancelActiveAim = true;

                if (PlayerStats.IsAttemptingToReloadInternalMag == true)
                {
                    StanceController.CancelHighReady = fc.Item.WeapClass != "shotgun" ? true : false;
                    StanceController.CancelLowReady = fc.Item.WeapClass == "shotgun" || fc.Item.WeapClass == "pistol" ? true : false;

                    float highReadyBonus = fc.Item.WeapClass == "shotgun" && StanceController.IsHighReady == true ? StanceController.HighReadyManipBuff : 1f;
                    float lowReadyBonus = fc.Item.WeapClass != "shotgun" && StanceController.IsLowReady == true ? StanceController.LowReadyManipBuff : 1f;

                    float IntenralMagReloadSpeed = Mathf.Clamp(WeaponStats.CurrentMagReloadSpeed * Plugin.InternalMagReloadMulti.Value * PlayerStats.ReloadSkillMulti * PlayerStats.ReloadInjuryMulti * highReadyBonus * lowReadyBonus * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)), 0.55f, 1.4f);
                    player.HandsAnimator.SetAnimationSpeed(IntenralMagReloadSpeed);

                    if (Plugin.EnableLogging.Value == true)
                    {
                        logger.LogWarning("IsAttemptingToReloadInternalMag = " + IntenralMagReloadSpeed);
                    }
                }
            }
            else
            {
                PlayerStats.IsAttemptingToReloadInternalMag = false;
                PlayerStats.IsAttemptingRevolverReload = false;
            }
        }
    }


    public class ChamberCheckUIPatch : ModulePatch
    {
        private static FieldInfo ammoCountPanelField;
        protected override MethodBase GetTargetMethod()
        {
            ammoCountPanelField = AccessTools.Field(typeof(BattleUIScreen), "_ammoCountPanel");
            return typeof(Player.FirearmController).GetMethod("CheckChamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            AmmoCountPanel panelUI = (AmmoCountPanel)ammoCountPanelField.GetValue(Singleton<GameUI>.Instance.BattleUiScreen);
            Slot slot = __instance.Weapon.Chambers.FirstOrDefault<Slot>();
            BulletClass bulletClass = (slot == null) ? null : (slot.ContainedItem as BulletClass);
            if (bulletClass != null)
            {
                string name = bulletClass.LocalizedName();
                panelUI.Show("", name);
            }
            else 
            {
                if (__instance.Weapon.Chambers.Length == 1) 
                {
                    panelUI.Show("Empty");
                }
            }
        }
    }


    public class SetSpeedParametersPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetSpeedParameters", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsAnimator __instance)
        {
            __instance.SetAnimationSpeed(1);

            if (Plugin.EnableLogging.Value == true)
            {
                Logger.LogWarning("===SetSpeedParameters===");
                Logger.LogWarning("=============");
            }
        }
    }

    public class SetAnimatorAndProceduralValuesPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(EFT.Player.FirearmController).GetMethod("SetAnimatorAndProceduralValues", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                StanceController.DoResetStances = true;

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("SetAnimatorAndProceduralValues");
                }
            }
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
            if (WeaponStats._WeapClass == "shotgun")
            {
                if (weaponLevel < 3)
                {
                    weaponLevel += 1;
                }
                WeaponAnimationSpeedControllerClass.SetWeaponLevel(__instance.Animator, weaponLevel);
            }

        }
    }

    public class CheckAmmoPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(EFT.Player.FirearmController).GetMethod("CheckAmmo", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                if (Plugin.EnableReloadPatches.Value)
                {
                    float bonus = Plugin.GlobalCheckAmmoMulti.Value;
                    if (WeaponStats._WeapClass == "pistol")
                    {
                        bonus = Plugin.GlobalCheckAmmoPistolSpeedMulti.Value;
                    }

                    float totalCheckAmmoPatch = Mathf.Clamp(WeaponStats.CurrentMagReloadSpeed * PlayerStats.ReloadSkillMulti * PlayerStats.ReloadInjuryMulti * StanceController.HighReadyManipBuff * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)) * bonus, 0.6f, 1.3f);
                    __instance.FirearmsAnimator.SetAnimationSpeed(totalCheckAmmoPatch);

                    if (Plugin.EnableLogging.Value == true)
                    {
                        Logger.LogWarning("===CheckAmmo===");
                        Logger.LogWarning("Check Ammo =" + totalCheckAmmoPatch);
                        Logger.LogWarning("=============");
                    }
                }

                StanceController.CancelLowReady = true;
                StanceController.CancelShortStock = true;
                StanceController.CancelActiveAim = true;
                // StanceController.CancelPistolStance = true;
            }
        }
    }

    public class CheckChamberPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(EFT.Player.FirearmController).GetMethod("CheckChamber", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPostfix(EFT.Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                if (Plugin.EnableReloadPatches.Value)
                {
                    float chamberSpeed = WeaponStats.TotalChamberCheckSpeed;
                    if (WeaponStats._WeapClass == "pistol")
                    {
                        chamberSpeed *= Plugin.GlobalCheckChamberPistolSpeedMulti.Value;
                    }
                    else if (WeaponStats._WeapClass == "shotgun")
                    {
                        chamberSpeed *= Plugin.GlobalCheckChamberShotgunSpeedMulti.Value;
                    }
                    else
                    {
                        chamberSpeed *= Plugin.GlobalCheckChamberSpeedMulti.Value;
                    }

                    float totalCheckChamberSpeed = Mathf.Clamp(chamberSpeed * PlayerStats.FixSkillMulti * PlayerStats.ReloadInjuryMulti * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)), 0.55f, 1.8f);
                    __instance.FirearmsAnimator.SetAnimationSpeed(totalCheckChamberSpeed);


                    if (Plugin.EnableLogging.Value == true)
                    {
                        Logger.LogWarning("===CheckChamber===");
                        Logger.LogWarning("Check Chamber = " + totalCheckChamberSpeed);
                        Logger.LogWarning("=============");
                    }
                }

                StanceController.CancelLowReady = true;
                StanceController.CancelHighReady = true;
                StanceController.CancelShortStock = true;
                StanceController.CancelActiveAim = true;
            }
        }
    }


    public class BoltActionReloadPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("InitiateShot", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPrefix(Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer && ((WeaponStats._IsManuallyOperated || __instance.Item.IsGrenadeLauncher || __instance.Item.IsUnderBarrelDeviceActive)))
            {
                float chamberSpeed = WeaponStats.TotalFiringChamberSpeed;
                float ammoRec = __instance.Item.CurrentAmmoTemplate.ammoRec;
                float ammoFactor = ammoRec < 0 ? 1f + (ammoRec / 100f) : 1f + (ammoRec / 150f);
                ammoFactor = 2f - ammoFactor;
                float stanceModifier = 1f;

                if (WeaponStats._WeapClass == "shotgun")
                {
                    chamberSpeed *= Plugin.GlobalShotgunRackSpeedFactor.Value;
                    stanceModifier = StanceController.IsBracing ? 1.1f : StanceController.IsMounting ? 1.2f : StanceController.IsActiveAiming ? 1.35f : 1f;
                }
                if (__instance.Item.IsGrenadeLauncher || __instance.Item.IsUnderBarrelDeviceActive)
                {
                    chamberSpeed *= Plugin.GlobalUBGLReloadMulti.Value;
                }
                if (WeaponStats._WeapClass == "sniperRifle")
                {
                    chamberSpeed *= Plugin.GlobalBoltSpeedMulti.Value;
                    stanceModifier = StanceController.IsBracing ? 1.2f : StanceController.IsMounting ? 1.4f : StanceController.IsActiveAiming ? 1.15f : 1f;
                }
                float totalChamberSpeed = Mathf.Clamp(chamberSpeed * PlayerStats.ReloadSkillMulti * PlayerStats.ReloadInjuryMulti * stanceModifier * ammoFactor * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)), 0.55f, 1.3f);
                __instance.FirearmsAnimator.SetAnimationSpeed(totalChamberSpeed);

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("===SetBoltActionReload===");
                    Logger.LogWarning("Set Bolt Action Reload = " + totalChamberSpeed);
                    Logger.LogWarning("=============");
                }
            }
        }
    }

    public class SetMalfRepairSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsAnimator).GetMethod("SetMalfRepairSpeed", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, float fix)
        {
            float totalFixSpeed = Mathf.Clamp(fix * WeaponStats.TotalFixSpeed * PlayerStats.ReloadInjuryMulti * Plugin.GlobalFixSpeedMulti.Value * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)), 0.55f, 1.15f);
            WeaponAnimationSpeedControllerClass.SetSpeedFix(__instance.Animator, totalFixSpeed);
            __instance.SetAnimationSpeed(totalFixSpeed);

            if (Plugin.EnableLogging.Value == true)
            {
                Logger.LogWarning("===SetMalfRepairSpeed===");
                Logger.LogWarning("SetMalfRepairSpeed = " + totalFixSpeed);
                Logger.LogWarning("=============");
            }
        }
    }

    public class RechamberPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(RechamberClass).GetMethod("Start", new Type[] { typeof(BulletClass), typeof(Callback) });
        }

        [PatchPostfix]
        private static void PatchPostfix(RechamberClass __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                if (Plugin.EnableReloadPatches.Value)
                {

                    float chamberSpeed = WeaponStats.TotalFixSpeed;
                    if (WeaponStats._WeapClass == "pistol")
                    {
                        chamberSpeed *= Plugin.RechamberPistolSpeedMulti.Value;
                    }
                    else
                    {
                        chamberSpeed *= Plugin.GlobalRechamberSpeedMulti.Value;
                    }

                    float totalRechamberSpeed = Mathf.Clamp(chamberSpeed * PlayerStats.FixSkillMulti * PlayerStats.ReloadInjuryMulti * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)), 0.5f, 1.5f);

                    FirearmsAnimator fa = (FirearmsAnimator)AccessTools.Field(typeof(Player.FirearmController.GClass1581), "firearmsAnimator_0").GetValue(__instance);
                    fa.SetAnimationSpeed(totalRechamberSpeed);

                    if (Plugin.EnableLogging.Value == true)
                    {
                        Logger.LogWarning("===Rechamber===");
                        Logger.LogWarning("Rechamber = " + totalRechamberSpeed);
                        Logger.LogWarning("=============");
                    }
                }

                StanceController.CancelShortStock = true;
                // StanceController.CancelPistolStance = true;
            }
        }
    }

    public class CanStartReloadPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("CanStartReload", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, bool __result)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                if (__result == true)
                {
                    if (__instance.Item.GetCurrentMagazine() == null)
                    {
                        PlayerStats.NoCurrentMagazineReload = true;
                    }
                    else
                    {
                        PlayerStats.NoCurrentMagazineReload = false;
                    }
                }
            }
        }
    }

    public class ReloadMagPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadMag", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, MagazineClass magazine)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                StatCalc.SetMagReloadSpeeds(__instance, magazine);

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("ReloadMag Patch");
                    Logger.LogWarning("magazine = " + magazine.LocalizedName());
                    Logger.LogWarning("magazine weight = " + magazine.GetSingleItemTotalWeight());
                }
            }
        }
    }


    public class QuickReloadMagPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("QuickReloadMag", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, MagazineClass magazine)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                StatCalc.SetMagReloadSpeeds(__instance, magazine, true);
                PlayerStats.IsQuickReloading = true;

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("===QuickReloadMag===");
                    Logger.LogWarning("=============");
                }
            }
        }
    }

    public class ReloadCylinderMagazinePatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadCylinderMagazine", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                PlayerStats.IsAttemptingToReloadInternalMag = true;
                PlayerStats.IsAttemptingRevolverReload = true;

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("===ReloadCylinderMagazine===");
                    Logger.LogWarning("=============");
                }
            }
        }
    }

    public class ReloadWithAmmoPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadWithAmmo", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                PlayerStats.IsAttemptingToReloadInternalMag = true;

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("===ReloadWithAmmo===");
                    Logger.LogWarning("=============");
                }
            }
        }
    }

    public class ReloadBarrelsPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("ReloadBarrels", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                PlayerStats.IsAttemptingToReloadInternalMag = true;

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("===ReloadBarrels===");
                    Logger.LogWarning("=============");
                }
            }
        }
    }


    public class SetMagTypeNewPatch : ModulePatch
    {
        private static FieldInfo playerField;
        private static FieldInfo faField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(Player.FirearmController.GClass1581), "player_0");
            faField = AccessTools.Field(typeof(Player.FirearmController.GClass1581), "firearmsAnimator_0");
            return typeof(MagReloadClass).GetMethod("Start", new Type[] { typeof(Player.FirearmController.GClass1573), typeof(Callback) });
        }

        [PatchPostfix]
        private static void PatchPostfix(MagReloadClass __instance)
        {
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer)
            {
                FirearmsAnimator fa = (FirearmsAnimator)faField.GetValue(__instance);

                float totalReloadSpeed = Mathf.Clamp(WeaponStats.CurrentMagReloadSpeed * PlayerStats.ReloadSkillMulti * PlayerStats.ReloadInjuryMulti * StanceController.HighReadyManipBuff * StanceController.ActiveAimManipBuff * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)), 0.5f, 1.3f);
                fa.SetAnimationSpeed(totalReloadSpeed);

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("===SetMagTypeNew===");
                    Logger.LogWarning("SetMagTypeNew = " + totalReloadSpeed);
                    Logger.LogWarning("=============");
                }
            }

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
            float totalReloadSpeed = Mathf.Clamp(WeaponStats.CurrentMagReloadSpeed * PlayerStats.ReloadSkillMulti * PlayerStats.ReloadInjuryMulti * StanceController.HighReadyManipBuff * StanceController.ActiveAimManipBuff * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)), 0.5f, 1.3f);
            __instance.SetAnimationSpeed(totalReloadSpeed);

            if (Plugin.EnableLogging.Value == true)
            {
                Logger.LogWarning("===SetMagTypeCurrent===");
                Logger.LogWarning("SetMagTypeCurrent = " + totalReloadSpeed);
                Logger.LogWarning("=============");
            }

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
            if (PlayerStats.IsMagReloading == true)
            {
                float totalReloadSpeed = Mathf.Clamp(WeaponStats.NewMagReloadSpeed * PlayerStats.ReloadSkillMulti * PlayerStats.ReloadInjuryMulti * PlayerStats.GearReloadMulti * StanceController.HighReadyManipBuff * StanceController.ActiveAimManipBuff * (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)), 0.5f, 1.3f);
                __instance.SetAnimationSpeed(totalReloadSpeed);

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("===SetMagInWeapon===");
                    Logger.LogWarning("SetMagInWeapon = " + totalReloadSpeed);
                    Logger.LogWarning("ReloadSkillMulti = " + PlayerStats.ReloadSkillMulti);
                    Logger.LogWarning("ReloadInjuryMulti = " + PlayerStats.ReloadInjuryMulti);
                    Logger.LogWarning("GearReloadMulti = " + PlayerStats.GearReloadMulti);
                    Logger.LogWarning("HighReadyManipBuff = " + StanceController.HighReadyManipBuff);
                    Logger.LogWarning("RemainingArmStamPercReload = " + (Mathf.Max(PlayerStats.RemainingArmStamPercReload, 0.7f)));
                    Logger.LogWarning("NewMagReloadSpeed = " + WeaponStats.NewMagReloadSpeed);
                    Logger.LogWarning("=============");
                }

            }
        }
    }


    public class OnMagInsertedPatch : ModulePatch
    {
        private static FieldInfo playerField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            return typeof(Player.FirearmController).GetMethod("method_47", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player.FirearmController __instance)
        {
            //to find this again, look for private void method_47(){ this.CurrentOperation.OnMagInsertedToWeapon(); }
            Player player = (Player)playerField.GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                PlayerStats.IsMagReloading = false;
                PlayerStats.IsQuickReloading = false;
                player.HandsAnimator.SetAnimationSpeed(1);

                if (Plugin.EnableLogging.Value == true)
                {
                    Logger.LogWarning("===OnMagInsertedPatch/method_47===");
                    Logger.LogWarning("=============");
                }
            }
        }
    }
}
