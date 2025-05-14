using Mutators.Managers;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mutators.Network
{
    internal class MutatorsNetworkManager : MonoBehaviourPunCallbacks
    {
        internal static MutatorsNetworkManager Instance { get; private set; } = null!;

        private PhotonView _photonView = null!;

        void Awake()
        {
            Instance = this;
            transform.parent = StatsManager.instance.transform;
            gameObject.name = "MutatorsNetworkManager";
            gameObject.hideFlags &= ~HideFlags.HideAndDontSave;
            _photonView = GetComponent<PhotonView>();
        }

        internal void ClearBufferedRPCs()
        {
            if (_photonView.IsMine)
            {
                PhotonNetwork.RemoveBufferedRPCs(_photonView.ViewID);
            }
        }

        public void SendMetadata(IDictionary<string, string> metadata)
        {
            string[] keys = new string[metadata.Count];
            string[] values = new string[metadata.Count];

            int currentIndex = 0;
            foreach (KeyValuePair<string, string> pairs in metadata)
            {
                keys[currentIndex] = pairs.Key;
                values[currentIndex] = pairs.Value;
                currentIndex++;
            }

            Send(keys, values, SetMetadata, RpcTarget.OthersBuffered);
        }

        public void SendActiveMutator(string name)
        {
            Send(name, SetActiveMutator, RpcTarget.OthersBuffered);
        }

        public void SendComponentForViews(int[] views, Type componentType)
        {
            Send(views, componentType.FullName, AddComponentToViewGameObject, RpcTarget.OthersBuffered);
        }

        [PunRPC]
        public void AddComponentToViewGameObject(int[] views, string componentType)
        {
            Type? actualType = Type.GetType(componentType);

            if (actualType == null)
            {
                RepoMutators.Logger.LogError($"Failed to resolve type: {componentType}");
                return;
            }

            foreach (int view in views)
            {
                PhotonView photonView = PhotonView.Find(view);
                if (!photonView || !photonView.gameObject) continue;

                photonView.gameObject.AddComponent(actualType);
            }
        }

        [PunRPC]
        public void SetActiveMutator(string name)
        {
            bool runIsLevel = SemiFunc.RunIsLevel();

            RepoMutators.Logger.LogDebug($"Set mutator to {name}, applying patch {(runIsLevel ? "now" : "later")}");
            MutatorManager.Instance.SetActiveMutator(name, runIsLevel);
        }

        [PunRPC]
        public void SetMetadata(string[] keys, string[] values)
        {
            IDictionary<string, string> metadata = new Dictionary<string, string>();
            for (int i = 0; i < keys.Length; i++)
            {
                metadata.Add(keys[i], values[i]);
            }

            RepoMutators.Logger.LogDebug($"[RPC] Received metadata: {string.Join(", ", metadata.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

            MutatorManager.Instance.metadata = metadata;
            MutatorManager.Instance.OnMetadataChanged?.Invoke(metadata);
        }

        [PunRPC]
        public void SendModVersion()
        {
            StartCoroutine(ComparedModVersionCoroutine());
        }

        [PunRPC]
        public void CompareModVersion(string version, PhotonMessageInfo info)
        {
            if (version == MyPluginInfo.PLUGIN_VERSION)
            {
                RepoMutators.Logger.LogInfo($"{info.Sender.NickName} is on the same version!");
            }
            else
            {
                HandleVersionMismatch(version, info.Sender.NickName);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (_photonView.IsMine)
            {
                _photonView.RPC(nameof(SendModVersion), newPlayer);
            }
            
        }

        private void HandleVersionMismatch(string version, string playerName)
        {
            string[] versionSegments = version.Split('.');
            string[] hostVersionSegments = MyPluginInfo.PLUGIN_VERSION.Split('.');

            if (versionSegments[0] != hostVersionSegments[0] || versionSegments[1] != hostVersionSegments[1])
            {
                RepoMutators.Logger.LogError($"{playerName} is on version {version}, which doesn't match the host version!");
            }
            else if (versionSegments[2] != hostVersionSegments[2])
            {
                RepoMutators.Logger.LogWarning($"{playerName} is on version {version}, which doesn't match the host version!");
            }
        }

        private void Send<T>(T data, Action<T> rpcMethod, RpcTarget rpcTarget)
        {
            if (SemiFunc.IsMultiplayer())
            {
                if (SemiFunc.IsMasterClient())
                {
                    _photonView.RPC(rpcMethod.Method.Name, rpcTarget, data);
                    rpcMethod.Invoke(data);
                }
            }
            else
            {
                rpcMethod.Invoke(data);
            }
        }

        private void Send<T, D>(T data, D value, Action<T, D> rpcMethod, RpcTarget rpcTarget)
        {
            if (SemiFunc.IsMultiplayer())
            {
                if (SemiFunc.IsMasterClient())
                {
                    _photonView.RPC(rpcMethod.Method.Name, rpcTarget, data, value);
                    rpcMethod.Invoke(data, value);
                }
            }
            else
            {
                rpcMethod.Invoke(data, value);
            }
        }

        private IEnumerator ComparedModVersionCoroutine()
        {
            while (_photonView == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            _photonView.RPC(nameof(CompareModVersion), RpcTarget.MasterClient, MyPluginInfo.PLUGIN_VERSION);
        }

        internal void Run(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}
