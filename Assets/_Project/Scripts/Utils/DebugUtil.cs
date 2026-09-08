using System;
using System.Collections.Generic;
using _Project.Scripts.Units;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Utils {
    public class DebugUtil : MonoBehaviour {
        //[Header("Debug key bindings")] public KeyActionPair[] keyActions;

        private Dictionary<KeyCode, Action> debugCommands;
        
        public float damage = 50f;
        public string sayMessage = "Hello, World!";
        
        /*[Serializable]
        public class KeyActionPair {
            public KeyCode key;
            public UnityEvent action;
        }

        private void Update() {
            foreach (var pair in keyActions) {
                if (Input.GetKeyDown(pair.key)) {
                    LogUtil.Info("DebugUtil", "Update", $"Key {pair.key} pressed");
                    pair.action.Invoke();
                }
            }
        }

        public void DealDamage() {
            LogUtil.Info("DebugUtil", "DealDamage", "called");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                Unit unit = hit.collider.GetComponentInParent<Unit>();
                if (unit && !unit.IsDead) {
                    Vector3 dir = (unit.transform.position - hit.point).normalized;
                    unit.TakeDamage(damage, dir);
                }
            }
        }

        public void SaySomething() {
            LogUtil.Info("DebugUtil", "SaySomething", "called");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                Unit unit = hit.collider.GetComponentInParent<Unit>();
                if (unit && !unit.IsDead) {
                    Vector3 dir = (unit.transform.position - hit.point).normalized;
                    unit.SaySomething(sayMessage);
                }
            }
        }*/
    }
}