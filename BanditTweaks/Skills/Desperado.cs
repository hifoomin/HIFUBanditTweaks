using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUBanditTweaks.Skills
{
    public class Desperado : TweakBase<Desperado>
    {
        public static float Damage;
        public static float StackDamage;
        public static float Cooldown;

        public override string Name => ": Special :: Desperado";

        public override string SkillToken => "special_alt";

        public override string DescText => "<style=cIsDamage>Slayer</style>. Fire a revolver shot for <style=cIsDamage>" + d(Damage) + " damage</style>. Kills grant <style=cIsDamage>stacking tokens</style> for <style=cIsDamage>" + d(StackDamage * Damage) + "</style> more Desperado skill damage.";

        public override void Init()
        {
            Cooldown = ConfigOption(6f, "Cooldown", "Vanilla is 4");
            Damage = ConfigOption(8f, "Damage", "Decimal. Vanilla is 6");
            StackDamage = ConfigOption(0.055f, "Damage Per Token", "Vanilla is 0.1. Additive Damage increase per token = Damage Per Token * Damage");
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.OnEnter += BaseFireSidearmRevolverState_OnEnter;
            IL.EntityStates.Bandit2.Weapon.FireSidearmSkullRevolver.ModifyBullet += FireSidearmSkullRevolver_ModifyBullet;
            Changes();
        }

        private void FireSidearmSkullRevolver_ModifyBullet(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.1f)))
            {
                c.Next.Operand = StackDamage;
            }
            else
            {
                Main.HBTLogger.LogError("Failed to apply Desperado Stacking Damage hook");
            }
        }

        private void BaseFireSidearmRevolverState_OnEnter(On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.orig_OnEnter orig, EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState self)
        {
            if (self is EntityStates.Bandit2.Weapon.FireSidearmSkullRevolver)
            {
                self.damageCoefficient = Damage;
            }
            orig(self);
        }

        private void Changes()
        {
            var skullemoji = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Bandit2/SkullRevolver.asset").WaitForCompletion();
            skullemoji.baseRechargeInterval = Cooldown;
            skullemoji.canceledFromSprinting = false;
        }
    }

    public static class Specials
    {
        public static void Init()
        {
            On.EntityStates.Bandit2.Weapon.BaseSidearmState.FixedUpdate += BaseSidearmState_FixedUpdate;
            On.EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState.FixedUpdate += BasePrepSidearmRevolverState_FixedUpdate;
        }

        private static void BasePrepSidearmRevolverState_FixedUpdate(On.EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState.orig_FixedUpdate orig, EntityStates.Bandit2.Weapon.BasePrepSidearmRevolverState self)
        {
            self.fixedAge += Time.fixedDeltaTime;
            if (self.fixedAge >= self.duration && !self.inputBank.skill4.down)
            {
                self.outer.SetNextState(self.GetNextState());
            }
        }

        public static void BaseSidearmState_FixedUpdate(On.EntityStates.Bandit2.Weapon.BaseSidearmState.orig_FixedUpdate orig, EntityStates.Bandit2.Weapon.BaseSidearmState self)
        {
            self.fixedAge += Time.fixedDeltaTime;
        }
    }
}