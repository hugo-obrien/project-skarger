using System;
using UnityEngine;

namespace _Project.Scripts.Units {
    
    [Serializable]
    public class UnitStats {

        [Header("Movement")] 
        [Min(0.05f)] public float walkSpeed = 2.5f;
        [Min(0.05f)] public float runSpeed = 3.5f;
        [Min(0f)] public float runDistanceThreshold = 4.0f;
        [Min(0f)] public float acceleration = 16f;
        [Min(0f)] public float stoppingDistance = 0.1f;
        
        [Header("Turn")]
        [Min(0f)] public float turnSpeed = 14f;

        [Header("Pathfinding")]
        [Min(0.05f)]
        public float destinationSnapDistance = 1.0f;

        [Header("Health")] 
        [Min(1f)] public float maxHealth = 100f;

        [Header("Death")] [Min(0f)] 
        public float deathAnimationDuration = 1.5f;

    }
}