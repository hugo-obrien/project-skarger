using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Systems
{
    public class GameTimeManager : MonoBehaviour
    {
        public static GameTimeManager Instance { get; private set; }

        [Header("Time scales")]
        [SerializeField] private float[] timeScales = { 1f, 1.5f, 2f };

        private int currentSpeedIndex;

        private GameplayControls controls;
        
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

            controls = new GameplayControls();
        }

        private void OnEnable()
        {
            controls?.Gameplay.Enable();
        }

        private void OnDisable()
        {
            controls?.Gameplay.Disable();
        }

        private void Update()
        {
            if (controls.Gameplay.Pause.WasPressedThisFrame())
            {
                TogglePause();
            }

            if (controls.Gameplay.Speed.WasPressedThisFrame())
            {
                CycleSpeed();
            }
        }

        private void OnDestroy()
        {
            controls?.Dispose();

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