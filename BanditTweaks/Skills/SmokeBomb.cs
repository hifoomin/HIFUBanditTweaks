using RoR2.Skills;
using UnityEngine.AddressableAssets;

namespace HBT.Skills
{
    public class SmokeBomb : TweakBase
    {
        public static float Cooldown;
        public static float CloakDur;
        public static float Damage;

        public override string Name => ": Utility : Smoke Bomb";

        public override string SkillToken => "utility";

        public override string DescText => "<style=cIsDamage>Stunning</style>. Deal <style=cIsDamage>" + d(Damage) + " damage</style>, become <style=cIsUtility>invisible</style> for <style=cIsUtility>" + CloakDur + "</style> seconds, then deal <style=cIsDamage>" + d(Damage) + " damage</style> again.";

        public override void Init()
        {
            Cooldown = ConfigOption(8f, "Cooldown", "Vanilla is 6");
            CloakDur = ConfigOption(3f, "Cloak Duration", "Vanilla is 3");
            Damage = ConfigOption(2f, "Damage", "Decimal. Vanilla is 2");
            base.Init();
        }

        public override void Hooks()
        {
            On.EntityStates.Bandit2.StealthMode.OnEnter += StealthMode_OnEnter;
            Changes();
        }

        private void StealthMode_OnEnter(On.EntityStates.Bandit2.StealthMode.orig_OnEnter orig, EntityStates.Bandit2.StealthMode self)
        {
            EntityStates.Bandit2.StealthMode.duration = CloakDur;
            EntityStates.Bandit2.StealthMode.blastAttackDamageCoefficient = Damage;
            orig(self);
        }

        private void Changes()
        {
            var cloak = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Bandit2/ThrowSmokebomb.asset").WaitForCompletion();
            cloak.baseRechargeInterval = Cooldown;
        }
    }
}