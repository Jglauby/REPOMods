﻿using Photon.Pun;
using REPOMods;
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
        public void RPC_SendDuckMovement(Vector3 movement, Vector3 camForward, int actorNumber, bool jump, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            DuckPlayerController control = GeneralUtil.FindDuckController(actorNumber);
            if (control != null)
                control.UpdateMovementAndRotation(movement, camForward, jump);
        }

        public void SendDuckMovement(Vector3 movement, Vector3 camForward, int actorNumber, bool jump)
        {
            photonView.RPC("RPC_SendDuckMovement", RpcTarget.MasterClient, movement, camForward, actorNumber, jump);
        }

        [PunRPC]
        public void RPC_ResetDuckControl(Vector3 loc, int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            GeneralUtil.ControlClosestDuck(loc, actorNumber);
        }

        public void ResetDuckControl(Vector3 loc, int actorNumber)
        {
            photonView.RPC("RPC_ResetDuckControl", RpcTarget.MasterClient, loc, actorNumber);
        }
    }
}
