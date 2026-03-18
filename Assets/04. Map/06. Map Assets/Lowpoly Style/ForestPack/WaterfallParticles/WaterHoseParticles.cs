using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityStandardAssets.Effects
{
    public class WaterHoseParticles : MonoBehaviour
    {
        public static float lastSoundTime;
        public float force = 1;

        private List<ParticleCollisionEvent> m_CollisionEvents;
        private ParticleSystem m_ParticleSystem;

        private void Start()
        {
            m_ParticleSystem = GetComponent<ParticleSystem>();
            m_CollisionEvents = new List<ParticleCollisionEvent>();
        }

        private void OnParticleCollision(GameObject other)
        {
            m_CollisionEvents.Clear();
            int numCollisionEvents = m_ParticleSystem.GetCollisionEvents(other, m_CollisionEvents);

            for (int i = 0; i < numCollisionEvents; i++)
            {
                if (Time.time > lastSoundTime + 0.2f)
                {
                    lastSoundTime = Time.time;
                }

                var col = m_CollisionEvents[i].colliderComponent;
                var attachedRigidbody = col.GetComponent<Rigidbody>();
                if (attachedRigidbody != null)
                {
                    Vector3 vel = m_CollisionEvents[i].velocity;
                    attachedRigidbody.AddForce(vel * force, ForceMode.Impulse);
                }

                other.BroadcastMessage("Extinguish", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
