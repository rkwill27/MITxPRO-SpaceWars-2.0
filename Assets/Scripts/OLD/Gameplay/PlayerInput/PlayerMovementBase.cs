using Scripts.UI;
using UnityEngine;

namespace Scripts.Gameplay.PlayerInput
{
    public class PlayerMovementBase : InputHandlerBase
    {
        protected Rigidbody2D cachedRigidbody2D;
 
        protected override void Start() 
        {
            base.Start();
            this.cachedRigidbody2D = this.GetComponent<Rigidbody2D>();
            this.cachedHitPoints = this.GetComponent<HitPoints>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            this.myPlayerInput.Player.Move.Enable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            this.myPlayerInput.Player.Move.Disable();
        }
    
        protected Vector2 GetMoveInput()
        {
            return this.ShouldProcessInput ? this.myPlayerInput.Player.Move.ReadValue<Vector2>() : Vector2.zero;
        }
    }
}