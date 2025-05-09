using Mutators.Managers;
using Mutators.Mutators;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mutators.Network
{
    internal class MutatorsNetworkManager : MonoBehaviour
    {
        internal static MutatorsNetworkManager Instance { get; private set; } = null!;
        private IDictionary<string, Type> _networkedTypes = new Dictionary<string, Type>();

        private PhotonView _photonView = null!;

        void Start()
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

        public void SendBigMessage(string message, string emoji)
        {
            Send(message, emoji, ShowBigUIMessage, RpcTarget.All);
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
        public void Test(string data)
        {
            RepoMutators.Logger.LogInfo(data);
        }

        [PunRPC]
        public void ShowBigUIMessage(string message, string emoji)
        {
            SemiFunc.UIBigMessage(message, emoji, 25f, Color.white, Color.white);
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
    }
}
