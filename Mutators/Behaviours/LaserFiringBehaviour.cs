﻿using Mutators.Mutators.Behaviours.Custom;
using Mutators.Mutators.Behaviours.UI;
using Mutators.Settings;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Mutators.Mutators.Behaviours
{
    internal class LaserFiringBehaviour : MonoBehaviour, IPunInstantiateMagicCallback
    {
        private PhotonView photonView;
        private PlayerAvatar playerAvatar;
        private SemiLaser semiLaser;
        private Transform visionTransform;
        private float laserTimer;

        internal float laserCooldown = 100f;
        internal float laserCooldownTimer = 100f;

        internal bool canFire = true;
        internal bool manualActionEnabled = true;

        void Awake()
        {
            photonView = GetComponent<PhotonView>();
        }

        void Start()
        {
            playerAvatar = transform.parent.GetComponent<PlayerAvatar>();

            visionTransform = playerAvatar.transform.Find("Vision Target");
            semiLaser = transform.Find("SemiLaser").GetComponent<SemiLaser>();
            transform.GetComponentInChildren<PlayerIgnoringHurtCollider>(true).ignoredPlayers.Add(playerAvatar);
        }

        void Update()
        {
            if (playerAvatar == PlayerAvatar.instance)
            {
                if (laserCooldownTimer < laserCooldown)
                {
                    laserCooldownTimer += Time.deltaTime;
                }

                if (manualActionEnabled && laserCooldownTimer >= laserCooldown && !ChatManager.instance.StateIsActive() && Input.GetKeyDown(RepoMutators.Settings.SpecialActionKey))
                {
                    FireLaser(2.5f);
                    laserCooldownTimer = 0;
                }

                SpecialActionAnnouncingBehaviour specialActionAnnouncing = SpecialActionAnnouncingBehaviour.instance;
                if (specialActionAnnouncing?.Text != null && specialActionAnnouncing?.TextMax != null)
                {
                    if (manualActionEnabled)
                    {
                        specialActionAnnouncing.Text.text = $"{(int)laserCooldownTimer}";
                        specialActionAnnouncing.TextMax.text = $"/{(int)laserCooldown}";
                    }
                    else
                    {
                        specialActionAnnouncing.Hide();
                    }
                    
                }
            }

            if (IsActive())
            {
                laserTimer -= Time.deltaTime;
                bool isHitting = false;
                Vector3 endPosition = visionTransform.position + visionTransform.forward * 15f;
                if (Physics.Raycast(visionTransform.position, visionTransform.forward, out var hitInfo, 15f, SemiFunc.LayerMaskGetVisionObstruct()))
                {
                    endPosition = hitInfo.point;
                    isHitting = true;
                }

                semiLaser.LaserActive(visionTransform.position, endPosition, isHitting);
            }
        }


        [PunRPC]
        public void StaffLaserRPC(float _time)
        {
            laserTimer = _time;
        }

        public void FireLaser(float _time)
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC(nameof(StaffLaserRPC), RpcTarget.All, _time);
            }
            else
            {
                StaffLaserRPC(_time);
            }
        }

        public void StopLaser(bool localOnly = false)
        {
            if (localOnly)
            {
                laserTimer = 0;
            }
            else
            {
                FireLaser(0);
            }
        }

        public bool IsActive()
        {
            return laserTimer > 0;
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] data = info.photonView.InstantiationData;

            if (data.Length < 4)
            {
                RepoMutators.Logger.LogWarning("Received invalid data for LaserFiringBehaviour");
                return;
            }

            if (data[0] is string steamId)
            {
                PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);
                transform.SetParent(playerAvatar.transform, false);
            }
            if (data[1] is int laserActionCooldown)
            {
                laserCooldown = laserActionCooldown;
                laserCooldownTimer = laserActionCooldown;
            }
            if (data[2] is int laserDamage)
            {
                StartCoroutine(ApplyDamageDelayed(laserDamage));
            }
            if (data[3] is bool allowManualAction)
            {
                manualActionEnabled = allowManualAction;
            }
        }

        private IEnumerator ApplyDamageDelayed(int damage)
        {
            while (semiLaser == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            semiLaser.GetComponentInChildren<PlayerIgnoringHurtCollider>(true).enemyDamage = damage;
        }
    }
}
