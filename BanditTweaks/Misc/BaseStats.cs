using HBT;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUBanditTweaks.Misc
{
    public class BaseStats : MiscBase
    {
        public static float MovementSpeed;

        public override string Name => ": Misc :: Base Stats";

        public override void Init()
        {
            MovementSpeed = ConfigOption(7.5f, "Base Move Speed", "Vanilla is 7");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        private void Changes()
        {
            var bandit = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2Body.prefab").WaitForCompletion().GetComponent<CharacterBody>();
            bandit.baseMoveSpeed = MovementSpeed;
        }
    }
}