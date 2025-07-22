using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mutators.Extensions;
using Mutators.Managers;
using Mutators.Mutators;
using Mutators.Settings;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Mutators.Network
{
    /// <summary>
    /// Network manager for sending Mutator-related RPCs.
    /// </summary>
    public class MutatorsNetworkManager : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// The singleton instance of the <see cref="MutatorsNetworkManager"/>.
        /// </summary>
        public static MutatorsNetworkManager Instance { get; private set; } = null!;

        private PhotonView _photonView = null!;

        // private readonly IDictionary<string, IAllowedClientMeta> allowedClientMeta = new Dictionary<string, IAllowedClientMeta>();

        private void Awake()
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

        /// <summary>
        /// Sends metadata to all clients. Buffered RPC.
        /// </summary>
        /// <remarks>
        /// The host consumes the metadata directly, without going through Photon.
        /// </remarks>
        /// <param name="mutatorNamespacedName">The unique identifier of the mutator to which the metadata belongs.</param>
        /// <param name="metadata">The metadata to send.</param>
        public void SendMetadata(string mutatorNamespacedName, IDictionary<string, object> metadata)
        {
            IDictionary<string, object> metaToSend = metadata.WithMutator(mutatorNamespacedName);
            Hashtable hashtable = metaToSend.ToPhotonHashtable();
            
            Send(
                nameof(SetMetadata),
                RpcTarget.OthersBuffered, 
                () => SetMetadataHostLocal(hashtable),
                hashtable
            );
        }

        internal void SendActiveMutator(string namespacedName, IDictionary<string, object>? metadata = null)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            Hashtable? hashtable = metadata?.ToPhotonHashtable();
            Send(
                nameof(SetActiveMutator),
                RpcTarget.OthersBuffered,
                () => SetActiveMutatorHostLocal(namespacedName, hashtable),
                namespacedName,
                hashtable!
            );
        }

        internal void SendActiveMutators(IMultiMutator multiMutator, IDictionary<string, object>? metadata = null)
        {
            Send(
                nameof(SetActiveMutators),
                RpcTarget.OthersBuffered,
                () => SetActiveMutatorsHost(multiMutator, metadata),
                multiMutator.SubMutators.Keys.Select(mutator => mutator.NamespacedName).ToArray(),
                metadata?.ToPhotonHashtable()!
            );
        }
        
        internal void SendComponentForViews(int[] views, Type componentType)
        {
            string fullName = componentType.FullName!;
            Send(
                nameof(AddComponentToViewGameObject),
                RpcTarget.OthersBuffered,
                () => AddComponentToViewGameObjectHostLocal(views, fullName),
                views,
                fullName
            );
        }

        /// <summary>
        /// Sends metadata to the host.
        /// </summary>
        /// <param name="namespacedName">The unique identifier of the mutator to which the metadata belongs.</param>
        /// <param name="meta">The metadata to send.</param>
        public void SendMetaToHost(string namespacedName, IDictionary<string, object> meta)
        {
            Hashtable data = meta.ToPhotonHashtable();
            if (SemiFunc.IsMultiplayer())
            {
                if (!SemiFunc.IsMasterClient())
                {
                    _photonView.RPC(nameof(SetClientMetadata), RpcTarget.MasterClient, namespacedName, data);
                }
            }
        }

        [PunRPC]
        private void AddComponentToViewGameObject(int[] views, string componentType, PhotonMessageInfo info = default)
        {
            if (!SemiFunc.MasterOnlyRPC(info))
            {
                return;
            }
            
            AddComponentToViewGameObjectHostLocal(views, componentType);
        }

        private static void AddComponentToViewGameObjectHostLocal(int[] views, string componentType)
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
        private void SetActiveMutator(string namespacedName, Hashtable? hashtable, PhotonMessageInfo info = default)
        {
            if (!SemiFunc.MasterOnlyRPC(info))
            {
                return;
            }
            
            SetActiveMutatorHostLocal(namespacedName, hashtable);
        }

        private static void SetActiveMutatorHostLocal(string namespacedName, Hashtable? hashtable)
        {
            MutatorManager mutatorManager = MutatorManager.Instance;
            bool runIsLevel = SemiFunc.RunIsLevel();

            RepoMutators.Logger.LogDebug($"Set mutator to {namespacedName}, applying patch {(runIsLevel ? "now" : "later")}");
            mutatorManager.SetActiveMutator(namespacedName, runIsLevel);

            IDictionary<string, object> metadata = hashtable == null ? new Dictionary<string, object>() : hashtable.FromPhotonHashtable();

            mutatorManager.CurrentMutator.ConsumeMetadata(metadata);
        }

        [PunRPC]
        private void SetActiveMutators(IList<string> names, Hashtable? hashtable, PhotonMessageInfo info = default) {
            if (!SemiFunc.MasterOnlyRPC(info))
            {
                return;
            }

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                RepoMutators.Logger.LogError("Received SetActiveMutators RPC as the host, but this is a client-only handler!");
                return;
            }

            MutatorManager mutatorManager = MutatorManager.Instance;
            bool runIsLevel = SemiFunc.RunIsLevel();

            if (names.Count == 0)
            {
                RepoMutators.Logger.LogError("Received empty list of active mutators");
                return;
            }

            if (names.Count > 0)
            {
                if (hashtable == null)
                {
                    RepoMutators.Logger.LogError("Critical failure during multi-mutator activation: no metadata hashtable found!");
                    return;
                }

                IDictionary<string, object> metadata = hashtable.FromPhotonHashtable();

                if (!SemiFunc.IsMasterClientOrSingleplayer())
                {
                    // Since we just transfer these over the network, the client shouldn't have them.
                    // And even if they do, they probably shouldn't use them...
                    IList<IMutator> mutators = mutatorManager.RegisteredMutators.Where(mutators => names.Contains(mutators.Key)).Select(x => x.Value).ToList();

                    IDictionary<string, object> overrides = metadata.Get<IDictionary<string, object>>(RepoMutators.MUTATOR_OVERRIDES)!;

                    IMutator mutator = new MultiMutator(
                        new MultiMutatorSettings(MyPluginInfo.PLUGIN_GUID, overrides.Get<string>("name") ?? "Name", overrides.Get<string>("description") ?? "Description"),
                        mutators.ToDictionary(k => k, IDictionary<string, object> (_) => new Dictionary<string, object>())
                    );

                    mutatorManager.SetActiveMutator(mutator, runIsLevel);
                    mutator.ConsumeMetadata(metadata);
                }
            }
        }

        private void SetActiveMutatorsHost(IMultiMutator mutator, IDictionary<string, object>? metadata)
        {
            if (metadata == null)
            {
                RepoMutators.Logger.LogError("Critical failure during multi-mutator activation: no metadata hashtable found!");
                return;
            }
            
            MutatorManager.Instance.SetActiveMutator(mutator, SemiFunc.RunIsLevel());
            mutator.ConsumeMetadata(metadata);
        }

        [PunRPC]
        private void SetMetadata(Hashtable hashtable, PhotonMessageInfo info = default)
        {
            if (!SemiFunc.MasterOnlyRPC(info))
            {
                return;
            }
            
            SetMetadataHostLocal(hashtable);
        }

        private static void SetMetadataHostLocal(Hashtable hashtable)
        {
            MutatorManager mutatorManager = MutatorManager.Instance;
            IDictionary<string, object> metadata = hashtable.FromPhotonHashtable();

            mutatorManager.CurrentMutator.ConsumeMetadata(metadata);
        }

        [PunRPC]
        private void SetClientMetadata(string namespacedName, Hashtable hashtable, PhotonMessageInfo info = default)
        {
            string? steamId = SemiFunc.PlayerAvatarGetFromPhotonPlayer(info.Sender)?.steamID;

            if (steamId == null)
            {
                RepoMutators.Logger.LogError("Received SetClientMetadata RPC with no steamID!");
                return;
            }

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

            mutatorManager.CurrentMutator.ConsumeMetadata(clientMeta.WithMutator(namespacedName));
        }

        [PunRPC]
        private void SendModVersion()
        {
            StartCoroutine(ComparedModVersionCoroutine());
        }

        [PunRPC]
        private void CompareModVersion(string version, Hashtable mutators, PhotonMessageInfo info)
        {
            if (version == MyPluginInfo.PLUGIN_VERSION)
            {
                RepoMutators.Logger.LogInfo($"{info.Sender.NickName} is on the same version!");
            }
            else
            {
                HandleVersionMismatch(version, info.Sender.NickName);
            }

            HashSet<string> mutatorNamespacedNames = MutatorManager.Instance.RegisteredMutators.Keys.ToHashSet();

            // Check if the client has all required mutators.
            foreach (string mutatorNamespacedName in mutatorNamespacedNames)
            {
                
            }
        }

        /// <summary>
        /// Photon callback when a player enters a room. Do not call this manually.
        /// </summary>
        /// <param name="newPlayer">The player that just joined.</param>
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

        // private bool ValidateClientMeta(IDictionary<string, object> clientMeta, IDictionary<string, IAllowedClientMeta> allowedMeta)
        // {
        //     foreach (var kvp in clientMeta)
        //     {
        //         if (!allowedMeta.TryGetValue(kvp.Key, out var allowedDefinition))
        //             return false;
        //
        //         if (allowedDefinition.HasNested())
        //         {
        //             if (kvp.Value is not IDictionary<string, object> nestedClientMeta)
        //                 return false;
        //
        //             if (!ValidateClientMeta(nestedClientMeta, allowedDefinition.NestedMeta))
        //                 return false;
        //         }
        //     }
        //
        //     return true;
        // }
        
        private void Send(string rpcMethod, RpcTarget rpcTarget, Action applyLocally, params object[] rpcArguments)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            if (SemiFunc.IsMultiplayer())
            {
                _photonView.RPC(rpcMethod, rpcTarget, rpcArguments);
            }

            applyLocally();
        }

        private IEnumerator ComparedModVersionCoroutine()
        {
            while (_photonView == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            IDictionary<string, object> mutatorNames = new Dictionary<string, object>();
            foreach (string namespacedName in MutatorManager.Instance.RegisteredMutators.Keys)
            {
                int index = namespacedName.IndexOf(':');
                if (index != -1)
                {
                    string[] fragments = namespacedName.Split(':');
                    string @namespace = fragments[0];
                    if (mutatorNames.TryGetValue(@namespace, out object? value) && value is List<string> list)
                    {
                        list.Add(fragments[1]);
                        continue;
                    }

                    mutatorNames[@namespace] = new List<string> { fragments[1] };
                }
            }

            _photonView.RPC(nameof(CompareModVersion), RpcTarget.MasterClient, MyPluginInfo.PLUGIN_VERSION, mutatorNames.ToPhotonHashtable());
        }

        /// <summary>
        /// Runs a coroutine.
        /// </summary>
        /// <param name="coroutine">The coroutine to start.</param>
        public void Run(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}
