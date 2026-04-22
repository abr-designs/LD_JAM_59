using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Prototypes.Alex.Utilities
{
    [CreateAssetMenu(fileName = "MaterialManager", menuName = "ScriptableObjects/Material Manager", order = 1)]
    public class SharedMaterialManager : ScriptableObject
    {
#if UNITY_EDITOR
        private static SharedMaterialManager Instance => TryGetInstance();
        private static SharedMaterialManager s_instance;
#else
        private static SharedMaterialManager Instance;
#endif
        [SerializeField]
        private Material flagMaterialBase;
        [SerializeField]
        private Material sailMaterialBase;
        
        [NonSerialized]
        private static Dictionary<FLAG, Material> s_flagMaterials;
        [NonSerialized]
        private static Dictionary<FLAG, Material> s_sailMaterials;

#if !UNITY_EDITOR
        [Button]
        private void OnEnable()
        {
            Instance = this;
            s_flagMaterials = new Dictionary<FLAG, Material>();
            s_sailMaterials = new Dictionary<FLAG, Material>();
        }
#endif
        
        public static Material GetFlagMaterial(FLAG flag)
        {
            return GetMaterial(flag, ref Instance.flagMaterialBase, ref s_flagMaterials);
        }
        
        public static Material GetSailMaterial(FLAG flag)
        {
            return GetMaterial(flag, ref Instance.sailMaterialBase, ref s_sailMaterials);
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
        
#if UNITY_EDITOR
        private  static SharedMaterialManager TryGetInstance()
        {
            if(s_instance != null)
                return s_instance;
            
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(SharedMaterialManager)}");
            
            if(guids == null || guids.Length == 0)
                throw new Exception("No SharedMaterialManager found!");
            
            if(guids.Length > 1)
                throw new Exception("More than one SharedMaterialManager found!");
            
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            
            SharedMaterialManager found = UnityEditor.AssetDatabase.LoadAssetAtPath<SharedMaterialManager>(path);
            
            if(found == null)
                throw new Exception("Could not load SharedMaterialManager!");
            
            s_flagMaterials = new Dictionary<FLAG, Material>();
            s_sailMaterials = new Dictionary<FLAG, Material>();
            
            return found;
        }
#endif

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