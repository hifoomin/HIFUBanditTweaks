using R2API;

namespace HIFUBanditTweaks
{
    public abstract class TweakBase
    {
        public abstract string Name { get; }
        public abstract string SkillToken { get; }
        public abstract string DescText { get; }
        public virtual bool isEnabled { get; } = true;

        public T ConfigOption<T>(T value, string name, string description)
        {
            var config = Main.HBTConfig.Bind<T>(Name, name, value, description);
            ConfigManager.HandleConfig<T>(config, Main.HBTBackupConfig, name);
            return config.Value;
        }

        public abstract void Hooks();

        public string d(float f)
        {
            return (f * 100f).ToString() + "%";
        }

        public virtual void Init()
        {
            Hooks();
            string descriptionToken = "BANDIT2_" + SkillToken.ToUpper() + "_DESCRIPTION";
            LanguageAPI.Add(descriptionToken, DescText);
            Main.HBTLogger.LogInfo("Added " + Name);
        }
    }
}