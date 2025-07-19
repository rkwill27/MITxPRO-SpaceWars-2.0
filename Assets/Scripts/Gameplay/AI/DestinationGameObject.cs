using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class DestinationGameObject : DestinationProvider
    {
        public GameObject targetObject;
        public float targetPositionDeltaForDestinationUpdate = 0.1f;

        private bool hasEnteredCameraView = false;

        public override Vector2? ChooseNewDestinationInWorldSpace()
        {
            if (!hasEnteredCameraView)
            {
                // Move to the edge or center of camera view before tracking target
                Vector3 camCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
                return camCenter;
            }

            if (this.targetObject)
                return this.targetObject.transform.position;

            return null;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasEnteredCameraView)
            {
                if (IsInCameraView(this.transform.position))
                {
                    hasEnteredCameraView = true;
                }

                return; // Stop pathing logic until in view
            }

            if (this.targetObject == null) return;

            var needNewDestination = !this.CurrentDestination.HasValue ||
                Vector2.Distance(this.targetObject.transform.position, this.CurrentDestination.Value) > targetPositionDeltaForDestinationUpdate;

            if (needNewDestination)
            {
                this.CurrentDestination = this.targetObject.transform.position;
            }
        }

        private bool IsInCameraView(Vector3 position)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(position);
            return viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;
        }
    }
}
