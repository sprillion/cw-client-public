using UnityEngine;

namespace character
{
    public struct EnemySnapshot
    {
        public int Tick;
        public Vector3 Position;
        public float RotationY;

        public EnemySnapshot(int tick, Vector3 position, float rotationY)
        {
            Tick = tick;
            Position = position;
            RotationY = rotationY;
        }
    }
}