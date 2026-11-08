using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StillSky.CameraSystem
{
    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson
    }

    /// <summary>
    /// Toggles between the first-person and third-person Cinemachine cameras,
    /// and swaps which player meshes the output camera renders.
    /// </summary>
    public class CameraModeSwitcher : MonoBehaviour
    {
        [Header("Cinemachine Cameras")]
        [SerializeField] private CinemachineCamera firstPersonCam;
        [SerializeField] private CinemachineCamera thirdPersonCam;

        [Header("Rendering")]
        [SerializeField] private Camera outputCamera;   // the Main Camera with the CinemachineBrain
        [SerializeField] private LayerMask bodyLayer;   // full-body mesh (third-person only)
        [SerializeField] private LayerMask armsLayer;   // first-person arms + weapon

        [Header("Input")]
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private CameraMode startMode = CameraMode.ThirdPerson;

        public CameraMode Mode { get; private set; }
        public event Action<CameraMode> ModeChanged;

        private void OnEnable()
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnToggle;
        }

        private void OnDisable()
        {
            toggleAction.action.performed -= OnToggle;
            toggleAction.action.Disable();
        }

        private void Start()
        {
            SetMode(startMode);
        }

        private void OnToggle(InputAction.CallbackContext _)
        {
            SetMode(Mode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson);
        }

        public void SetMode(CameraMode mode)
        {
            Mode = mode;
            bool fp = mode == CameraMode.FirstPerson;

            // Only one camera active -> the CinemachineBrain blends to it.
            firstPersonCam.gameObject.SetActive(fp);
            thirdPersonCam.gameObject.SetActive(!fp);

            int mask = outputCamera.cullingMask;
            mask = fp
                ? (mask & ~bodyLayer.value) | armsLayer.value   // hide body, show arms
                : (mask | bodyLayer.value) & ~armsLayer.value;  // show body, hide arms
            outputCamera.cullingMask = mask;

            ModeChanged?.Invoke(mode);
        }
    }
}