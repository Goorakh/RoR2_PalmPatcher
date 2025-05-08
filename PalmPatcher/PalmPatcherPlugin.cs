using BepInEx;
using EntityStates.Seeker;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PalmPatcher.Utilities.Extensions;
using RoR2;
using RoR2.Projectile;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace PalmPatcher
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PalmPatcherPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "PalmPatcher";
        public const string PluginVersion = "1.1.0";

        static PalmPatcherPlugin _instance;
        internal static PalmPatcherPlugin Instance => _instance;

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            SingletonHelper.Assign(ref _instance, this);

            Log.Init(Logger);

            IL.EntityStates.Seeker.BaseFirePalmBlast.FireProjectile += BaseFirePalmBlast_FireProjectile;

            static void addPalmInitializer(GameObject palmPrefab)
            {
                if (!palmPrefab.GetComponent<PalmBlastProjectileInitializer>())
                {
                    palmPrefab.AddComponent<PalmBlastProjectileInitializer>();
                }
            }

            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/PalmBlastProjectile.prefab").CallOnSuccess(addPalmInitializer);
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/PalmBlastChargedProjectile.prefab").CallOnSuccess(addPalmInitializer);

            stopwatch.Stop();
            Log.Message_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }

        void OnDestroy()
        {
            SingletonHelper.Unassign(ref _instance, this);
        }

        static void BaseFirePalmBlast_FireProjectile(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                               x => x.MatchCallOrCallvirt(typeof(NetworkServer), "get_" + nameof(NetworkServer.active))))
            {
                Log.Error("Failed to find patch location");
                return;
            }

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(actualFireProjectile);

            static void actualFireProjectile(BaseFirePalmBlast self)
            {
                if (self.isAuthority)
                {
                    GameObject palmProjectilePrefab = self.charge < 1f ? self.projectilePrefab : self.chargedProjectilePrefab;

                    Ray aimRay = self.GetAimRay();
                    TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref aimRay, palmProjectilePrefab, self.gameObject);

                    // Damage and speed are initialized by PalmBlastProjectileController.Init,
                    // not ideal to still do it there, but this is the less intrusive solution
                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                    {
                        projectilePrefab = palmProjectilePrefab,
                        owner = self.gameObject,
                        position = aimRay.origin,
                        rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                        crit = self.RollCrit()
                    });
                }
            }

            // Turn server active check into if (false) to skip original code
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_0);
        }
    }
}
