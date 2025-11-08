using UnityEngine;

namespace Game.Flow
{
    public abstract class BoardBase : MonoBehaviour
    {
        public virtual void Init() {}
        public virtual void Enter() {}
        public virtual void Tick(float deltaTime) {}
        public virtual void Exit() {}
    }
}

