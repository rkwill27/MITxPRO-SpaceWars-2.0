using Scripts.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Scripts.UI
{
    public abstract class InputHandlerBase : MonoBehaviour
    {
        protected bool IsPointerOverUI { get; private set; }= false;
        public bool canActWhenDead = false;
        
        protected MyPlayerInput myPlayerInput;
        protected HitPoints cachedHitPoints;

        public virtual bool IsAlive => !this.cachedHitPoints || this.cachedHitPoints.IsAlive;
        public bool ShouldProcessInput => this.canActWhenDead || this.IsAlive;

        protected virtual void Start() 
        {
            this.cachedHitPoints = this.GetComponent<HitPoints>();
        }

        /// <summary>
        /// Enable this.myPlayerInput sections here.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (null != this.myPlayerInput) return;
            this.myPlayerInput = new MyPlayerInput();
        }

        /// <summary>
        /// Disable this.myPlayerInput sections here.
        /// </summary>
        protected virtual void OnDisable()
        {
        }
        
        protected virtual void Update()
        {
            // this variable should not be needed, but we put it in Update() to avoid a warning when IsPointerOverGameObject is called from an event handler.
            this.IsPointerOverUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        }


        protected bool IsGuiAction(InputAction.CallbackContext callbackContext)
        {
            return IsPointerOverUI && IsMouseClick(callbackContext);
        }
        
        protected static bool IsMouseClick(InputAction.CallbackContext callbackContext)
        {
            // Is this a mouse click?
            //  This is a hack, but they only way to filter out UGUI clicks that I've found so far.
            if (null == callbackContext.control?.device?.description) return false;
            
            return callbackContext.control.device.description.deviceClass == "Mouse";
        }
    }
}