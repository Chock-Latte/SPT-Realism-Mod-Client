﻿using EFT;
using System.Linq;
using UnityEngine;
using EffectClass = EFT.HealthSystem.ActiveHealthController.GClass2429;
using ExistanceClass = GClass2470;
using InterfaceOne = GInterface252;
using InterfaceTwo = GInterface267;

namespace RealismMod
{
    public enum EHealthEffectType
    {
        Surgery,
        Tourniquet,
        HealthRegen,
        HealthDrain,
        Adrenaline,
        ResourceRate,
        PainKiller,
        Stim,
        FoodPoisoning,
        Toxicity,
        Detoxification,
        Radiation,
        RadiationTreatment
    }

    public interface ICustomHealthEffect
    {
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; }
        public int TimeExisted { get; set; }
        public void Tick();
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        public RealismHealthController RealHealthController { get; set; }
    }

    public class TourniquetEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; }
        public int TimeExisted { get; set; }
        public float HpPerTick { get; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        private bool haveNotified = false;
        private bool haveRemovedSurgery = false;

        public TourniquetEffect(float hpTick, int? dur, EBodyPart part, Player player, int delay, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            HpPerTick = -hpTick;
            Duration = dur;
            BodyPart = part;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.Tourniquet;
            RealHealthController = realHealthController;
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                TimeExisted++;
       
                if (!haveNotified)
                {
                    if (PluginConfig.EnableMedNotes.Value) NotificationManagerClass.DisplayWarningNotification("Tourniquet Applied On " + BodyPart + ", You Are Losing Health On This Limb. Use A Surgery Kit To Remove It", EFT.Communications.ENotificationDurationType.Long);
                    haveNotified = true;
                }

                if (!haveRemovedSurgery)
                {
                    haveRemovedSurgery = true;
                    RealHealthController.RemoveCustomEffectOfType(typeof(SurgeryEffect), BodyPart);
                }

                float currentPartHP = _Player.ActiveHealthController.GetBodyPartHealth(BodyPart).Current;

                if (TimeExisted > 10 && TimeExisted % 10 == 0)
                {
                    RealHealthController.RemoveBaseEFTEffect(_Player, BodyPart, "HeavyBleeding");
                    RealHealthController.RemoveBaseEFTEffect(_Player, BodyPart, "LightBleeding");
                }

                if (currentPartHP > 25f && TimeExisted % 3 == 0) 
                {
                    _Player.ActiveHealthController.AddEffect<HealthChange>(BodyPart, 0f, 3f, 1f, HpPerTick, null);
                }
            }
        }
    }

    public class SurgeryEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public float HpPerTick { get; }
        public Player _Player { get; }
        public float HpRegened { get; set; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        public float HpRegenLimit { get; }
        private bool hasRemovedTrnqt = false;
        private bool haveNotified = false;

        public SurgeryEffect(float hpTick, int? dur, EBodyPart part, Player player, int delay, float limit, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            HpRegened = 0;
            HpPerTick = hpTick;
            Duration = dur;
            BodyPart = part;
            _Player = player;
            Delay = delay;
            HpRegenLimit = limit;
            EffectType = EHealthEffectType.Surgery;
            RealHealthController = realHealthController;
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                TimeExisted++;

                if (!haveNotified)
                {
                    if (PluginConfig.EnableMedNotes.Value) NotificationManagerClass.DisplayMessageNotification("Surgery Kit Applied On " + BodyPart + ", Restoring HP.", EFT.Communications.ENotificationDurationType.Long);
                    haveNotified = true;
                }

                if (!hasRemovedTrnqt)
                {
                    hasRemovedTrnqt = true;
                    RealHealthController.RemoveCustomEffectOfType(typeof(TourniquetEffect), BodyPart);
                    if (PluginConfig.EnableMedNotes.Value) NotificationManagerClass.DisplayMessageNotification("Surgical Kit Used, Removing Tourniquet Effect Present On Limb If Present: " + BodyPart, EFT.Communications.ENotificationDurationType.Long);
                }

                float currentHp = _Player.ActiveHealthController.GetBodyPartHealth(BodyPart).Current;
                float maxHp = _Player.ActiveHealthController.GetBodyPartHealth(BodyPart).Maximum;
                float maxHpRegen = maxHp * HpRegenLimit;

                if (HpRegened < maxHpRegen && TimeExisted % 3 == 0)
                {
                    _Player.ActiveHealthController.AddEffect<HealthChange>(BodyPart, 0f, 3f, 1f, HpPerTick, null);
                    HpRegened += HpPerTick;
                }

                if (HpRegened >= maxHpRegen || currentHp == maxHp)
                {
                    if (PluginConfig.EnableMedNotes.Value) NotificationManagerClass.DisplayMessageNotification("Surgical Kit Health Regeneration On " + BodyPart + " Has Expired", EFT.Communications.ENotificationDurationType.Long);
                    Duration = 0;
                    return;
                }
            }
        }
    }

    public class HealthDrainEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public float HpPerTick { get; }
        public Player _Player { get; }
        public float HpDrained { get; set; }
        public float HpDrainLimit { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }

        public HealthDrainEffect(float hpTick, Player player, int delay, float limit, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            HpDrained = 0;
            HpDrainLimit = limit;
            HpPerTick = hpTick;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.HealthDrain;
            BodyPart = EBodyPart.Common;
            RealHealthController = realHealthController;
        }

        public void Tick()
        {
            TimeExisted++;
            if (HpDrained < HpDrainLimit)
            {
                if (Delay <= 0 && TimeExisted % 3 == 0)
                {
                    _Player.ActiveHealthController.AddEffect<HealthDrain>(0f, 3f, 1f, HpPerTick, null);
                    HpDrained += HpPerTick;
                }
            }

            if (HpDrained >= HpDrainLimit)
            {
                Duration = 0;
            }
        }
    }

    public class HealthRegenEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public float HpPerTick { get; }
        public Player _Player { get; }
        public float HpRegened { get; set; }
        public float HpRegenLimit { get; }
        public int Delay { get; set; }
        public EDamageType DamageType { get; }
        public EHealthEffectType EffectType { get; }
        private bool deductedRecordedDamage = false;

        public HealthRegenEffect(float hpTick, int? dur, EBodyPart part, Player player, int delay, float limit, EDamageType damageType, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            HpRegened = 0;
            HpRegenLimit = limit;
            HpPerTick = hpTick;
            Duration = dur;
            BodyPart = part;
            _Player = player;
            Delay = delay;
            DamageType = damageType;
            EffectType = EHealthEffectType.HealthRegen;
            RealHealthController = realHealthController;
        }

        public void Tick()
        {
            TimeExisted++;
            float currentHp = _Player.ActiveHealthController.GetBodyPartHealth(BodyPart).Current;
            float maxHp = _Player.ActiveHealthController.GetBodyPartHealth(BodyPart).Maximum;

            if (!deductedRecordedDamage)
            {
                if (DamageType == EDamageType.HeavyBleeding)
                {
                    RealHealthController.DmgeTracker.TotalHeavyBleedDamage = Mathf.Max(RealHealthController.DmgeTracker.TotalHeavyBleedDamage - HpRegenLimit, 0f);
                }
                if (DamageType == EDamageType.LightBleeding)
                {
                    RealHealthController.DmgeTracker.TotalLightBleedDamage = Mathf.Max(RealHealthController.DmgeTracker.TotalLightBleedDamage - HpRegenLimit, 0f);

                }
                deductedRecordedDamage = true;
            }

            if (HpRegened < HpRegenLimit)
            {
                if (Delay <= 0 && TimeExisted % 3 == 0)
                {
                    _Player.ActiveHealthController.AddEffect<HealthChange>(BodyPart, 0f, 3f, 1f, HpPerTick, null);
                    HpRegened += HpPerTick;
                }
            }

            if (HpRegened >= HpRegenLimit || (currentHp >= maxHp) || currentHp <= 0f)
            {
                Duration = 0;
            }
        }
    }

    public class ResourceRateEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        private bool addedEffect = false;

        public ResourceRateEffect(int? dur, Player player, int delay, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            Duration = dur;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.ResourceRate;
            BodyPart = EBodyPart.Chest;
            RealHealthController = realHealthController;
        }

        public void Tick()
        {
            if (!addedEffect) 
            {
                _Player.ActiveHealthController.AddEffect<ResourceRateDrain>(BodyPart, 0f, null, 0f, 0f, null);
                addedEffect = true;
            }
        }
    }

    public class PainKillerEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        public float TunnelVisionStrength { get; }
        public float PainKillerStrength { get; set; }
        private bool addedEffect = false;

        public PainKillerEffect(int? dur, Player player, int delay, float tunnelStrength, float painKillerStrength, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            Duration = dur;
            BodyPart = EBodyPart.Head;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.PainKiller;
            TunnelVisionStrength = tunnelStrength;
            PainKillerStrength = painKillerStrength;
            RealHealthController = realHealthController;
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                Duration--;
                if (Duration <= 0)
                {
                    Plugin.RealHealthController.PainReliefStrength -= PainKillerStrength;
                    Plugin.RealHealthController.PainTunnelStrength -= TunnelVisionStrength;
                }
                else if (!addedEffect)
                {
                    addedEffect = true;
                    Plugin.RealHealthController.PainReliefStrength += PainKillerStrength;
                    Plugin.RealHealthController.PainTunnelStrength += TunnelVisionStrength;
                    Plugin.RealHealthController.ReliefDuration += (int)Duration;
                }
            }
        }
    }

    public class FoodPoisoningEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        private bool addedEffect = false;
        private float effectStrength = 1f;

        public FoodPoisoningEffect(int? dur, Player player, int delay, float strength, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            Duration = dur;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.FoodPoisoning;
            BodyPart = EBodyPart.Stomach;
            effectStrength = strength;
            RealHealthController = realHealthController;
        }

        public void Tick()
        {
            if (Delay <= 0) 
            {
                if (!addedEffect)
                {
                    _Player.ActiveHealthController.AddEffect<ResourceRateDrain>(BodyPart, 0f, null, 0f, 0f, null);
                    RealHealthController.AddBasesEFTEffect(_Player, "TunnelVision", EBodyPart.Head, 1f, 20f, 5f, 1f);
                    RealHealthController.AddToExistingBaseEFTEffect(_Player, "Contusion", EBodyPart.Head, 1f, 20f, 5f, 0.5f);
                    RealHealthController.AddBasesEFTEffect(_Player, "Tremor", EBodyPart.Head, 1f, 20f, 5f, 1f);

                    addedEffect = true;
                }
            }
        }
    }

    public class AdrenalineEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        public float PositiveEffectDuration { get; }
        public float NegativeEffectDuration { get; }
        public float EffectStrength { get; }
        private bool _addedAdrenalineEffect = false;

        public AdrenalineEffect(Player player, int? dur, int delay, float negativeEffectDur, float posEffectDur, float strength, RealismHealthController realismHealthController)
        {
            TimeExisted = 0;
            Duration = dur;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.Adrenaline;
            BodyPart = EBodyPart.Chest;
            PositiveEffectDuration = posEffectDur;
            NegativeEffectDuration = negativeEffectDur;
            EffectStrength = strength;
            RealHealthController = realismHealthController; 
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                if (!_addedAdrenalineEffect)
                {
                    RealHealthController.HasAdrenalineEffect = true;
                    RealHealthController.AddBaseEFTEffectIfNoneExisting(_Player, "PainKiller", EBodyPart.Head, 0f, PositiveEffectDuration, 3f, 1f);
                    RealHealthController.AddBasesEFTEffect(_Player, "TunnelVision", EBodyPart.Head, 0f, NegativeEffectDuration, 3f, EffectStrength);
                    RealHealthController.AddBasesEFTEffect(_Player, "Tremor", EBodyPart.Head, PositiveEffectDuration, NegativeEffectDuration, 3f, EffectStrength);
                    _addedAdrenalineEffect = true;
                }

                Duration--;
                if (Duration <= 0) 
                {
                    RealHealthController.HasAdrenalineEffect = false;
                    Duration = 0;
                }
                   
            }
        }
    }

    public class ToxicityEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }

        public ToxicityEffect(int? dur, Player player, int delay, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            Duration = dur;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.Toxicity;
            RealHealthController = realHealthController;
            BodyPart = EBodyPart.Chest;
        }

        private float GetDrainRate()
        {
            switch (HazardTracker.TotalToxicity)
            {
                case < 40f:
                    return 0f;
                case <= 50f:
                    return -0.05f;
                case <= 60f:
                    return -0.2f;
                case <= 70f:
                    return -0.35f;
                case <= 80f:
                    return -0.5f;
                case <= 90f:
                    return -0.7f;
                case < 100f:
                    return -0.9f;
                case >= 100f:
                    return -1.1f;
                default:
                    return 0f;
            }
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                TimeExisted++;
                if (TimeExisted % 3 == 0 && HazardTracker.TotalToxicity >= 50f)
                {
                    for (int i = 0; i < RealHealthController.BodyPartsArr.Length; i++)
                    {
                        EBodyPart bodyPart = RealHealthController.BodyPartsArr[i];
                        float baseDrainRate = GetDrainRate();
                        baseDrainRate *= _Player.ActiveHealthController.GetBodyPartHealth(bodyPart).Maximum / 120f;
                        _Player.ActiveHealthController.AddEffect<HealthChange>(bodyPart, 0f, 3f, 2f, baseDrainRate, null);
                    }

                }
            }
        }
    }

    public class RadiationEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }

        public RadiationEffect(int? dur, Player player, int delay, RealismHealthController realHealthController)
        {
            TimeExisted = 0;
            Duration = dur;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.Radiation;
            RealHealthController = realHealthController;
            BodyPart = EBodyPart.Chest;
        }

        private float GetDrainRate()
        {
            switch (HazardTracker.TotalRadiation)
            {
                case < 80f:
                    return 0f;
                case <= 90f:
                    return -0.05f;
                case < 100f:
                    return -0.1f;
                case >= 100f:
                    return -0.25f;
                default:
                    return 0f;
            }
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                TimeExisted++;
                if (TimeExisted % 3 == 0 && HazardTracker.TotalRadiation >= 80f)
                {
                    for (int i = 0; i < RealHealthController.BodyPartsArr.Length; i++)
                    {
                        EBodyPart bodyPart = RealHealthController.BodyPartsArr[i];
                        float baseDrainRate = GetDrainRate();
                        baseDrainRate *= _Player.ActiveHealthController.GetBodyPartHealth(bodyPart).Maximum / 120f;
                        _Player.ActiveHealthController.AddEffect<HealthChange>(bodyPart, 0f, 3f, 2f, baseDrainRate, null);
                    }
                }
            }
        }
    }

    public class RadationTreatmentEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        private float _deradRate = 0f;
        private bool _addedRate = false;

        public RadationTreatmentEffect(Player player, int? dur, int delay, RealismHealthController realismHealthController, float rate)
        {
            TimeExisted = 0;
            Duration = dur;
            _Player = player;
            Delay = delay;
            RealHealthController = realismHealthController;
            _deradRate = rate;
            EffectType = EHealthEffectType.RadiationTreatment;
            BodyPart = EBodyPart.Chest;
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                if (!_addedRate)
                {
                    HazardTracker.RadiationRateMeds += _deradRate;
                    _addedRate = true;
                }

                Duration--;
                if (Duration <= 0)
                {
                    HazardTracker.RadiationRateMeds -= _deradRate;
                    Duration = 0;
                }
            }
        }
    }

    public class DetoxificationEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        private float _detoxRate = 0f;
        private bool _addedRate = false;    

        public DetoxificationEffect(Player player, int? dur, int delay, RealismHealthController realismHealthController, float rate)
        {
            TimeExisted = 0;
            Duration = dur;
            _Player = player;
            Delay = delay;
            RealHealthController = realismHealthController;
            _detoxRate = rate;
            EffectType = EHealthEffectType.Detoxification;
            BodyPart = EBodyPart.Chest;
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                if (!_addedRate) 
                {
                    HazardTracker.ToxicityRateMeds += _detoxRate;
                    _addedRate = true;
                }
         
                Duration--;
                if (Duration <= 0) 
                {
                    HazardTracker.ToxicityRateMeds -= _detoxRate;
                    Duration = 0;
                }
            }
        }
    }

    public class StimShellEffect : ICustomHealthEffect
    {
        public RealismHealthController RealHealthController { get; set; }
        public EBodyPart BodyPart { get; set; }
        public int? Duration { get; set; }
        public int TimeExisted { get; set; }
        public Player _Player { get; }
        public int Delay { get; set; }
        public EHealthEffectType EffectType { get; }
        public EStimType StimType { get; }
        private bool _hasRemovedTrnqt = false;

        public StimShellEffect(Player player, int? dur, int delay, EStimType stimType, RealismHealthController realismHealthController)
        {
            TimeExisted = 0;
            Duration = dur;
            _Player = player;
            Delay = delay;
            EffectType = EHealthEffectType.Stim;
            BodyPart = EBodyPart.Head;
            StimType = stimType;
            RealHealthController = realismHealthController;
        }

        public void Tick()
        {
            if (Delay <= 0)
            {
                if (!_hasRemovedTrnqt && (StimType == EStimType.Regenerative || StimType == EStimType.Clotting))
                {
                    RealHealthController.RemoveEffectsOfType(EHealthEffectType.Tourniquet);
                    if (PluginConfig.EnableMedNotes.Value) NotificationManagerClass.DisplayMessageNotification("Removing Tourniquet Effects Due To Stim Type: " + StimType, EFT.Communications.ENotificationDurationType.Long);
                    _hasRemovedTrnqt = true;
                }

                Duration--;
                if (Duration <= 0) Duration = 0;
            }
        }
    }

    public class HealthDrain : EffectClass, IEffect, InterfaceOne, InterfaceTwo
    {
        private float _hpPerTick;
        private float _time;
        private EBodyPart _bodyPart;
        private EBodyPart[] _bodyParts = { EBodyPart.Chest, EBodyPart.Stomach };

        public override void Started()
        {
            this._hpPerTick = base.Strength;
            this.SetHealthRatesPerSecond(this._hpPerTick / _bodyParts.Count(), 0f, 0f, 0f);
            this._bodyPart = base.BodyPart;
        }

        public override void RegularUpdate(float deltaTime)
        {
            this._time += deltaTime;
            if (this._time < 3f)
            {
                return;
            }
            this._time -= 3f;
            foreach (EBodyPart part in _bodyParts)
            {
                if (this.HealthController.GetBodyPartHealth(part).Current > 0f) 
                {
                    this.HealthController.ApplyDamage(part, this._hpPerTick / _bodyParts.Count(), ExistanceClass.PoisonDamage);
                }
            }
        }
    }


    public class HealthChange : EffectClass, IEffect, InterfaceOne, InterfaceTwo
    {
        private float _hpPerTick;
        private float _time;
        private EBodyPart _bodyPart;

        public override void Started()
        {
            this._hpPerTick = base.Strength;
            this.SetHealthRatesPerSecond(this._hpPerTick, 0f, 0f, 0f);
            this._bodyPart = base.BodyPart;
        }

        public override void RegularUpdate(float deltaTime)
        {
            this._time += deltaTime;
            if (this._time < 3f)
            {
                return;
            }
            this._time -= 3f;
            base.HealthController.ChangeHealth(_bodyPart, this._hpPerTick, ExistanceClass.Existence);
        }
    }

    public class ResourceRateDrain : EffectClass, IEffect, InterfaceOne, InterfaceTwo
    {
        private float _resourcePerTick;
        private float _time;
        private EBodyPart _bodyPart;

        public override void Started()
        {
            this._resourcePerTick = Plugin.RealHealthController.ResourcePerTick;
            this._bodyPart = base.BodyPart;
            this.SetHealthRatesPerSecond(0f, -this._resourcePerTick, -this._resourcePerTick, 0f);
        }

        public override void RegularUpdate(float deltaTime)
        {
            this._time += deltaTime;
            if (this._time < 3f) 
            {
                return;
            }
            this._time -= 3f;
            this._resourcePerTick = Plugin.RealHealthController.ResourcePerTick;
            this.SetHealthRatesPerSecond(0f, -this._resourcePerTick * PluginConfig.EnergyRateMulti.Value, -this._resourcePerTick * PluginConfig.HydrationRateMulti.Value, 0f);
        }
    }

}
