using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REPOMods
{
    public class EnemySpecs
    {
        public float MoveSpeed { get; set; } = 2.7f;
        public float TurnSpeed { get; set; } = 3f;
        public float JumpForce { get; set; } = 0.5f;
        public float AttackDelay { get; set; } = 2f;
        public bool FlyingEnemy { get; set; } = false;
        public bool MovingAnimationOverrides { get; set; } = false;
    }
}
