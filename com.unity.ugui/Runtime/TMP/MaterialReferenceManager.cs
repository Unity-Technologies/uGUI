using System.Collections.Generic;
using UnityEngine;
using System;


namespace TMPro
{

    public class MaterialReferenceManager
    {
        private static MaterialReferenceManager s_Instance;

        // Dictionaries used to track Asset references.
        private Dictionary<int, Material> m_FontMaterialReferenceLookup = new Dictionary<int, Material>();
        private Dictionary<int, TMP_FontAsset> m_FontAssetReferenceLookup = new Dictionary<int, TMP_FontAsset>();
        private Dictionary<int, TMP_SpriteAsset> m_SpriteAssetReferenceLookup = new Dictionary<int, TMP_SpriteAsset>();
        private Dictionary<int, TMP_ColorGradient> m_ColorGradientReferenceLookup = new Dictionary<int, TMP_ColorGradient>();


        /// <summary>
        /// Get a singleton instance of the registry
        /// </summary>
        public static MaterialReferenceManager instance
        {
            get
            {
                if (MaterialReferenceManager.s_Instance == null)
                    MaterialReferenceManager.s_Instance = new MaterialReferenceManager();
                return MaterialReferenceManager.s_Instance;
            }
        }



        /// <summary>
        /// Add new font asset reference to dictionary.
        /// </summary>
        /// <remarks>
        /// Registers the font asset and its material with the singleton so TextMesh Pro
        /// can reuse them across text objects. Duplicate hash codes are ignored. Call
        /// when loading or creating font assets at runtime.
        /// </remarks>
        /// <param name="fontAsset">The font asset to register with the manager.</param>
        /// <example>
        /// <para>Register a font asset after loading it so it is tracked for material
        /// and mesh updates. Typically called from font asset initialization.</para>
        /// <code><![CDATA[
        /// TMP_FontAsset myFont = Resources.Load<TMP_FontAsset>("MyFont");
        /// MaterialReferenceManager.AddFontAsset(myFont);
        /// ]]></code>
        /// </example>
        public static void AddFontAsset(TMP_FontAsset fontAsset)
        {
            MaterialReferenceManager.instance.AddFontAssetInternal(fontAsset);
        }

        /// <summary>
        ///  Add new Font Asset reference to dictionary.
        /// </summary>
        /// <param name="fontAsset"></param>
        private void AddFontAssetInternal(TMP_FontAsset fontAsset)
        {
            if (m_FontAssetReferenceLookup.ContainsKey(fontAsset.hashCode)) return;

            // Add reference to the font asset.
            m_FontAssetReferenceLookup.Add(fontAsset.hashCode, fontAsset);

            // Add reference to the font material.
            m_FontMaterialReferenceLookup.Add(fontAsset.materialHashCode, fontAsset.material);
        }



        /// <summary>
        /// Add new Sprite Asset to dictionary.
        /// </summary>
        /// <remarks>
        /// Registers the sprite asset and its material with the singleton so inline
        /// sprites in text can resolve references. Duplicate hash codes are ignored.
        /// Call when loading or creating sprite assets at runtime.
        /// </remarks>
        /// <param name="spriteAsset">The sprite asset to register with the manager.</param>
        /// <example>
        /// <para>Register a sprite asset so it can be used in rich text (e.g. sprite
        /// tags). Typically called when the asset is loaded or created.</para>
        /// <code><![CDATA[
        /// TMP_SpriteAsset emojiAsset = Resources.Load<TMP_SpriteAsset>("EmojiSprites");
        /// MaterialReferenceManager.AddSpriteAsset(emojiAsset);
        /// ]]></code>
        /// </example>
        public static void AddSpriteAsset(TMP_SpriteAsset spriteAsset)
        {
            MaterialReferenceManager.instance.AddSpriteAssetInternal(spriteAsset);
        }

        /// <summary>
        /// Internal method to add a new sprite asset to the dictionary.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="spriteAsset"></param>
        private void AddSpriteAssetInternal(TMP_SpriteAsset spriteAsset)
        {
            if (m_SpriteAssetReferenceLookup.ContainsKey(spriteAsset.hashCode)) return;

            // Add reference to sprite asset.
            m_SpriteAssetReferenceLookup.Add(spriteAsset.hashCode, spriteAsset);

            // Adding reference to the sprite asset material as well
            m_FontMaterialReferenceLookup.Add(spriteAsset.hashCode, spriteAsset.material);
        }

        /// <summary>
        /// Add new Sprite Asset to dictionary.
        /// </summary>
        /// <remarks>
        /// Registers the sprite asset under the given hash code so it can be looked up
        /// by that code. Use when the asset's default hashCode is not suitable (e.g.
        /// custom atlases). Duplicate hash codes overwrite the previous entry.
        /// </remarks>
        /// <param name="hashCode">The hash code to use when looking up the sprite asset.</param>
        /// <param name="spriteAsset">The sprite asset to register with the manager.</param>
        /// <example>
        /// <para>Register a sprite asset with a specific hash for custom lookup. Use
        /// the same hashCode when resolving sprites in rich text.</para>
        /// <code><![CDATA[
        /// int customHash = GetCustomSpriteHash(mySpriteAsset);
        /// MaterialReferenceManager.AddSpriteAsset(customHash, mySpriteAsset);
        /// ]]></code>
        /// </example>
        public static void AddSpriteAsset(int hashCode, TMP_SpriteAsset spriteAsset)
        {
            MaterialReferenceManager.instance.AddSpriteAssetInternal(hashCode, spriteAsset);
        }

        /// <summary>
        /// Internal method to add a new sprite asset to the dictionary.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="spriteAsset"></param>
        private void AddSpriteAssetInternal(int hashCode, TMP_SpriteAsset spriteAsset)
        {
            if (m_SpriteAssetReferenceLookup.ContainsKey(hashCode)) return;

            // Add reference to Sprite Asset.
            m_SpriteAssetReferenceLookup.Add(hashCode, spriteAsset);

            // Add reference to Sprite Asset using the asset hashcode.
            m_FontMaterialReferenceLookup.Add(hashCode, spriteAsset.material);

            // Compatibility check
            if (spriteAsset.hashCode == 0)
                spriteAsset.hashCode = hashCode;
        }


        /// <summary>
        /// Add new Material reference to dictionary.
        /// </summary>
        /// <remarks>
        /// Registers a font material by hash code so TextMesh Pro can look it up when
        /// building meshes. Use the same hashCode as the font asset's material for
        /// correct reference counting. Duplicate hash codes overwrite the previous entry.
        /// </remarks>
        /// <param name="hashCode">The hash code used to look up the material.</param>
        /// <param name="material">The material instance to register with the manager.</param>
        /// <example>
        /// <para>Register a material by hash when you need to track it separately from
        /// a font asset. Ensure the hashCode matches the material instance.</para>
        /// <code><![CDATA[
        /// int matHash = myMaterial.GetHashCode();
        /// MaterialReferenceManager.AddFontMaterial(matHash, myMaterial);
        /// ]]></code>
        /// </example>
        public static void AddFontMaterial(int hashCode, Material material)
        {
            MaterialReferenceManager.instance.AddFontMaterialInternal(hashCode, material);
        }

        /// <summary>
        /// Add new material reference to dictionary.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="material"></param>
        private void AddFontMaterialInternal(int hashCode, Material material)
        {
            // Since this function is called after checking if the material is
            // contained in the dictionary, there is no need to check again.
            m_FontMaterialReferenceLookup.Add(hashCode, material);
        }


        /// <summary>
        /// Add new Color Gradient Preset to dictionary.
        /// </summary>
        /// <remarks>
        /// Registers a color gradient preset by hash code so it can be referenced in
        /// rich text (e.g. &lt;gradient&gt; tags). Duplicate hash codes are ignored.
        /// Call when loading or creating gradient presets at runtime.
        /// </remarks>
        /// <param name="hashCode">The hash code used to look up the gradient preset.</param>
        /// <param name="colorGradient">The color gradient preset to register with the manager.</param>
        /// <example>
        /// <para>Register a gradient preset so it can be used in text. Use a stable
        /// hashCode (e.g. from the preset name or asset) for lookups.</para>
        /// <code><![CDATA[
        /// int gradientHash = myGradientAsset.GetHashCode();
        /// MaterialReferenceManager.AddColorGradientPreset(gradientHash, myGradientAsset);
        /// ]]></code>
        /// </example>
        public static void AddColorGradientPreset(int hashCode, TMP_ColorGradient colorGradient)
        {
            MaterialReferenceManager.instance.AddColorGradientPreset_Internal(hashCode, colorGradient);
        }

        /// <summary>
        /// Internal method to add a new Color Gradient Preset to the dictionary.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="colorGradient"></param>
        private void AddColorGradientPreset_Internal(int hashCode, TMP_ColorGradient colorGradient)
        {
            if (m_ColorGradientReferenceLookup.ContainsKey(hashCode)) return;

            // Add reference to Color Gradient Preset Asset.
            m_ColorGradientReferenceLookup.Add(hashCode, colorGradient);
        }



        /// <summary>
        /// Add new material reference and return the index of this new reference in the materialReferences array.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="materialHashCode"></param>
        /// <param name="fontAsset"></param>
        //public int AddMaterial(Material material, int materialHashCode, TMP_FontAsset fontAsset)
        //{
        //    if (!m_MaterialReferenceLookup.ContainsKey(materialHashCode))
        //    {
        //        int index = m_MaterialReferenceLookup.Count;

        //        materialReferences[index].fontAsset = fontAsset;
        //        materialReferences[index].material = material;
        //        materialReferences[index].isDefaultMaterial = material.GetEntityId() == fontAsset.material.GetEntityId() ? true : false;
        //        materialReferences[index].index = index;
        //        materialReferences[index].referenceCount = 0;

        //        m_MaterialReferenceLookup[materialHashCode] = index;

        //        // Compute Padding value and store it
        //        // TODO

        //        int fontAssetHashCode = fontAsset.hashCode;

        //        if (!m_FontAssetReferenceLookup.ContainsKey(fontAssetHashCode))
        //            m_FontAssetReferenceLookup.Add(fontAssetHashCode, fontAsset);

        //        m_countInternal += 1;

        //        return index;
        //    }
        //    else
        //    {
        //        return m_MaterialReferenceLookup[materialHashCode];
        //    }
        //}


        /// <summary>
        /// Add new material reference and return the index of this new reference in the materialReferences array.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="materialHashCode"></param>
        /// <param name="spriteAsset"></param>
        /// <returns></returns>
        //public int AddMaterial(Material material, int materialHashCode, TMP_SpriteAsset spriteAsset)
        //{
        //    if (!m_MaterialReferenceLookup.ContainsKey(materialHashCode))
        //    {
        //        int index = m_MaterialReferenceLookup.Count;

        //        materialReferences[index].fontAsset = materialReferences[0].fontAsset;
        //        materialReferences[index].spriteAsset = spriteAsset;
        //        materialReferences[index].material = material;
        //        materialReferences[index].isDefaultMaterial = true;
        //        materialReferences[index].index = index;
        //        materialReferences[index].referenceCount = 0;

        //        m_MaterialReferenceLookup[materialHashCode] = index;

        //        int spriteAssetHashCode =  spriteAsset.hashCode;

        //        if (!m_SpriteAssetReferenceLookup.ContainsKey(spriteAssetHashCode))
        //            m_SpriteAssetReferenceLookup.Add(spriteAssetHashCode, spriteAsset);

        //        m_countInternal += 1;

        //        return index;
        //    }
        //    else
        //    {
        //        return m_MaterialReferenceLookup[materialHashCode];
        //    }
        //}


        /// <summary>
        /// Checks whether the font asset is referenced.
        /// </summary>
        /// <param name="font">The font asset to check for in the manager.</param>
        /// <returns>True if the font asset is already registered in the manager.</returns>
        public bool Contains(TMP_FontAsset font)
        {
            return m_FontAssetReferenceLookup.ContainsKey(font.hashCode);
        }


        /// <summary>
        /// Checks whether the sprite asset is referenced.
        /// </summary>
        /// <param name="sprite">The sprite asset to check for in the manager.</param>
        /// <returns>True if the sprite asset is already registered in the manager.</returns>
        public bool Contains(TMP_SpriteAsset sprite)
        {
            return m_FontAssetReferenceLookup.ContainsKey(sprite.hashCode);
        }



        /// <summary>
        /// Gets the font asset for the given hash code, if registered.
        /// </summary>
        /// <param name="hashCode">The hash code of the font asset to look up.</param>
        /// <param name="fontAsset">The font asset if found, or null otherwise.</param>
        /// <returns>True if the font asset was found; otherwise, false.</returns>
        public static bool TryGetFontAsset(int hashCode, out TMP_FontAsset fontAsset)
        {
            return MaterialReferenceManager.instance.TryGetFontAssetInternal(hashCode, out fontAsset);
        }

        /// <summary>
        /// Returns the font asset for the given hash code from the internal lookup.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        private bool TryGetFontAssetInternal(int hashCode, out TMP_FontAsset fontAsset)
        {
            fontAsset = null;

            return m_FontAssetReferenceLookup.TryGetValue(hashCode, out fontAsset);
        }



        /// <summary>
        /// Gets the sprite asset for the given hash code, if registered.
        /// </summary>
        /// <param name="hashCode">The hash code of the sprite asset to look up.</param>
        /// <param name="spriteAsset">The sprite asset if found, or null otherwise.</param>
        /// <returns>True if the sprite asset was found; otherwise, false.</returns>
        public static bool TryGetSpriteAsset(int hashCode, out TMP_SpriteAsset spriteAsset)
        {
            return MaterialReferenceManager.instance.TryGetSpriteAssetInternal(hashCode, out spriteAsset);
        }

        /// <summary>
        /// Returns the sprite asset for the given hash code from the internal lookup.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        private bool TryGetSpriteAssetInternal(int hashCode, out TMP_SpriteAsset spriteAsset)
        {
            spriteAsset = null;

            return m_SpriteAssetReferenceLookup.TryGetValue(hashCode, out spriteAsset);
        }


        /// <summary>
        /// Gets the color gradient preset for the given hash code, if registered.
        /// </summary>
        /// <param name="hashCode">The hash code of the gradient preset to look up.</param>
        /// <param name="gradientPreset">The gradient preset if found, or null otherwise.</param>
        /// <returns>True if the gradient preset was found; otherwise, false.</returns>
        public static bool TryGetColorGradientPreset(int hashCode, out TMP_ColorGradient gradientPreset)
        {
            return MaterialReferenceManager.instance.TryGetColorGradientPresetInternal(hashCode, out gradientPreset);
        }

        /// <summary>
        /// Returns the color gradient preset for the given hash code from the internal lookup.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        private bool TryGetColorGradientPresetInternal(int hashCode, out TMP_ColorGradient gradientPreset)
        {
            gradientPreset = null;

            return m_ColorGradientReferenceLookup.TryGetValue(hashCode, out gradientPreset);
        }


        /// <summary>
        /// Gets the font material for the given hash code, if registered.
        /// </summary>
        /// <param name="hashCode">The hash code of the material to look up.</param>
        /// <param name="material">The material if found, or null otherwise.</param>
        /// <returns>True if the material was found; otherwise, false.</returns>
        public static bool TryGetMaterial(int hashCode, out Material material)
        {
            return MaterialReferenceManager.instance.TryGetMaterialInternal(hashCode, out material);
        }

        /// <summary>
        /// Returns the font material for the given hash code from the internal lookup.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        private bool TryGetMaterialInternal(int hashCode, out Material material)
        {
            material = null;

            return m_FontMaterialReferenceLookup.TryGetValue(hashCode, out material);
        }


        /// <summary>
        /// Function to lookup a material based on hash code and returning the MaterialReference containing this material.
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        //public bool TryGetMaterial(int hashCode, out MaterialReference materialReference)
        //{
        //    int materialIndex = -1;

        //    if (m_MaterialReferenceLookup.TryGetValue(hashCode, out materialIndex))
        //    {
        //        materialReference = materialReferences[materialIndex];

        //        return true;
        //    }

        //    materialReference = new MaterialReference();

        //    return false;
        //}



        /// <summary>
        ///
        /// </summary>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        //public int GetMaterialIndex(TMP_FontAsset fontAsset)
        //{
        //    if (m_MaterialReferenceLookup.ContainsKey(fontAsset.materialHashCode))
        //        return m_MaterialReferenceLookup[fontAsset.materialHashCode];

        //    return -1;
        //}


        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        //public TMP_FontAsset GetFontAsset(int index)
        //{
        //    if (index >= 0  && index < materialReferences.Length)
        //        return materialReferences[index].fontAsset;

        //    return null;
        //}


        /// <summary>
        ///
        /// </summary>
        /// <param name="material"></param>
        /// <param name="materialHashCode"></param>
        /// <param name="fontAsset"></param>
        //public void SetDefaultMaterial(Material material, int materialHashCode, TMP_FontAsset fontAsset)
        //{
        //    if (!m_MaterialReferenceLookup.ContainsKey(materialHashCode))
        //    {
        //        materialReferences[0].fontAsset = fontAsset;
        //        materialReferences[0].material = material;
        //        materialReferences[0].index = 0;
        //        materialReferences[0].isDefaultMaterial = material.GetEntityId() == fontAsset.material.GetEntityId() ? true : false;
        //        materialReferences[0].referenceCount = 0;
        //        m_MaterialReferenceLookup[materialHashCode] = 0;

        //        // Compute Padding value and store it
        //        // TODO

        //        int fontHashCode = fontAsset.hashCode;

        //        if (!m_FontAssetReferenceLookup.ContainsKey(fontHashCode))
        //            m_FontAssetReferenceLookup.Add(fontHashCode, fontAsset);
        //    }
        //    else
        //    {
        //        materialReferences[0].fontAsset = fontAsset;
        //        materialReferences[0].material = material;
        //        materialReferences[0].index = 0;
        //        materialReferences[0].referenceCount = 0;
        //        m_MaterialReferenceLookup[materialHashCode] = 0;
        //    }
        //    // Compute padding
        //    // TODO

        //    m_countInternal = 1;
        //}



        /// <summary>
        ///
        /// </summary>
        //public void Clear()
        //{
        //    //m_currentIndex = 0;
        //    m_MaterialReferenceLookup.Clear();
        //    m_SpriteAssetReferenceLookup.Clear();
        //    m_FontAssetReferenceLookup.Clear();
        //}


        /// <summary>
        /// Function to clear the reference count for each of the material references.
        /// </summary>
        //public void ClearReferenceCount()
        //{
        //    m_countInternal = 0;

        //    for (int i = 0; i < materialReferences.Length; i++)
        //    {
        //        if (materialReferences[i].fontAsset == null)
        //            return;

        //        materialReferences[i].referenceCount = 0;
        //    }
        //}

    }


    public struct TMP_MaterialReference
    {
        public Material material;
        public int referenceCount;
    }


    public struct MaterialReference
    {

        public int index;
        public TMP_FontAsset fontAsset;
        public TMP_SpriteAsset spriteAsset;
        public Material material;
        public bool isDefaultMaterial;
        public bool isFallbackMaterial;
        public Material fallbackMaterial;
        public float padding;
        public int referenceCount;


        /// <summary>
        /// Constructor for new Material Reference.
        /// </summary>
        /// <param name="index">The index in the material reference array.</param>
        /// <param name="fontAsset">The font asset for this material reference.</param>
        /// <param name="spriteAsset">The sprite asset for this material reference.</param>
        /// <param name="material">The material instance for this reference entry.</param>
        /// <param name="padding">The padding value applied to the material reference.</param>
        public MaterialReference(int index, TMP_FontAsset fontAsset, TMP_SpriteAsset spriteAsset, Material material, float padding)
        {
            this.index = index;
            this.fontAsset = fontAsset;
            this.spriteAsset = spriteAsset;
            this.material = material;
            this.isDefaultMaterial = material.GetEntityId() == fontAsset.material.GetEntityId();
            this.isFallbackMaterial = false;
            this.fallbackMaterial = null;
            this.padding = padding;
            this.referenceCount = 0;
        }


        /// <summary>
        /// Checks whether the font asset is contained in the material reference array.
        /// </summary>
        /// <param name="materialReferences">The material reference array to search.</param>
        /// <param name="fontAsset">The font asset to look for.</param>
        /// <returns>True if the font asset is in the array.</returns>
        public static bool Contains(MaterialReference[] materialReferences, TMP_FontAsset fontAsset)
        {
            EntityId id = fontAsset.GetEntityId();

            for (int i = 0; i < materialReferences.Length && materialReferences[i].fontAsset != null; i++)
            {
                if (materialReferences[i].fontAsset.GetEntityId() == id)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Adds a new material reference and returns its index in the material reference array.
        /// </summary>
        /// <param name="material">The material to add.</param>
        /// <param name="fontAsset">The font asset.</param>
        /// <param name="materialReferences">The material reference array.</param>
        /// <param name="materialReferenceIndexLookup">The lookup dictionary.</param>
        /// <returns>The index of the added or existing material reference.</returns>
        [Obsolete("AddMaterialReference(Material material, TMP_FontAsset fontAsset, ref MaterialReference[] materialReferences, Dictionary<int, int> materialReferenceIndexLookup) is obsolete, use AddMaterialReference(Material material, TMP_FontAsset fontAsset, ref MaterialReference[] materialReferences, Dictionary<int, int> materialReferenceIndexLookup) instead.", true)]
        public static int AddMaterialReference(Material material, TMP_FontAsset fontAsset, ref MaterialReference[] materialReferences, Dictionary<int, int> materialReferenceIndexLookup)
        {
            int materialID = material.GetEntityId();
            int index;

            if (materialReferenceIndexLookup.TryGetValue(materialID, out index))
                return index;

            index = materialReferenceIndexLookup.Count;

            // Add new reference index
            materialReferenceIndexLookup[materialID] = index;

            if (index >= materialReferences.Length)
                System.Array.Resize(ref materialReferences, Mathf.NextPowerOfTwo(index + 1));

            materialReferences[index].index = index;
            materialReferences[index].fontAsset = fontAsset;
            materialReferences[index].spriteAsset = null;
            materialReferences[index].material = material;
            materialReferences[index].isDefaultMaterial = materialID == fontAsset.material.GetEntityId();
            materialReferences[index].referenceCount = 0;

            return index;
        }

        /// <summary>
        /// Adds a new material reference and returns its index in the material reference array.
        /// </summary>
        /// <param name="material">The material to add.</param>
        /// <param name="fontAsset">The font asset.</param>
        /// <param name="materialReferences">The material reference array.</param>
        /// <param name="materialReferenceIndexLookup">The lookup dictionary.</param>
        /// <returns>The index of the added or existing material reference.</returns>
        public static int AddMaterialReference(Material material, TMP_FontAsset fontAsset, ref MaterialReference[] materialReferences, Dictionary<EntityId, int> materialReferenceIndexLookup)
        {
            EntityId materialID = material.GetEntityId();
            int index;

            if (materialReferenceIndexLookup.TryGetValue(materialID, out index))
                return index;

            index = materialReferenceIndexLookup.Count;

            // Add new reference index
            materialReferenceIndexLookup[materialID] = index;

            if (index >= materialReferences.Length)
                System.Array.Resize(ref materialReferences, Mathf.NextPowerOfTwo(index + 1));

            materialReferences[index].index = index;
            materialReferences[index].fontAsset = fontAsset;
            materialReferences[index].spriteAsset = null;
            materialReferences[index].material = material;
            materialReferences[index].isDefaultMaterial = materialID == fontAsset.material.GetEntityId();
            materialReferences[index].referenceCount = 0;

            return index;
        }


        /// <summary>
        /// Adds a new material reference for a sprite asset and returns its index in the array.
        /// </summary>
        /// <param name="material">The material to add.</param>
        /// <param name="spriteAsset">The sprite asset.</param>
        /// <param name="materialReferences">The material reference array.</param>
        /// <param name="materialReferenceIndexLookup">The lookup dictionary.</param>
        /// <returns>The index of the added or existing material reference.</returns>
        [Obsolete("AddMaterialReference(Material material, TMP_SpriteAsset spriteAsset, ref MaterialReference[] materialReferences, Dictionary<int, int> materialReferenceIndexLookup) is obsolete, use AddMaterialReference(Material material, TMP_SpriteAsset spriteAsset, ref MaterialReference[] materialReferences, Dictionary<int, int> materialReferenceIndexLookup) instead.", true )]
        public static int AddMaterialReference(Material material, TMP_SpriteAsset spriteAsset, ref MaterialReference[] materialReferences, Dictionary<int, int> materialReferenceIndexLookup)
        {
            int materialID = material.GetEntityId();
            int index;

            if (materialReferenceIndexLookup.TryGetValue(materialID, out index))
                return index;
            index = materialReferenceIndexLookup.Count;
            // Add new reference index
            materialReferenceIndexLookup[materialID] = index;
            if (index >= materialReferences.Length)
                System.Array.Resize(ref materialReferences, Mathf.NextPowerOfTwo(index + 1));
            materialReferences[index].index = index;
            materialReferences[index].fontAsset = materialReferences[0].fontAsset;
            materialReferences[index].spriteAsset = spriteAsset;
            materialReferences[index].material = material;
            materialReferences[index].isDefaultMaterial = true;
            materialReferences[index].referenceCount = 0;
            return index;
        }




        /// <summary>
        /// Adds a new material reference for a sprite asset and returns its index in the array.
        /// </summary>
        /// <param name="material">The material to add.</param>
        /// <param name="spriteAsset">The sprite asset.</param>
        /// <param name="materialReferences">The material reference array.</param>
        /// <param name="materialReferenceIndexLookup">The lookup dictionary.</param>
        /// <returns>The index of the added or existing material reference.</returns>
        public static int AddMaterialReference(Material material, TMP_SpriteAsset spriteAsset, ref MaterialReference[] materialReferences, Dictionary<EntityId, int> materialReferenceIndexLookup)
        {
            EntityId materialID = material.GetEntityId();
            int index;

            if (materialReferenceIndexLookup.TryGetValue(materialID, out index))
                return index;

            index = materialReferenceIndexLookup.Count;

            // Add new reference index
            materialReferenceIndexLookup[materialID] = index;

            if (index >= materialReferences.Length)
                System.Array.Resize(ref materialReferences, Mathf.NextPowerOfTwo(index + 1));

            materialReferences[index].index = index;
            materialReferences[index].fontAsset = materialReferences[0].fontAsset;
            materialReferences[index].spriteAsset = spriteAsset;
            materialReferences[index].material = material;
            materialReferences[index].isDefaultMaterial = true;
            materialReferences[index].referenceCount = 0;

            return index;
        }
    }
}
