using System;
using _Project.Scripts.Systems;
using _Project.Scripts.Units;
using _Project.Scripts.Units.Factions;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Dev {
    public class DevSpawner : MonoBehaviour {

        [Header("Spawn settings")] [SerializeField]
        private Unit unitPrefab;

        [SerializeField] [Tooltip("Spawn height offset")]
        private float spawnHeightOffset = 0.1f;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayer;

        private GameControls Controls => GameControlsProvider.Instance.Controls;

        private void Reset() {
            mainCamera = Camera.main;
        }

        private void Awake() {
            


            if (unitPrefab == null) {
                LogUtil.Error("DevSpawner", "Awake", "Unit prefab is null");
            }

            if (mainCamera == null) {
                mainCamera = Camera.main;
                if (mainCamera == null) {
                    LogUtil.Error("DevSpawner", "Awake", "MainCamera not found");
                }
            }
        }

        private void Update() {
            if (unitPrefab == null) {
                return;
            }

            if (Controls.DevTools.SpawnUser.WasPressedThisFrame()) {
                SpawnAtMouse(UnitFaction.User);
                return;
            }

            if (Controls.DevTools.SpawnAllied.WasPressedThisFrame()) {
                SpawnAtMouse(UnitFaction.Allied);
                return;
            }
            
            if (Controls.DevTools.SpawnNeutral.WasPressedThisFrame()) {
                SpawnAtMouse(UnitFaction.Neutral);
                return;
            }
            
            if (Controls.DevTools.SpawnEnemy.WasPressedThisFrame()) {
                SpawnAtMouse(UnitFaction.Enemy);
            }
        }

        public Unit SpawnUnit(UnitFaction faction, Vector3 position) {
            if (unitPrefab == null) {
                LogUtil.Error("DevSpawner", "SpawnUnit","Unable to spawn unit. Prefab is null");
                return null;
            }

            Unit spawnedUnit = Instantiate(unitPrefab, position, Quaternion.identity);
            spawnedUnit.SetFaction(faction);
            
            LogUtil.Info("DevSpawner", "SpawnUnit", faction + " unit spawned at " + position);
            return spawnedUnit;
        }

        private void SpawnAtMouse(UnitFaction faction) {
            if (mainCamera == null) {
                LogUtil.Info("DevSpawner", "SpawnAtMouse", "Camera not found");
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Controls.GameControl.PointerPosition.ReadValue<Vector2>());
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                Vector3 spawnPosition = hit.point + Vector3.up * spawnHeightOffset;
                SpawnUnit(faction, spawnPosition);
            } else {
                LogUtil.Warn("DevSpawner", "SpawnAtMouse", "Unable to find spawn location");
            }
        }
    }
}