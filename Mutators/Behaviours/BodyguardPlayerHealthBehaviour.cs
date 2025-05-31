using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators.Patches;
using Mutators.Network;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class BodyguardPlayerHealthBehaviour : MonoBehaviour
    {
        public BodyguardPlayerHealthBehaviour instance;
        private PlayerAvatar playerAvatar;
        private PlayerHealth playerHealth;
        internal int originalHealth = 100;
        internal int originalMaxHealth = 100;
        private bool initDone = false;

        void Awake()
        {
            instance = this;
            playerAvatar = PlayerAvatar.instance;
            playerHealth = playerAvatar.playerHealth;
            StartCoroutine(GetAndSetHealthLate());
        }

        private IEnumerator GetAndSetHealthLate()
        {
            while (!playerHealth.healthSet)
            {
                yield return new WaitForSeconds(0.1f);
            }

            originalHealth = playerHealth.health;
            originalMaxHealth = playerHealth.maxHealth;

            SendHealth();

            while (GetBodyguardId() == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            UpdateHealth();
        }

        internal void UpdateHealth()
        {
            bool isBodyguard = GetBodyguardId() == playerAvatar.steamID;
            if (isBodyguard)
            {
                if (!initDone)
                {
                    float multiplier = CalculateBodyguardHealthMultiplier(playerHealth.maxHealth);

                    playerHealth.health = (int)(playerHealth.health * multiplier);
                    playerHealth.maxHealth = (int)(playerHealth.maxHealth * multiplier);
                }
                else
                {
                    float multiplier = CalculateBodyguardHealthMultiplier(originalMaxHealth);

                    playerHealth.health = (int)(originalMaxHealth * multiplier);
                    playerHealth.maxHealth = (int)(originalMaxHealth * multiplier);
                }
            }
            else
            {
                playerHealth.health = 60;
                playerHealth.maxHealth = 60;
            }

            SetHealth(playerHealth.health, playerHealth.maxHealth);
            
            initDone = true;
        }

        internal void RestoreOriginalHealth()
        {
            SetHealth(originalHealth, originalMaxHealth);
        }

        private void SetHealth(int health, int maxHealth, bool setInShop = false)
        {
            StatsManager.instance.SetPlayerHealth(playerAvatar.steamID, health, setInShop);

            if (SemiFunc.IsMultiplayer())
            {
                playerHealth.photonView.RPC("UpdateHealthRPC", RpcTarget.Others, health, maxHealth, true);
            }
        }

        internal void SendHealth()
        {
            var clientMeta = new System.Collections.Generic.Dictionary<string, object>()
            {
                { "originalHealth", originalHealth }
            };

            MutatorsNetworkManager.Instance.SendMetaToHost(playerAvatar.steamID, clientMeta);
        }

        private string GetBodyguardId()
        {
            return MutatorManager.Instance.Metadata.Get<string>(ProtectTheWeakPatch.BodyGuardId);
        }

        private static float CalculateBodyguardHealthMultiplier(int health)
        {

            float maxMultiplier = 2f;
            float minMultiplier = 1.1f;
            float scaleFactor = 125;

            int effectiveHealth = Math.Max(health, 100);

            float decay = scaleFactor / (effectiveHealth - 100 + scaleFactor);
            float multiplier = minMultiplier + (maxMultiplier - minMultiplier) * decay;

            return multiplier;
        }
    }
}
