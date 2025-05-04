using Mutators.Managers;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Mutators.Network
{
    internal class MutatorsNetworkManager : MonoBehaviour
    {
        internal static MutatorsNetworkManager Instance { get; private set; } = null!;

        private PhotonView _photonView = null!;

        void Start()
        {
            Instance = this;

            transform.parent = StatsManager.instance.transform;
            gameObject.name = "MutatorsNetworkManager";
            gameObject.hideFlags &= ~HideFlags.HideAndDontSave;
            _photonView = GetComponent<PhotonView>();
        }

        public void SendTestData(string data)
        {
            if (SemiFunc.IsMultiplayer())
            {
                if(SemiFunc.IsMasterClient())
                {
                    _photonView.RPC(nameof(Test), RpcTarget.Others, data);
                    Test(data);
                }
            }
            else
            {
                Test(data);
            }
            
        }

        public void SendActiveMutator(string name)
        {
            Send(name, SetActiveMutator, RpcTarget.OthersBuffered);
        }

        [PunRPC]
        public void Test(string data)
        {
            RepoMutators.Logger.LogInfo(data);
        }

        [PunRPC]
        public void SetActiveMutator(string name)
        {
            MutatorManager.Instance.SetActiveMutator(name, SemiFunc.RunIsLevel());
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
    }
}
