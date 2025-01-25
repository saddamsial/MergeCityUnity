using UnityEngine;
using UnityEngine.AI;

namespace Hiker.Idle
{
    public class UnitMovementBehavior : MonoBehaviour
    {
        protected NavMeshAgent agent;
        public bool ReachedDestination
        {
            get
            {
                if (!agent.pathPending)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public void Init()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.avoidancePriority = Random.Range(25, 100);
        }

        public void MoveTo(Vector3 movePosition)
        {
            agent.SetDestination(movePosition);
        }

        public void SetSpeed(float value)
        {
            agent.speed = value;
        }

        public void ToggleActive(bool isActive)
        {
            if (!isActive)
            {
                agent.isStopped = true;
            }
            agent.enabled = isActive;
            if (isActive)
            {
                agent.isStopped = false;
            }

        }
    }
}
