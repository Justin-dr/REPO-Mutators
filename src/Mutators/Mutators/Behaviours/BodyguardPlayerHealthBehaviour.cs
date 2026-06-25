using System;
using System.Collections;
using System.Collections.Generic;
using Mutators.Network;
using Mutators.Settings;
using Photon.Pun;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class BodyguardPlayerHealthBehaviour : MonoBehaviour
    {
        private PlayerAvatar _playerAvatar = null!;
        private PlayerHealth _playerHealth = null!;
        internal int originalHealth = 100;
        internal int originalMaxHealth = 100;
        private bool _initDone;

        internal string? BodyguardId { get; set; }

        void Awake()
        {
            _playerAvatar = PlayerAvatar.instance;
            _playerHealth = _playerAvatar.playerHealth;
            StartCoroutine(GetAndSetHealthLate());
        }

        private IEnumerator GetAndSetHealthLate()
        {
            while (!_playerHealth.healthSet)
            {
                yield return new WaitForSeconds(0.1f);
            }

            originalHealth = _playerHealth.health;
            originalMaxHealth = _playerHealth.maxHealth;

            SendHealth();

            while (BodyguardId == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            UpdateHealth();
        }

        internal void UpdateHealth()
        {
            bool isBodyguard = BodyguardId == _playerAvatar.steamID;
            if (isBodyguard)
            {
                if (!_initDone)
                {
                    float multiplier = CalculateBodyguardHealthMultiplier(_playerHealth.maxHealth);

                    _playerHealth.health = (int)(_playerHealth.health * multiplier);
                    _playerHealth.maxHealth = (int)(_playerHealth.maxHealth * multiplier);
                }
                else
                {
                    float multiplier = CalculateBodyguardHealthMultiplier(originalMaxHealth);

                    _playerHealth.health = (int)(originalMaxHealth * multiplier);
                    _playerHealth.maxHealth = (int)(originalMaxHealth * multiplier);
                }
            }
            else
            {
                _playerHealth.health = 60;
                _playerHealth.maxHealth = 60;
            }

            SetHealth(_playerHealth.health, _playerHealth.maxHealth);
            
            _initDone = true;
        }

        internal void RestoreOriginalHealth()
        {
            SetHealth(originalHealth, originalMaxHealth);
        }

        private void SetHealth(int health, int maxHealth, bool setInShop = false)
        {
            StatsManager.instance.SetPlayerHealth(_playerAvatar.steamID, health, setInShop);

            if (SemiFunc.IsMultiplayer())
            {
                _playerHealth.photonView.RPC("UpdateHealthRPC", RpcTarget.Others, health, maxHealth, true, false);
            }
        }

        internal void SendHealth()
        {
            Dictionary<string, object> clientMeta = new Dictionary<string, object>
            {
                { "originalHealth", originalHealth }
            };

            MutatorsNetworkManager.Instance.SendMetaToHost(MutatorSettings.ProtectTheWeak.NamespacedName, clientMeta);
        }

        private static float CalculateBodyguardHealthMultiplier(int health)
        {
            const float maxMultiplier = 2f;
            const float minMultiplier = 1.1f;
            const float scaleFactor = 125;

            int effectiveHealth = Math.Max(health, 100);

            float decay = scaleFactor / (effectiveHealth - 100 + scaleFactor);
            float multiplier = minMultiplier + (maxMultiplier - minMultiplier) * decay;

            return multiplier;
        }
    }
}
