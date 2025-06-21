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
        

        protected override void OnEnable()
        {
            base.OnEnable();
            
            this.myPlayerInput.Player.Fire.Enable();
            this.myPlayerInput.Player.Fire.performed += this.SpawnPrefab;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            this.myPlayerInput.Player.Fire.Disable();
            myPlayerInput.Player.Fire.performed -= this.SpawnPrefab;
        }


        private void SpawnPrefab(InputAction.CallbackContext obj)
        {
            if (this.prefabSpawnMe == null) return;
            if (PauseManager.IsPaused) return;
            if (!this.ShouldProcessInput) return;
            if (IsGuiAction(obj)) return;

            this.spawnInfo.Spawn(this.transform, this.prefabSpawnMe.transform, this.spawnedObjectParent);
        }

    }
}