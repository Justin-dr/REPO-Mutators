using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Network.Meta;
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

        private readonly bool _debug = false;

        // private readonly IDictionary<string, IAllowedClientMeta> allowedClientMeta = new Dictionary<string, IAllowedClientMeta>();

        void Awake()
        {
            Instance = this;
            transform.parent = StatsManager.instance.transform;
            gameObject.name = "MutatorsNetworkManager";
            gameObject.hideFlags &= ~HideFlags.HideAndDontSave;
            _photonView = GetComponent<PhotonView>();

            if (_debug)
            {
                Run(PrintViewId());
            }
        }

        private IEnumerator PrintViewId()
        {
            while (true)
            {
                yield return new WaitForSeconds(3);
                RepoMutators.Logger.LogInfo($"ViewID - {_photonView.ViewID}");
            }
        }

        internal void ClearBufferedRPCs()
        {
            if (_photonView.IsMine)
            {
                PhotonNetwork.RemoveBufferedRPCs(_photonView.ViewID);
            }
        }

        public void SendMetadata(IDictionary<string, object> metadata)
        {
            Send(metadata.ToPhotonHashtable(), SetMetadata, RpcTarget.OthersBuffered);
        }

        public void SendActiveMutator(string name, IDictionary<string, object>? metadata = null)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            ExitGames.Client.Photon.Hashtable? hashtable = metadata?.ToPhotonHashtable();
            Send(name, hashtable, SetActiveMutator, RpcTarget.OthersBuffered);
        }

        public void SendComponentForViews(int[] views, Type componentType)
        {
            Send(views, componentType.FullName, AddComponentToViewGameObject, RpcTarget.OthersBuffered);
        }

        internal void SendScaleChange(int photonViewId, float scale)
        {
            Send(photonViewId, scale, SetScale, RpcTarget.OthersBuffered);
        }

        public void SendMetaToHost(string sender, IDictionary<string, object> meta)
        {
            ExitGames.Client.Photon.Hashtable data = meta.ToPhotonHashtable();
            if (SemiFunc.IsMultiplayer())
            {
                if (!SemiFunc.IsMasterClient())
                {
                    _photonView.RPC(nameof(SetClientMetadata), RpcTarget.MasterClient, sender, data);
                }
            }
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
        public void SetActiveMutator(string name, ExitGames.Client.Photon.Hashtable? hashtable)
        {
            bool runIsLevel = SemiFunc.RunIsLevel();

            RepoMutators.Logger.LogDebug($"Set mutator to {name}, applying patch {(runIsLevel ? "now" : "later")}");
            MutatorManager.Instance.SetActiveMutator(name, runIsLevel);

            IDictionary<string, object> metadata = hashtable == null ? new Dictionary<string, object>() : hashtable.FromPhotonHashtable();

            MutatorManager mutatorManager = MutatorManager.Instance;

            mutatorManager.metadata = metadata;
            mutatorManager.OnMetadataChanged?.Invoke(metadata);
        }

        [PunRPC]
        public void SetMetadata(ExitGames.Client.Photon.Hashtable hashtable)
        {
            MutatorManager mutatorManager = MutatorManager.Instance;
            IDictionary<string, object> metadata = hashtable.FromPhotonHashtable();

            RepoMutators.Logger.LogDebug($"[RPC] Received metadata: {string.Join(", ", metadata.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

            metadata = mutatorManager.metadata.DeepMergedWith(metadata);

            mutatorManager.metadata = metadata;
            mutatorManager.OnMetadataChanged?.Invoke(metadata);
        }

        [PunRPC]
        public void SetClientMetadata(string steamId, ExitGames.Client.Photon.Hashtable hashtable)
        {
            MutatorManager mutatorManager = MutatorManager.Instance;
            IDictionary<string, object> incomingClientMeta = hashtable.FromPhotonHashtable();

            //if (!ValidateClientMeta(incomingClientMeta, allowedClientMeta))
            //{
            //    RepoMutators.Logger.LogWarning("Received illegal client meta, skipping");
            //    return;
            //}

            IDictionary<string, object> sender = new Dictionary<string, object>()
            {
                { steamId, incomingClientMeta}
            };

            IDictionary<string, object> clientMeta = new Dictionary<string, object>
            {
                { "clients", sender }
            };

            IDictionary<string, object> metadata = mutatorManager.metadata.DeepMergedWith(clientMeta);

            mutatorManager.metadata = metadata;
            mutatorManager.OnMetadataChanged?.Invoke(metadata);
        }

        [PunRPC]
        public void SetScale(int viewId, float scale)
        {
            PhotonView view = PhotonView.Find(viewId);
            if (view != null)
            {
                view.transform.localScale = new Vector3(scale, scale, scale);
            }
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

        private bool ValidateClientMeta(IDictionary<string, object> clientMeta, IDictionary<string, IAllowedClientMeta> allowedMeta)
        {
            foreach (var kvp in clientMeta)
            {
                if (!allowedMeta.TryGetValue(kvp.Key, out var allowedDefinition))
                    return false;

                if (allowedDefinition.HasNested())
                {
                    if (kvp.Value is not IDictionary<string, object> nestedClientMeta)
                        return false;

                    if (!ValidateClientMeta(nestedClientMeta, allowedDefinition.NestedMeta))
                        return false;
                }
            }

            return true;
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
