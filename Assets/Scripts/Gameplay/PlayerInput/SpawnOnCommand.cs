using Scripts.Helpers;
using Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Scripts.Gameplay.PlayerInput
{
    public class SpawnOnCommand : InputHandlerBase
    {
        public GameObject prefabSpawnMe;
        public Transform spawnedObjectParent;
        public SpawnInfo spawnInfo = new SpawnInfo();

        public float spawnInterval = 2f; // Seconds between spawns
        private float spawnTimer;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Commented out input-based spawning
            // this.myPlayerInput.Player.Fire.Enable();
            // this.myPlayerInput.Player.Fire.performed += this.SpawnPrefab;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Commented out input-based unbinding
            // this.myPlayerInput.Player.Fire.Disable();
            // myPlayerInput.Player.Fire.performed -= this.SpawnPrefab;
        }

        private void Update()
        {
            if (PauseManager.IsPaused || !this.ShouldProcessInput) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnPrefab();
                spawnTimer = spawnInterval;
            }
        }

        // Modified to be parameterless for timer use
        private void SpawnPrefab()
        {
            if (this.prefabSpawnMe == null) return;

            this.spawnInfo.Spawn(this.transform, this.prefabSpawnMe.transform, this.spawnedObjectParent);
        }

        // Original method left here for reference or reactivation later
        // private void SpawnPrefab(InputAction.CallbackContext obj)
        // {
        //     if (this.prefabSpawnMe == null) return;
        //     if (PauseManager.IsPaused) return;
        //     if (!this.ShouldProcessInput) return;
        //     if (IsGuiAction(obj)) return;

        //     this.spawnInfo.Spawn(this.transform, this.prefabSpawnMe.transform, this.spawnedObjectParent);
        // }
    }
}
