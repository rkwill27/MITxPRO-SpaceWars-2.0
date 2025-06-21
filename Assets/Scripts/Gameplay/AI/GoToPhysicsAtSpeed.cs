using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class GoToPhysicsAtSpeed : GoToBase
    {
        public float maxAcceleration = 10f;
        public RangeF rSpeed = RangeF.Range01;

        protected float speedThisTime;

        public override void OnDestinationChanged(Vector2? newDestination)
        {
            base.OnDestinationChanged(newDestination);

            this.speedThisTime = this.rSpeed.RandomValue();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (!this.cachedRigidbody2D) return;
           if (!this.DestinationProvider) return;
           if (!this.DestinationProvider.CurrentDestination.HasValue) return;
            
            Vector2 toDestination = this.DestinationProvider.CurrentDestination.Value - (Vector2) transform.position;
            var desiredVelocity = DestinationProvider.IsCloseEnoughToDestination ? Vector2.zero : toDestination.normalized * this.speedThisTime;

            Vector2 desiredDeltaV = desiredVelocity - this.cachedRigidbody2D.velocity;
            Vector2 desiredAcceleration = desiredDeltaV / Time.fixedDeltaTime;
            
            // Cap acceleration.
            var acceleration = Mathf.Min(desiredAcceleration.magnitude, this.maxAcceleration);
            
            this.cachedRigidbody2D.AddForce(desiredDeltaV.normalized * (acceleration * this.cachedRigidbody2D.mass), ForceMode2D.Force);
        }
    }
}