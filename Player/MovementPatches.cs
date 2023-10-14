﻿using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static BaseBallistic;
using static EFT.Player;
using ValueHandler = GClass654;
using SkillMovementStruct = EFT.SkillManager.GStruct213;

namespace RealismMod
{
    public static class MovementSpeedController
    {
        private static Dictionary<BaseBallistic.ESurfaceSound, float> SurfaceSpeedModifiers = new Dictionary<BaseBallistic.ESurfaceSound, float>()
        {
            {BaseBallistic.ESurfaceSound.Metal, 0.95f },
            {BaseBallistic.ESurfaceSound.MetalThin, 0.95f },
            {BaseBallistic.ESurfaceSound.GarbageMetal, 0.75f },
            {BaseBallistic.ESurfaceSound.Garbage, 0.75f },
            {BaseBallistic.ESurfaceSound.Concrete, 1.05f },
            {BaseBallistic.ESurfaceSound.Asphalt, 1.05f },
            {BaseBallistic.ESurfaceSound.Gravel, 0.85f },
            {BaseBallistic.ESurfaceSound.Slate, 0.85f },
            {BaseBallistic.ESurfaceSound.Tile, 0.8f },
            {BaseBallistic.ESurfaceSound.Plastic, 0.95f },
            {BaseBallistic.ESurfaceSound.Glass, 0.9f },
            {BaseBallistic.ESurfaceSound.WholeGlass, 0.85f },
            {BaseBallistic.ESurfaceSound.Wood, 0.95f},
            {BaseBallistic.ESurfaceSound.WoodThick, 0.95f },
            {BaseBallistic.ESurfaceSound.WoodThin, 0.95f },
            {BaseBallistic.ESurfaceSound.Soil, 1.0f},
            {BaseBallistic.ESurfaceSound.Grass, 0.95f },
            {BaseBallistic.ESurfaceSound.Swamp, 1.0f },
            {BaseBallistic.ESurfaceSound.Puddle, 0.8f }
        };

        public static BaseBallistic.ESurfaceSound CurrentSurface;

        private static float currentModifier = 1f;
        private static float targetModifier = 1f;
        private static float smoothness = 0.5f;

        public static float GetSurfaceSpeed() 
        {
            targetModifier = SurfaceSpeedModifiers.TryGetValue(CurrentSurface, out float value) ? value : 1f;
            currentModifier = Mathf.Lerp(currentModifier, targetModifier, smoothness);
            return (float)Math.Round(currentModifier, 3);
        }

        private static float maxSlopeAngle = 5f;
        private static float maxSlowdownFactor = 0.48f;

        public static float GetSlope(Player player) 
        {
            Vector3 movementDirecion = player.MovementContext.MovementDirection.normalized;
            Vector3 position = player.Transform.position;
            RaycastHit hit;
            float slowdownFactor = 1f;

            if (Physics.Raycast(position, -Vector3.up, out hit))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle > maxSlopeAngle)
                {
                    slowdownFactor = Mathf.Lerp(1f, maxSlowdownFactor, (slopeAngle - maxSlopeAngle) / (90f - maxSlopeAngle));
                }
            }
            return slowdownFactor;

        }

        public static float GetFiringMovementSpeedFactor(Player player, ManualLogSource logger) 
        {
            if (!RecoilController.IsFiringMovement) 
            {
                return 1f;
            }

            Player.FirearmController fc = player.HandsController as Player.FirearmController;
            if (fc == null) 
            {
                return 1f;
            }

            float convergenceFactor = 1f - (RecoilController.BaseTotalConvergence / 100f);
            float dampingFactor = RecoilController.BaseTotalHandDamping + RecoilController.BaseTotalRecoilDamping;
            float dispersionFactor = 1f + (RecoilController.BaseTotalDispersion / 100f);
            float recoilFactor = RecoilController.FactoredTotalVRecoil + RecoilController.FactoredTotalHRecoil;
            recoilFactor = recoilFactor * dispersionFactor * dampingFactor * convergenceFactor;
            recoilFactor = fc.Item.WeapClass == "pistol" ? recoilFactor * 0.1f : recoilFactor;
            float totalRecoilFactor = 1f - ((recoilFactor / 1000f) * RecoilController.ShotCount);
            totalRecoilFactor = Mathf.Clamp(totalRecoilFactor, 0.25f, 1f);
            return totalRecoilFactor;
        }

    }

    public class ClampSpeedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MovementContext).GetMethod("ClampSpeed", BindingFlags.Instance | BindingFlags.Public);
        }


        [PatchPrefix]
        private static bool Prefix(MovementContext __instance, float speed, ref float __result)
        {

            Player player = (Player)AccessTools.Field(typeof(MovementContext), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                float slopeFactor = 1f;

                if (Utils.IsReady && Plugin.EnableSlopeSpeed.Value)
                {
                    slopeFactor = MovementSpeedController.GetSlope(player);
                }

                float surfaceMulti = Plugin.EnableMaterialSpeed.Value ? MovementSpeedController.GetSurfaceSpeed() : 1f;
                float firingMulti = MovementSpeedController.GetFiringMovementSpeedFactor(player, Logger);
                float stanceFactor = StanceController.IsPatrolStance ? 1.25f : StanceController.IsHighReady || StanceController.IsShortStock ? 0.95f : 1f;

                __result = Mathf.Clamp(speed, 0f, __instance.StateSpeedLimit * PlayerProperties.HealthWalkSpeedFactor * surfaceMulti * slopeFactor * firingMulti * stanceFactor);
                return false;
            }
            return true;

        }
    }

    public class CalculateSurfacePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("method_48", BindingFlags.Instance | BindingFlags.NonPublic);
        }


        [PatchPostfix]
        private static void PatchPostfix(Player __instance, ref ValueTuple<bool, BaseBallistic.ESurfaceSound> __result)
        {

            if (__instance.IsYourPlayer == true)
            {
                MovementSpeedController.CurrentSurface = __result.Item2;
            }
        }
    }

    public class SetAimingSlowdownPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MovementContext).GetMethod("SetAimingSlowdown", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(MovementContext __instance, bool isAiming)
        {
            
            Player player = (Player)AccessTools.Field(typeof(MovementContext), "_player").GetValue(__instance);
            if (player.IsYourPlayer == true)
            {
                if (isAiming)
                {
                    //slow is hard set to 0.33 when called, 0.4-0.43 feels best.
                    float baseSpeed = PlayerProperties.AimMoveSpeedBase * WeaponProperties.AimMoveSpeedWeapModifier * PlayerProperties.AimMoveSpeedInjuryMulti;
                    float totalSpeed = StanceController.IsActiveAiming ? baseSpeed * 1.55f : baseSpeed;
                    totalSpeed = WeaponProperties._WeapClass == "pistol" ? totalSpeed + 0.15f : totalSpeed;
                    __instance.AddStateSpeedLimit(Mathf.Clamp(totalSpeed, 0.3f, 0.9f), Player.ESpeedLimit.Aiming);

                    return false;
                }
                __instance.RemoveStateSpeedLimit(Player.ESpeedLimit.Aiming);
                return false;
            }
            return true;
        }
    }

    public class SprintAccelerationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MovementContext).GetMethod("SprintAcceleration", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static bool Prefix(MovementContext __instance, float deltaTime)
        {
            Player player = (Player)AccessTools.Field(typeof(MovementContext), "_player").GetValue(__instance);

            if (player.IsYourPlayer == true)
            {
                ValueHandler rotationFrameSpan = (ValueHandler)AccessTools.Field(typeof(MovementContext), "_averageRotationX").GetValue(__instance);

                float slopeFactor = Plugin.EnableSlopeSpeed.Value ? MovementSpeedController.GetSlope(player) : 1f;
                float surfaceMulti = Plugin.EnableMaterialSpeed.Value ? MovementSpeedController.GetSurfaceSpeed() : 1f;
                float stanceSpeedBonus = StanceController.IsHighReady && Plugin.EnableTacSprint.Value ? 1.15f : 1f;
                float stanceAccelBonus = StanceController.IsPatrolStance ? 1.5f : StanceController.IsShortStock ? 0.9f : StanceController.IsLowReady ? 1.3f : StanceController.IsHighReady && Plugin.EnableTacSprint.Value ? 1.7f : StanceController.IsHighReady ? 1.3f : 1f;

                if (surfaceMulti < 1.0f) 
                {
                    surfaceMulti = Mathf.Max(surfaceMulti * 0.85f, 0.2f);
                }
                if (slopeFactor < 1.0f)
                {
                    surfaceMulti = Mathf.Max(surfaceMulti * 0.85f, 0.2f);
                }

                float sprintAccel = player.Physical.SprintAcceleration * stanceAccelBonus * PlayerProperties.HealthSprintAccelFactor * surfaceMulti * slopeFactor * deltaTime;
                float speed = (player.Physical.SprintSpeed * __instance.SprintingSpeed + 1f) * __instance.StateSprintSpeedLimit * stanceSpeedBonus * PlayerProperties.HealthSprintSpeedFactor * surfaceMulti * slopeFactor;
                float sprintInertia = Mathf.Max(EFTHardSettings.Instance.sprintSpeedInertiaCurve.Evaluate(Mathf.Abs((float)rotationFrameSpan.Average)), EFTHardSettings.Instance.sprintSpeedInertiaCurve.Evaluate(2.1474836E+09f) * (2f - player.Physical.Inertia));
                speed = Mathf.Clamp(speed * sprintInertia, 0.1f, speed);
                __instance.SprintSpeed = Mathf.Clamp(__instance.SprintSpeed + sprintAccel * Mathf.Sign(speed - __instance.SprintSpeed), 0.01f, speed);
                return false;
            }
            return true;
        }
    }

    public class EnduranceSprintActionPatch : ModulePatch
    {

        private static Type _targetType;
        private static MethodInfo _method_0;

        public EnduranceSprintActionPatch()
        {
            _targetType = PatchConstants.EftTypes.Single(PlayerHelper.IsEnduraStrngthType);
            _method_0 = _targetType.GetMethod("method_0", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected override MethodBase GetTargetMethod()
        {
            return _method_0;
        }

        [PatchPrefix]
        private static bool Prefix(ref float __result, SkillMovementStruct movement, SkillManager __instance)
        {
            float xp = __instance.Settings.Endurance.SprintAction * (1f + __instance.Settings.Endurance.GainPerFatigueStack * movement.Fatigue);
            if (movement.Overweight <= 0f)
            {
                __result = xp;
            }
            else
            {
                __result = xp * 0.5f;
            }

            return false;
        }
    }

    public class EnduranceMovementActionPatch : ModulePatch
    {

        private static Type _targetType;
        private static MethodInfo _method_1;

        public EnduranceMovementActionPatch()
        {
            _targetType = PatchConstants.EftTypes.Single(PlayerHelper.IsEnduraStrngthType);
            _method_1 = _targetType.GetMethod("method_1", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
        }


        protected override MethodBase GetTargetMethod()
        {
            return _method_1;
        }

        [PatchPrefix]
        private static bool Prefix(ref float __result, SkillMovementStruct movement, SkillManager __instance)
        {
            float xp = __instance.Settings.Endurance.MovementAction * (1f + __instance.Settings.Endurance.GainPerFatigueStack * movement.Fatigue);
            if (movement.Overweight <= 0f)
            {
                __result = xp;
            }
            else
            {
                __result = xp * 0.5f;
            }

            return false;
        }
    }

    public static class PlayerHelper
    {
        public static bool IsEnduraStrngthType(Type type)
        {
            return type.GetField("skillsRelatedToHealth") != null && type.GetField("skillManager_0") != null;
        }
    }
}
