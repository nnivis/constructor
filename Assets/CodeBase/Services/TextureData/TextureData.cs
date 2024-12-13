using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Services.TextureData
{
    [CreateAssetMenu(menuName = "Texture Data/TextureData", fileName = "TextureData")]
    public class TextureData : ScriptableObject
    {
        [System.Serializable]
        public class TextureEntry
        {
            [HideInInspector] public string name; 
            public Texture2D texture;
        }

        [SerializeField] private List<TextureEntry> _textures = new List<TextureEntry>();

        public Texture2D GetTexture(string name)
        {
            var entry = _textures.Find(t => t.name == name);
            if (entry != null)
            {
                return entry.texture;
            }
            Debug.LogWarning($"Texture with name '{name}' not found.");
            return null;
        }

        private void OnValidate()
        {
            foreach (var textureEntry in _textures)
            {
                if (textureEntry.texture != null)
                {
                    textureEntry.name = textureEntry.texture.name; 
                }
            }
        }
    }
}