using System;
using UnityEngine;

namespace _Project.Scripts.Units.Stats
{
    [Serializable]
    public class CombatStats
    {
        [Header("General")] 
        public bool isRanged = false;

        [Header("AI")] 
        public bool aiControlled = false;
        [Min(0.1f)] public float perceptionRadius = 15f;
        [Min(0.1f)] public float targetSearchInterval = 0.5f;
    }
}