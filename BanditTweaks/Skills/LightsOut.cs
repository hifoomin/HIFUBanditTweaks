using R2API;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static R2API.DamageAPI;

namespace HBT.Skills
{
    public class LightsOut : TweakBase<LightsOut>
    {
        public static float Damage;
        public static float Cooldown;
        public static ModdedDamageType cooldownReset = ReserveDamageType();
        public static float CooldownReduction;

        public override string Name => ": Special : Lights Out";

        public override string SkillToken => "special";

        public override string DescText => "<style=cIsDamage>Slayer</style>. Fire a revolver shot for <style=cIsDamage>" + d(Damage) + " damage</style>. Kills <style=cIsUtility>reset Lights Out's cooldown</style> and <style=cIsUtility>reduce other cooldowns</style> by <style=cIsUtility>" + d(CooldownReduction) + "</style>.";

        public override void Init()
        {
            Cooldown = ConfigOption(6f, "Cooldown", "Vanilla is 4");
            Damage = ConfigOption(8f, "Damage", "Decimal. Vanilla is 6");
            CooldownReduction = ConfigOption(0.3f, "Non-Special Cooldown Reduction on Kill", "Decimal. Vanilla is 1");
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.OnEnter += BaseFireSidearmRevolverState_OnEnter;
            On.EntityStates.Bandit2.Weapon.FireSidearmResetRevolver.ModifyBullet += FireSidearmResetRevolver_ModifyBullet;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            Changes();
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            var damageInfo = damageReport.damageInfo;

            if (DamageAPI.HasModdedDamageType(damageInfo, cooldownReset))
            {
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/Bandit2ResetEffect"), new EffectData
                {
                    origin = damageInfo.position
                }, true);

                var attacker = damageReport.attacker;
                if (attacker)
                {
                    new SyncCooldownReduction(attacker.GetComponent<NetworkIdentity>().netId, CooldownReduction).Send(R2API.Networking.NetworkDestination.Clients);

                    var sl = attacker.GetComponent<SkillLocator>();
                    if (sl)
                    {
                        var primary = sl.primary;
                        var secondary = sl.secondary;
                        var utility = sl.utility;
                        var special = sl.special;
                        if (primary && primary.stock < primary.maxStock)
                        {
                            primary.rechargeStopwatch += primary.finalRechargeInterval * CooldownReduction;
                        }
                        if (secondary && secondary.stock < secondary.maxStock)
                        {
                            secondary.rechargeStopwatch += secondary.finalRechargeInterval * CooldownReduction;
                        }
                        if (utility && utility.stock < utility.maxStock)
                        {
                            utility.rechargeStopwatch += utility.finalRechargeInterval * CooldownReduction;
                        }
                        if (special && special.stock < special.maxStock)
                        {
                            special.rechargeStopwatch += special.finalRechargeInterval * 1f;
                        }
                    }
                }
            }
        }

        private void FireSidearmResetRevolver_ModifyBullet(On.EntityStates.Bandit2.Weapon.FireSidearmResetRevolver.orig_ModifyBullet orig, EntityStates.Bandit2.Weapon.FireSidearmResetRevolver self, RoR2.BulletAttack bulletAttack)
        {
            orig(self, bulletAttack);
            DamageAPI.AddModdedDamageType(bulletAttack, cooldownReset);
            bulletAttack.damageType &= ~DamageType.ResetCooldownsOnKill;
        }

        private void BaseFireSidearmRevolverState_OnEnter(On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.orig_OnEnter orig, EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState self)
        {
            if (self is EntityStates.Bandit2.Weapon.FireSidearmResetRevolver)
            {
                self.damageCoefficient = Damage;
            }
            orig(self);
        }

        private void Changes()
        {
            var reset = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Bandit2/ResetRevolver.asset").WaitForCompletion();
            reset.baseRechargeInterval = Cooldown;
        }
    }

    public class SyncCooldownReduction : INetMessage
    {
        private NetworkInstanceId objID;
        private float cooldownReduction;

        public SyncCooldownReduction()
        { }

        public SyncCooldownReduction(NetworkInstanceId objID, float cooldownReduction)
        {
            this.objID = objID;
            this.cooldownReduction = cooldownReduction;
        }

        public void Deserialize(NetworkReader reader)
        {
            objID = reader.ReadNetworkId();
            cooldownReduction = reader.ReadSingle();
        }

        public void OnReceived()
        {
            if (NetworkServer.active) return;
            var obj = Util.FindNetworkObject(objID);
            if (obj)
            {
                var sl = obj.GetComponent<SkillLocator>();
                if (sl)
                {
                    var primary = sl.primary;
                    var secondary = sl.secondary;
                    var utility = sl.utility;
                    var special = sl.special;
                    if (primary && primary.stock < primary.maxStock)
                    {
                        primary.rechargeStopwatch += primary.finalRechargeInterval * cooldownReduction;
                    }
                    if (secondary && secondary.stock < secondary.maxStock)
                    {
                        secondary.rechargeStopwatch += secondary.finalRechargeInterval * cooldownReduction;
                    }
                    if (utility && utility.stock < utility.maxStock)
                    {
                        utility.rechargeStopwatch += utility.finalRechargeInterval * cooldownReduction;
                    }
                    if (special && special.stock < special.maxStock)
                    {
                        special.rechargeStopwatch += special.finalRechargeInterval * 1f;
                    }
                }
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(objID);
            writer.Write(cooldownReduction);
        }
    }
}