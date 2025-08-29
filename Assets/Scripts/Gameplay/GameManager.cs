using Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Gameplay
{
    public class GameManager : InputHandlerBase
    {
        // Singleton pattern.
        public static GameManager Instance;

        public int score;

        // 🔹 Global flag for safe cleanup
        public static bool IsQuittingOrRestarting = false;

        void Start()
        {
            AudioManager.instance.PlayBGM();
        }
        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError($"GameMaster {Instance.name} already exists!  Deleting {this.name}");
                Destroy(gameObject);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.myPlayerInput.Meta.Enable();
            this.myPlayerInput.Meta.Restart.performed += ReloadScene;
        }

        private void ReloadScene(InputAction.CallbackContext obj)
        {
            // ✅ Before reloading, mark game as restarting
            IsQuittingOrRestarting = true;
            Helpers.GeneralHelpers.ReloadScene();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.myPlayerInput.Meta.Disable();
            this.myPlayerInput.Meta.Restart.performed -= ReloadScene;
        }

        private void OnApplicationQuit()
        {
            // ✅ Block pickups or spawns during quit
            IsQuittingOrRestarting = true;
        }

        private void OnDestroy()
        {
            if (Instance == this) // only main instance
                IsQuittingOrRestarting = true;
        }

        public void AddScore(int scoreValue)
        {
            // Why put this in a function?
            //  Maybe we want to do something fancy with the score.
            //  Send an event to the UI to show an animation, maybe?
            this.score += scoreValue;
        }
    }
}
