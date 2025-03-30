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

        [PunRPC]
        public void RPC_SendDuckMovement(Vector3 movement, float mouseX, Vector3 duckPos, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            DuckPlayerController control = GeneralUtil.FindDuckController(GeneralUtil.FindClosestDuck(duckPos));
            if (control != null)
                control.UpdateMovementAndRotation(movement, mouseX);
        }

        public void SendDuckMovement(Vector3 movement, float mouseX, Vector3 duckPos)
        {
            photonView.RPC("RPC_SendDuckMovement", RpcTarget.MasterClient, movement, mouseX, duckPos);
        }
    }
}
