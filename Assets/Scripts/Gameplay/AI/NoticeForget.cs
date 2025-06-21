using UnityEngine;

namespace Scripts.Gameplay.AI
{
    public class NoticeForget : MonoBehaviour
    {
        public void OnNotice(float awareness)
        {
            Enable<DestinationGameObject>();
            Enable<SpawnPeriodically>();

            Disable<DestinationDrunkenWalk>();

            var constrainer = GetComponentInChildren<ConstrainToCircle>();
            if (constrainer)
            {
                constrainer.enabled = true;
                constrainer.radius = 0.5f;
            }
        }

        public void OnForget(float awareness)
        {
            Disable<DestinationGameObject>();
            Disable<SpawnPeriodically>();

            Enable<DestinationDrunkenWalk>();

            var constrainer = GetComponentInChildren<ConstrainToCircle>();
            if (constrainer)
            {
                constrainer.enabled = true;
                constrainer.radius = 0.0f;
            }
        }

        
        private void Enable<T>() where T :MonoBehaviour
        {
            T t = GetComponent<T>();
            if (t)
            {
                t.enabled = true;
            }
        }
        
        private void Disable<T>() where T :MonoBehaviour
        {
            T t = GetComponent<T>();
            if (t)
            {
                t.enabled = false;
            }
        }

    }
}