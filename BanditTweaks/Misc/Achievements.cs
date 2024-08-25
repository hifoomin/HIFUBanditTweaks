using HIFUBanditTweaks.Skills;
using MonoMod.RuntimeDetour;
using R2API;
using RoR2;
using RoR2.Achievements.Bandit2;
using System.Reflection;
using static R2API.DamageAPI;

namespace HIFUBanditTweaks.Misc
{
    public class Achievements : MiscBase<Achievements>
    {
        public static Hook hook;
        public static int hemoRequirement;

        public override string Name => ": Misc : Achievements";

        public override void Init()
        {
            hemoRequirement = ConfigOption(4, "Hemorrhage Required", "Vanilla is 20");
            base.Init();
        }

        public override void Hooks()
        {
            On.RoR2.Achievements.Bandit2.Bandit2StackSuperBleedAchievement.LookUpRequiredBodyIndex += Bandit2StackSuperBleedAchievement_LookUpRequiredBodyIndex;
            On.RoR2.Achievements.Bandit2.Bandit2RevolverFinaleAchievement.Bandit2RevolverFinaleServerAchievement.DoesDamageQualify += Bandit2RevolverFinaleServerAchievement_DoesDamageQualify;
            hook = new(typeof(Bandit2ConsecutiveResetAchievement.Bandit2ConsecutiveResetServerAchievement).GetMethod(nameof(Bandit2ConsecutiveResetAchievement.Bandit2ConsecutiveResetServerAchievement.OnCharacterDeathGlobal), BindingFlags.NonPublic | BindingFlags.Instance), typeof(Achievements).GetMethod(nameof(OnKillDamageTypeHook), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            Changes();
        }

        private static void OnKillDamageTypeHook(Bandit2ConsecutiveResetAchievement.Bandit2ConsecutiveResetServerAchievement orig, DamageReport damageReport)
        {
            if (damageReport.attackerBody == orig.trackedBody && damageReport.attackerBody && ((damageReport.damageInfo.damageType & DamageType.ResetCooldownsOnKill) == DamageType.ResetCooldownsOnKill) || DamageAPI.HasModdedDamageType(damageReport.damageInfo, LightsOut.cooldownReset))
            {
                orig.waitingForKill = false;
                orig.progress++;
                if (orig.progress >= Bandit2ConsecutiveResetAchievement.requirement)
                {
                    orig.Grant();
                }
            }
        }

        private bool Bandit2RevolverFinaleServerAchievement_DoesDamageQualify(On.RoR2.Achievements.Bandit2.Bandit2RevolverFinaleAchievement.Bandit2RevolverFinaleServerAchievement.orig_DoesDamageQualify orig, RoR2.Achievements.BaseServerAchievement self, DamageReport damageReport)
        {
            return (damageReport.damageInfo.damageType & DamageType.ResetCooldownsOnKill) == DamageType.ResetCooldownsOnKill || DamageAPI.HasModdedDamageType(damageReport.damageInfo, LightsOut.cooldownReset);
        }

        private BodyIndex Bandit2StackSuperBleedAchievement_LookUpRequiredBodyIndex(On.RoR2.Achievements.Bandit2.Bandit2StackSuperBleedAchievement.orig_LookUpRequiredBodyIndex orig, RoR2.Achievements.Bandit2.Bandit2StackSuperBleedAchievement self)
        {
            Bandit2StackSuperBleedAchievement.requirement = hemoRequirement;
            return orig(self);
        }

        private void Changes()
        {
            LanguageAPI.Add("ACHIEVEMENT_BANDIT2STACKSUPERBLEED_DESCRIPTION", "As Bandit, kill a monster with " + hemoRequirement + " stacks of Hemorrhage.");
        }
    }
}