using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Prototypes.Alex.Utilities
{
    [CreateAssetMenu(fileName = "MaterialManager", menuName = "ScriptableObjects/Material Manager", order = 1)]
    public class SharedMaterialManager : ScriptableObject
    {
        private static SharedMaterialManager s_instance;
        [SerializeField]
        private Material flagMaterialBase;
        [SerializeField]
        private Material sailMaterialBase;
        
        [NonSerialized]
        private static Dictionary<FLAG, Material> s_flagMaterials;
        [NonSerialized]
        private static Dictionary<FLAG, Material> s_sailMaterials;

        [Button]
        private void OnEnable()
        {
            s_instance = this;
            s_flagMaterials = new Dictionary<FLAG, Material>();
            s_sailMaterials = new Dictionary<FLAG, Material>();
        }
        
        public static Material GetFlagMaterial(FLAG flag)
        {
            return GetMaterial(flag, ref s_instance.flagMaterialBase, ref s_flagMaterials);
        }
        
        public static Material GetSailMaterial(FLAG flag)
        {
            return GetMaterial(flag, ref s_instance.sailMaterialBase, ref s_sailMaterials);
        }
        
        private static Material GetMaterial(FLAG flag, ref Material baseMaterial, ref Dictionary<FLAG, Material> collection)
        {
            if (collection.TryGetValue(flag, out var materialInstance))
                return materialInstance;

            var newMaterial = new Material(baseMaterial)
            {
                name = $"{baseMaterial.name}_{flag.ToString().Replace('_','-')}_Instance",
                mainTexture = flag.GetTexture()
            };
            
            collection.Add(flag, newMaterial);
            return newMaterial;
        }

    }

    public static class MaterialExtensions
    {
        private static List<Material> s_materials;
        public static void SetSharedMaterial(this Renderer renderer, int index, Material material)
        {
            if(s_materials == null)
                s_materials = new List<Material>();
            
            s_materials.Clear();
            
            renderer.GetSharedMaterials(s_materials);
            s_materials[index] = material;
            renderer.SetSharedMaterials(s_materials);
        }
    }
}