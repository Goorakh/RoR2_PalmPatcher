using BepInEx;
using RoR2;
using System.Diagnostics;

namespace PalmPatcher
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PalmPatcherPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "PalmPatcher";
        public const string PluginVersion = "1.0.0";

        static PalmPatcherPlugin _instance;
        internal static PalmPatcherPlugin Instance => _instance;

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            SingletonHelper.Assign(ref _instance, this);

            Log.Init(Logger);

            On.PalmBlastProjectileController.Init += PalmBlastProjectileController_Init;

            stopwatch.Stop();
            Log.Message_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }

        void OnDestroy()
        {
            SingletonHelper.Unassign(ref _instance, this);
        }

        static void PalmBlastProjectileController_Init(On.PalmBlastProjectileController.orig_Init orig, PalmBlastProjectileController self, CharacterBody body)
        {
            orig(self, body);

            if (self && self.projectileDamage)
            {
                CharacterBody owner = self.ownerBody;
                if (!owner)
                    owner = body;

                if (owner)
                {
                    self.projectileDamage.damage = owner.damage;
                }
            }
        }
    }
}
