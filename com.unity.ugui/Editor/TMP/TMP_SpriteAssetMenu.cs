using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.Text;
using UnityEngine.U2D;
using UnityEditor;


namespace TMPro.EditorUtilities
{
    public static class TMP_SpriteAssetMenu
    {
        //[MenuItem("Assets/Create/TextMeshPro/Sprite Asset", false, 150)]
        static void CreateSpriteAsset()
        {
            Object[] targets = Selection.objects;

            if (targets == null)
            {
                Debug.LogWarning("A Sprite Texture must first be selected in order to create a Sprite Asset.");
                return;
            }

            // Make sure TMP Essential Resources have been imported in the user project.
            if (TMP_Settings.instance == null)
            {
                Debug.Log("Unable to create sprite asset. Please import the TMP Essential Resources.");

                // Show Window to Import TMP Essential Resources
                return;
            }

            for (int i = 0; i < targets.Length; i++)
            {
                Object target = targets[i];

                // Make sure the selection is a font file
                if (target == null || target.GetType() != typeof(Texture2D))
                {
                    Debug.LogWarning("Selected Object [" + target.name + "] is not a Sprite Texture. A Sprite Texture must be selected in order to create a Sprite Asset.", target);
                    continue;
                }

                CreateSpriteAssetFromSelectedObject(target);
            }
        }


        static void CreateSpriteAssetFromSelectedObject(Object target)
        {
            // Get the path to the selected asset.
            string filePathWithName = AssetDatabase.GetAssetPath(target);
            string fileNameWithExtension = Path.GetFileName(filePathWithName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathWithName);
            string filePath = filePathWithName.Replace(fileNameWithExtension, "");
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(filePath + fileNameWithoutExtension + ".asset");

            // Create new Sprite Asset
            SpriteAsset spriteAsset = ScriptableObject.CreateInstance<SpriteAsset>();
            AssetDatabase.CreateAsset(spriteAsset, uniquePath);

            spriteAsset.version = "1.1.0";

            // Compute the hash code for the sprite asset.
            spriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteAsset.name);

            List<SpriteGlyph> spriteGlyphTable = new List<SpriteGlyph>();
            List<SpriteCharacter> spriteCharacterTable = new List<SpriteCharacter>();

            if (target.GetType() == typeof(Texture2D))
            {
                Texture2D sourceTex = target as Texture2D;

                // Assign new Sprite Sheet texture to the Sprite Asset.
                spriteAsset.spriteSheet = sourceTex;

                PopulateSpriteTables(sourceTex, ref spriteCharacterTable, ref spriteGlyphTable);

                spriteAsset.spriteCharacterTable = spriteCharacterTable;
                spriteAsset.spriteGlyphTable = spriteGlyphTable;

                // Add new default material for sprite asset.
                AddDefaultMaterial(spriteAsset);
            }
            else if (target.GetType() == typeof(SpriteAtlas))
            {
                //SpriteAtlas spriteAtlas = target as SpriteAtlas;

                //PopulateSpriteTables(spriteAtlas, ref spriteCharacterTable, ref spriteGlyphTable);

                //spriteAsset.spriteCharacterTable = spriteCharacterTable;
                //spriteAsset.spriteGlyphTable = spriteGlyphTable;

                //spriteAsset.spriteSheet = spriteGlyphTable[0].sprite.texture;

                //// Add new default material for sprite asset.
                //AddDefaultMaterial(spriteAsset);
            }

            // Update Lookup tables.
            spriteAsset.UpdateLookupTables();

            // Get the Sprites contained in the Sprite Sheet
            EditorUtility.SetDirty(spriteAsset);

            //spriteAsset.sprites = sprites;

            // Set source texture back to Not Readable.
            //texImporter.isReadable = false;

            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(spriteAsset));  // Re-import font asset to get the new updated version.

            //AssetDatabase.Refresh();
        }


        static void PopulateSpriteTables(Texture source, ref List<SpriteCharacter> spriteCharacterTable, ref List<SpriteGlyph> spriteGlyphTable)
        {
            //Debug.Log("Creating new Sprite Asset.");

            string filePath = AssetDatabase.GetAssetPath(source);

            // Get all the Sprites sorted by Index
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(filePath).Select(x => x as Sprite).Where(x => x != null).OrderByDescending(x => x.rect.y).ThenBy(x => x.rect.x).ToArray();

            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];

                SpriteGlyph spriteGlyph = new SpriteGlyph();
                spriteGlyph.index = (uint)i;
                spriteGlyph.metrics = new GlyphMetrics(sprite.rect.width, sprite.rect.height, -sprite.pivot.x, sprite.rect.height - sprite.pivot.y, sprite.rect.width);
                spriteGlyph.glyphRect = new GlyphRect(sprite.rect);
                spriteGlyph.scale = 1.0f;
                spriteGlyph.sprite = sprite;

                spriteGlyphTable.Add(spriteGlyph);

                SpriteCharacter spriteCharacter = new SpriteCharacter(0xFFFE, spriteGlyph);

                // Special handling for .notdef sprite name.
                string fileNameToLowerInvariant = sprite.name.ToLowerInvariant();
                if (fileNameToLowerInvariant == ".notdef" || fileNameToLowerInvariant == "notdef")
                {
                    spriteCharacter.unicode = 0;
                    spriteCharacter.name = fileNameToLowerInvariant;
                }
                else
                {
                    if (!string.IsNullOrEmpty(sprite.name) && sprite.name.Length > 2 && sprite.name[0] == '0' && (sprite.name[1] == 'x' || sprite.name[1] == 'X'))
                    {
                        spriteCharacter.unicode = (uint)TMP_TextUtilities.StringHexToInt(sprite.name.Remove(0, 2));
                    }
                    spriteCharacter.name = sprite.name;
                }

                spriteCharacter.scale = 1.0f;

                spriteCharacterTable.Add(spriteCharacter);
            }
        }


        static void PopulateSpriteTables(SpriteAtlas spriteAtlas, ref List<SpriteCharacter> spriteCharacterTable, ref List<SpriteGlyph> spriteGlyphTable)
        {
            // Get number of sprites contained in the sprite atlas.
            int spriteCount = spriteAtlas.spriteCount;
            Sprite[] sprites = new Sprite[spriteCount];

            // Get all the sprites
            spriteAtlas.GetSprites(sprites);

            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];

                SpriteGlyph spriteGlyph = new SpriteGlyph();
                spriteGlyph.index = (uint)i;
                spriteGlyph.metrics = new GlyphMetrics(sprite.textureRect.width, sprite.textureRect.height, -sprite.pivot.x, sprite.textureRect.height - sprite.pivot.y, sprite.textureRect.width);
                spriteGlyph.glyphRect = new GlyphRect(sprite.textureRect);
                spriteGlyph.scale = 1.0f;
                spriteGlyph.sprite = sprite;

                spriteGlyphTable.Add(spriteGlyph);

                SpriteCharacter spriteCharacter = new SpriteCharacter(0xFFFE, spriteGlyph);
                spriteCharacter.name = sprite.name;
                spriteCharacter.scale = 1.0f;

                spriteCharacterTable.Add(spriteCharacter);
            }
        }


        /// <summary>
        /// Create and add new default material to sprite asset.
        /// </summary>
        /// <param name="spriteAsset"></param>
        static void AddDefaultMaterial(SpriteAsset spriteAsset)
        {
            Shader shader = Shader.Find("TextMeshPro/Sprite");
            Material material = new Material(shader);
            material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

            spriteAsset.material = material;
            material.name = spriteAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(material, spriteAsset);
        }
    }
}
