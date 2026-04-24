using System.Collections;
using Prototypes.Alex.Interactables;
using UnityEngine;

namespace Prototypes.Alex
{
    public class PlayerFlagHoist : FlagHoister, IInteractable
    {
        private static readonly int TexturePropertyId = Shader.PropertyToID(k_TextureName);
        private const string k_TextureName = "_MainTex";
        
        private static FPSHandsController s_fpsHandsController;
        private static PlayerFlagInventory s_playerFlagInventory;

        [SerializeField, Header("Player Flag Hoist")]
        private AudioSource audioSource;
        private Coroutine m_audioCoroutine;
        
        [Header("Ropes")]
        [SerializeField]
        private LineRenderer ropeTargetRenderer;
        
        [SerializeField]
        private Transform handWheelTransform;
        [SerializeField]
        private float ropeTextureTargetXOffset = 1.0f;
        private Material m_ropeMaterial;
        
        [Header("Pulley")]

        [SerializeField]
        private float targetZRotation = 360.0f;

        protected override void Start()
        {
            base.Start();

            if (s_playerFlagInventory == null)
                s_playerFlagInventory = FindAnyObjectByType<PlayerFlagInventory>();

            if (s_fpsHandsController == null)
                s_fpsHandsController = FindAnyObjectByType<FPSHandsController>();

            m_ropeMaterial = ropeTargetRenderer.sharedMaterial;
        }
        public void OnInteract()
        {
            //FIXME Need an input cooldown here to prevent taking down flags prematurely
            HoistFlags(s_playerFlagInventory.holdingFlags);
            StartCoroutine(RopePuller());
            s_playerFlagInventory.DropAllFlags();

            s_fpsHandsController.SetState(FPSHandsController.HandState.Dropping);

            if (m_audioCoroutine != null)
                StopCoroutine(m_audioCoroutine);

            if (CurrentFlags == null || CurrentFlags.Count == 0)
                return;
            
            m_audioCoroutine = StartCoroutine(PlayAudio((CurrentFlags.Count > 0), 0.75f));
        }

        private IEnumerator RopePuller()
        {
            var startRotation = handWheelTransform.localEulerAngles;
            Vector2 startOffset = m_ropeMaterial.GetTextureOffset(TexturePropertyId);

            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                var dt = t / duration;
                
                float newX = Mathf.Lerp(startOffset.x, ropeTextureTargetXOffset, dt);
                float rotZ = Mathf.Lerp(startRotation.z, targetZRotation, dt);
                m_ropeMaterial.SetTextureOffset(TexturePropertyId, new Vector2(newX, startOffset.y));
                handWheelTransform.localEulerAngles = new Vector3(startRotation.x, startRotation.y, rotZ);
                yield return null; // Wait for next frame
            }

            handWheelTransform.localEulerAngles = startRotation;
            m_ropeMaterial.SetTextureOffset(TexturePropertyId, new Vector2(0.0f, startOffset.y));
        }

        private IEnumerator PlayAudio(bool state, float time)
        {
            var startVolume = state ? 0f : 1f;
            var endVolume = state ? 1f : 0f;

            if (state)
                audioSource.Play();

            audioSource.volume = startVolume;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, endVolume, t / time);

                yield return null;
            }

            audioSource.volume = endVolume;

            if (!state)
                audioSource.Pause();
        }
    }
}