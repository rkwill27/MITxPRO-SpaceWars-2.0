using UnityEngine;

namespace Scripts.UI
{
    /// <summary>
    /// This allows us to access the pause function in the PauseManager without having to add a reference to it.
    /// Not needed if we were to use the PauseManager directly,
    ///   but useful when wiring up the UI in a prefab that does not contain the PauseManager.
    /// </summary>
    public class PauseManagerProxy : MonoBehaviour
    {
        public static void TogglePause()
        {
            PauseManager.TogglePauseStatic();
        }
    }
}