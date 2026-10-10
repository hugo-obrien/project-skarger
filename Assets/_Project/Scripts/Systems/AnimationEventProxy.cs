using System;
using _Project.Scripts.Units;
using _Project.Scripts.Units.Components;
using UnityEngine;

namespace _Project.Scripts.Systems
{
    public class AnimationEventProxy : MonoBehaviour
    {
        [SerializeField] private Unit unit;

        private void Awake()
        {
            if (unit == null)
            {
                unit = GetComponentInParent<Unit>();
            }
        }

        private void DispatchAnimationEvent(string eventName)
        {
            if (unit == null)
            {
                Debug.LogWarning("AnimationEventProxy.DispatchAnimationEvent(): unit not found");
                return;
            }
            
            unit.DispatchAnimationEvent(eventName);
        }
    }
}