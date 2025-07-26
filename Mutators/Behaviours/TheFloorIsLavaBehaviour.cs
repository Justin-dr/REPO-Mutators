using Mutators.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class TheFloorIsLavaBehaviour : MonoBehaviour
    {
        private PlayerAvatar playerAvatar;
        private Transform physObjectStander;
        private LayerMask layerMask = ~LayerMask.GetMask("Player", "CollisionCheck");

        internal float immunityTimer = 0f;
        private float groundedTimer = 0f;
        private float damageCooldownTimer = 0f;
        private readonly float damageCooldown = 1f; // 1 second cooldown between damage
        private readonly float damageInterval = 1f;
        private readonly float raycastDistance = 0.5f;

        internal int damagePerTick = MutatorSettings.TheFloorIsLava.DamagePerTick;
        internal bool usePercentageDamage = MutatorSettings.TheFloorIsLava.UsePercentageDamage;

        private int playerOnlyCollisionLayer;

        internal bool Immune { get; set; }

        void Start ()
        {
            playerAvatar = GetComponent<PlayerAvatar>();
            physObjectStander = playerAvatar.transform.Find("Phys Object Stander");
            playerOnlyCollisionLayer = LayerMask.NameToLayer("PlayerOnlyCollision");
        }

        void Update()
        {
            if (!LevelGenerator.Instance.Generated) return;
            if (playerAvatar.RoomVolumeCheck.inTruck || playerAvatar.RoomVolumeCheck.inExtractionPoint) return;

            if (immunityTimer > 0)
            {
                immunityTimer -= Time.deltaTime;
                return;
            }

            if (Immune) return;

            damageCooldownTimer += Time.deltaTime;
            groundedTimer += Time.deltaTime;

            bool onDamagingGround = false;

            if (Physics.Raycast(physObjectStander.position, Vector3.down, out RaycastHit hit, raycastDistance, layerMask))
            {
                Collider hitCollider = hit.collider;

                if (hitCollider.CompareTag("Floor") || hitCollider.CompareTag("Wall"))
                {
                    onDamagingGround = true;
                }
                else if (hitCollider.CompareTag("Untagged") && (playerAvatar.MaterialTrigger.LastMaterialType == Materials.Type.Catwalk || hit.collider.gameObject.layer == playerOnlyCollisionLayer))
                {
                    onDamagingGround = true;
                }

                // RepoMutators.Logger.LogDebug($"I'm standing on {hit.collider.tag}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, Type: {playerAvatar.MaterialTrigger.LastMaterialType}");
            }

            if (onDamagingGround)
            {
                if (groundedTimer >= damageInterval && damageCooldownTimer >= damageCooldown)
                {
                    int damage = damagePerTick;
                    if (usePercentageDamage)
                    {
                        damage = (int)MathF.Ceiling(playerAvatar.playerHealth.maxHealth * (damage / 100f));
                    }

                    playerAvatar.playerHealth.Hurt(damage, false);
                    groundedTimer = 0f;
                    damageCooldownTimer = 0f;
                }
            }
        }
    }
}
