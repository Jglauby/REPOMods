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
        public void RPC_EnableEnemyAI(int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            EnemyControllerBase control = GeneralUtil.FindEnemyController(actorNumber);
            control.isInBlendMode = true;
            GeneralUtil.EnableEnemyAI(control.thisEnemyEnemy);
        }

        public void EnableEnemyAI(int actorNumber)
        {
            photonView.RPC("RPC_EnableEnemyAI", RpcTarget.MasterClient, actorNumber);
        }

        [PunRPC]
        public void RPC_BreakEnemyAI(int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            EnemyControllerBase control = GeneralUtil.FindEnemyController(actorNumber);
            control.isInBlendMode = false;
            GeneralUtil.BreakEnemyAI(control.thisEnemyEnemy);
        }

        public void BreakEnemyAI(int actorNumber)
        {
            photonView.RPC("RPC_BreakEnemyAI", RpcTarget.MasterClient, actorNumber);
        }

        [PunRPC]
        public void RPC_ControlEnemy(Vector3 pos, int actorNumber, int enemyTypeId)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber != actorNumber || PhotonNetwork.IsMasterClient)
                return;

            GeneralUtil.ControlClosestEnemy(pos, actorNumber, (EnemyTypes)enemyTypeId);
        }

        public void ControlEnemy(Vector3 pos, int actorNumber, EnemyTypes enemyType)
        {
            photonView.RPC("RPC_ControlEnemy", RpcTarget.All, pos, actorNumber, (int)enemyType);
        }

        [PunRPC]
        public void RPC_TriggerSpecialAttack(Vector3 pos, Vector3 rot, int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            EnemyControllerBase control = GeneralUtil.FindEnemyController(actorNumber);
            control.SpecialAttack(pos, rot);
        }

        public void TriggerSpecialAttack(Vector3 pos, Vector3 rot, int actorNumber)
        {
            photonView.RPC("RPC_TriggerSpecialAttack", RpcTarget.MasterClient, pos, rot, actorNumber);
        }

        [PunRPC]
        public void RPC_TriggerSpecialMovement(int num, int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            EnemyControllerBase control = GeneralUtil.FindEnemyController(actorNumber);
            control.SpecialMovement(num);
        }

        public void TriggerSpecialMovement(int num, int actorNumber)
        {
            photonView.RPC("RPC_TriggerSpecialMovement", RpcTarget.MasterClient, num, actorNumber);
        }
    }
}
