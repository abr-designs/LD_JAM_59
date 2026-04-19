using Prototypes.Alex.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototypes.Alex
{
    public class PlayerFlagHoist : FlagHoister, IInteractable
    {
        private static PlayerFlagInventory s_playerFlagInventory;
        // public float duration = 5.0f;
        public float targetXOffset = 1.0f;
        public LineRenderer ropeTargetRenderer;
        public string textureName = "_MainTex";
        private Material mat;
        
        [SerializeField]
        private AudioSource audioSource;
        private Coroutine audioCoroutine;
        
        protected override void Start()
        {
            base.Start();

            if (s_playerFlagInventory == null)
                s_playerFlagInventory = FindAnyObjectByType<PlayerFlagInventory>();

            mat = ropeTargetRenderer.sharedMaterial;
        }
        public void OnInteract()
        {
            //FIXME Need an input cooldown here to prevent taking down flags prematurely
            HoistFlags(s_playerFlagInventory.holdingFlags);
            StartCoroutine(RopePuller());
            s_playerFlagInventory.DropAllFlags();
            
            if(audioCoroutine != null)
                StopCoroutine(audioCoroutine);
            
            audioCoroutine = StartCoroutine(PlayAudio((CurrentFlags.Count > 0), 0.75f));
        }

        IEnumerator RopePuller()
        {
            float elapsed = 0f;

            Vector2 startOffset = mat.GetTextureOffset(textureName);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newX = Mathf.Lerp(startOffset.x, targetXOffset, elapsed / duration);
                mat.SetTextureOffset(textureName, new Vector2(newX, startOffset.y));
                yield return null; // Wait for next frame
            }

            mat.SetTextureOffset(textureName, new Vector2(0.0f, startOffset.y));
        }

        private IEnumerator PlayAudio(bool state, float time)
        {
            var startVolume = state ? 0f : 1f;
            var endVolume = state ? 1f : 0f;
            
            if(state)
                audioSource.Play();
            
            audioSource.volume = startVolume;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, endVolume, t / time);
                
                yield return null;
            }
            
            audioSource.volume = endVolume;
            
            if(!state)
                audioSource.Pause();
        }
    }
}