using Photon.Pun;
using UnityEngine;

namespace OpJosModREPO.IAmDucky.Networking
{
    public class DuckSpawnerNetwork : MonoBehaviourPun
    {
        public static DuckSpawnerNetwork Instance;

        void Awake()
        {
            Instance = this;
        }

        [PunRPC]
        public void RPC_RequestSpawnDuck(Vector3 position, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            GeneralUtil.SpawnDuckAt(position, info.Sender.ActorNumber);
        }

        public void RequestDuckSpawn(Vector3 position)
        {
            photonView.RPC("RPC_RequestSpawnDuck", RpcTarget.MasterClient, position);
        }
    }
}
