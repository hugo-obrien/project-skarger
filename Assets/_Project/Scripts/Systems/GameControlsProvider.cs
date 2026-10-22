using UnityEngine;

namespace _Project.Scripts.Systems
{
    /// <summary>
    /// Owns the single <see cref="GameControls"/> instance shared by all input consumers.
    /// Created automatically on first access and survives scene loads.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameControlsProvider : MonoBehaviour
    {
        private static GameControlsProvider instance;

        public static GameControlsProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(nameof(GameControlsProvider)).AddComponent<GameControlsProvider>();
                }

                return instance;
            }
        }

        public GameControls Controls { get; private set; }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Controls = new GameControls();
            Controls.Enable();
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            instance = null;
            Controls?.Disable();
            Controls?.Dispose();
        }
    }
}