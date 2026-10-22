using System;
using UnityEngine;

namespace _Project.Scripts.Systems
{
    public class GameTimeManager : MonoBehaviour
    {
        public static GameTimeManager Instance { get; private set; }

        private GameControls Controls => GameControlsProvider.Instance.Controls;

        [Header("Speed")]

        [SerializeField] private float[] timeScales = { 1f, 1.5f, 2f };

        private int currentSpeedIndex;
        
        public bool IsPaused { get; private set; }
        public float CurrentTimeScale => timeScales[currentSpeedIndex];

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (Controls.GameControl.Pause.WasPressedThisFrame())
            {
                TogglePause();
            }

            if (Controls.GameControl.SpeedUp.WasPressedThisFrame())
            {
                CycleSpeed();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Time.timeScale = 1f;
            }
        }

        private void TogglePause()
        {
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : CurrentTimeScale;
        }

        private void SetPaused(bool value)
        {
            IsPaused = value;
            Time.timeScale = IsPaused ? 0f : CurrentTimeScale;
        }

        private void CycleSpeed()
        {
            currentSpeedIndex = (currentSpeedIndex + 1) % timeScales.Length;
            if (!IsPaused)
            {
                Time.timeScale = CurrentTimeScale;
            }
        }
    }
}