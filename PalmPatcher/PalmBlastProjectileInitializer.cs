using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace PalmPatcher
{
    public class PalmBlastProjectileInitializer : MonoBehaviour
    {
        void Awake()
        {
            if (TryGetComponent(out ProjectileController projectileController))
            {
                projectileController.onInitialized += onInitialized;
            }
        }

        void onInitialized(ProjectileController projectileController)
        {
            if (TryGetComponent(out PalmBlastProjectileController palmBlastProjectileController))
            {
                CharacterBody ownerBody = projectileController.owner ? projectileController.owner.GetComponent<CharacterBody>() : null;
                if (ownerBody)
                {
                    palmBlastProjectileController.Init(ownerBody);

                    if (TryGetComponent(out ProjectileDamage projectileDamage))
                    {
                        projectileDamage.damage = ownerBody.damage;
                    }
                }
            }
        }
    }
}
