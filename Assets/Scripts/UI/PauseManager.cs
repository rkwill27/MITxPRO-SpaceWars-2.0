using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.UI
{
    public class PauseManager : InputHandlerBase
    {
        public static bool IsPaused { get; protected set; }= false;
        
        public GameObject pauseMenu;

        protected override void Start()
        {
            base.Start();
            
            if (!this.pauseMenu)
            {
                Debug.LogWarning($"Pause menu not assigned to PauseManager component on {this.gameObject.name}"); 
            }
            
            HandlePauseChange();
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
            HandlePauseChange();
        }
 
        public static void TogglePauseStatic()
        {
            var manager = FindObjectOfType<PauseManager>();

            if (!manager)
            {
                Debug.LogWarning("Could not find PauseManager object.");
                return;
            }

            manager.TogglePause();
        }

        
        public void Pause()
        {
            IsPaused = true;
            HandlePauseChange();
        }
        
        public void UnPause()
        {
            IsPaused = false;
            HandlePauseChange();
        }

        private void HandlePauseChange()
        {
            pauseMenu.SetActive(IsPaused);
            Time.timeScale = IsPaused ? 0 : 1;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            this.myPlayerInput.Meta.TogglePause.Enable();
            this.myPlayerInput.Meta.TogglePause.performed += OnTogglePauseUserInput;
        }

        private void OnTogglePauseUserInput(InputAction.CallbackContext obj)
        {
            this.TogglePause();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            this.myPlayerInput.Meta.TogglePause.Disable();
            this.myPlayerInput.Meta.TogglePause.performed -= OnTogglePauseUserInput;
        }
    }
}