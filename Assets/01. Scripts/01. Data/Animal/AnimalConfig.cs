using UnityEngine;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "AnimalStats", menuName = "DungBeetle/AI/AnimalStats", order = 1)]
    public class AnimalConfig : ScriptableObject
    {
        [Header("Ability Settings")]
        public float maxHP = 100f;
        public float attackDamage = 10f;
        public float defense = 0f;

        [Header("AI Sensor Settings")]
        public SensorTargetData targetSetting;
        public LayerMask obstacleLayer;
        public float fovRange = 15f;
        [Range(0, 360)]
        public float fovAngle = 120f;
        public float fovHeight = 1.5f;
        public float soundRange = 10f;
        public float attackRange = 2f;
        public float stoppingDistance = 1.5f;
        [Header("Walkable AI Settings")]
        public LayerMask groundLayer;
        [Header("Flyable AI Settings")]
        public float minHeight = 100f;
        public float maxHeight = 100f;

        [Header("State Machine Settings")]
        [Header("Idle")]
        public float idleMinTime = 2f;
        public float idleMaxTime = 4f;

        [Header("Patrol Settings")]
        public float patrolMinRadius = 5f;
        public float patrolMaxRadius = 5f;

        [Header("Speed Settings")]
        public float walkSpeed = 1.5f;
        public float runSpeed = 3f;
        public float rotateSpeed = 30f;

        [Header("Attack Settings")]
        public bool friendly = false;
        public bool attackable = true;
        public float attackDelay = 0.5f;
        public float attackTimeout = 2f;

        [Header("Animal Things")]
        public float hungry = 50.0f;
        public float poo = 0f;
        public float energy = 80.0f;
    }
}