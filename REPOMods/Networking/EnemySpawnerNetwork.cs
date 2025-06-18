using OpJosModREPO.Controllers.IAmEnemy;
using OpJosModREPO.IAmEnemy.Util;
using Photon.Pun;
using UnityEngine;

namespace OpJosModREPO.IAmEnemy.Networking
{
    public class EnemySpawnerNetwork : MonoBehaviourPun
    {
        public static EnemySpawnerNetwork Instance;

        void Awake()
        {
            Instance = this;
        }

        [PunRPC]
        public void RPC_RequestSpawnEnemy(Vector3 position, int enemyTypeInt, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            GeneralUtil.SpawnEnemyAt(position, info.Sender.ActorNumber, (EnemyTypes)enemyTypeInt);
        }

        public void RequestSpawnEnemy(Vector3 position, EnemyTypes enemyType)
        {
            photonView.RPC("RPC_RequestSpawnEnemy", RpcTarget.MasterClient, position, (int)enemyType);
        }

        [PunRPC]
        public void RPC_SendEnemyMovement(Vector3 movement, Vector3 camForward, int actorNumber, bool jump, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            EnemyControllerBase control = GeneralUtil.FindEnemyController(actorNumber);
            if (control != null)
                control.UpdateMovementAndRotation(movement, camForward, jump);
        }

        public void SendEnemyMovement(Vector3 movement, Vector3 camForward, int actorNumber, bool jump)
        {
            photonView.RPC("RPC_SendEnemyMovement", RpcTarget.MasterClient, movement, camForward, actorNumber, jump);
        }

        [PunRPC]
        public void RPC_EnableDuckAI(int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            EnemyControllerBase control = GeneralUtil.FindEnemyController(actorNumber);
            control.isInBlendMode = true;
            GeneralUtil.EnableEnemyAI(control.thisEnemyEnemy);
        }

        public void EnableDuckAI(int actorNumber)
        {
            photonView.RPC("RPC_EnableDuckAI", RpcTarget.MasterClient, actorNumber);
        }

        [PunRPC]
        public void RPC_BreakDuckAI(int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            EnemyControllerBase control = GeneralUtil.FindEnemyController(actorNumber);
            control.isInBlendMode = false;
            GeneralUtil.BreakEnemyAI(control.thisEnemyEnemy);
        }

        public void BreakDuckAI(int actorNumber)
        {
            photonView.RPC("RPC_BreakDuckAI", RpcTarget.MasterClient, actorNumber);
        }

        [PunRPC]
        public void RPC_ControlDuck(Vector3 pos, int actorNumber)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber != actorNumber || PhotonNetwork.IsMasterClient)
                return;

            GeneralUtil.ControlClosestDuck(pos, actorNumber);
        }

        public void ControlDuck(Vector3 pos, int actorNumber)
        {
            photonView.RPC("RPC_ControlDuck", RpcTarget.All, pos, actorNumber);
        }
    }
}
