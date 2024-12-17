using System;
using System.Collections.Generic;
using CodeBase.Services.FurnitureConstructor.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor
{
    public class FurnitureLoader
    {
        private const string DatabasePath = "DataBase/database";
        private Dictionary<string, List<TypeInfo>> _typeInfoDictionary;
        private Database _database;

        public FurnitureData GetFurnitureData(string furnitureName)
        {
            if (_database == null)
                LoadDatabase();

            foreach (var category in _database.modelsDB)
            {
                var data = category.objects.Find(o => o.name == furnitureName);
                if (data != null)
                    return data;
            }

            return null;
        }

        public Database LoadDatabase()
        {
            if (_database != null)
                return _database;

            TextAsset databaseFile = Resources.Load<TextAsset>(DatabasePath); // загрузка бд из Ресурсов
            if (databaseFile == null)
            {
                Debug.LogError($"Database file not found at path: {DatabasePath}");
                return null;
            }

            string json = CleanJavaScript(databaseFile.text);
            JObject jsonObject = JObject.Parse(json);
            _database = JsonConvert.DeserializeObject<Database>(jsonObject.ToString());
            if (_database == null)
            {
                Debug.LogError("Failed to parse database from JSON.");
                return null;
            }

            JObject typesObject = jsonObject["types"] as JObject;
            _typeInfoDictionary = new Dictionary<string, List<TypeInfo>>();
            if (typesObject != null)
            {
                foreach (var property in typesObject.Properties())
                {
                    string key = property.Name;
                    List<TypeInfo> list = property.Value.ToObject<List<TypeInfo>>();
                    _typeInfoDictionary[key] = list;
                }
            }

            BindTypesToMaterialsAndStyles(_database);
            LoadTexturesForMaterials(_database);
            InitializeFurnitureDataParts(_database);

            return _database;
        }

        private void BindTypesToMaterialsAndStyles(Database database)
        {
            foreach (var category in database.modelsDB)
            {
                foreach (var data in category.objects)
                {
                    foreach (var material in data.materials)
                    {
                        if (!string.IsNullOrEmpty(material.types) &&
                            _typeInfoDictionary.TryGetValue(material.types, out var materialList))
                            material.typeInfo = materialList;
                    }

                    if (data.styles != null)
                    {
                        foreach (var style in data.styles)
                        {
                            if (!string.IsNullOrEmpty(style.types) &&
                                _typeInfoDictionary.TryGetValue(style.types, out var styleList))
                                style.typeInfo = styleList;
                        }
                    }
                }
            }
        }

        private void LoadTexturesForMaterials(Database database)
        {
            foreach (var category in database.modelsDB)
            {
                foreach (var data in category.objects)
                {
                    foreach (var material in data.materials)
                    {
                        if (material.typeInfo != null)
                        {
                            foreach (var type in material.typeInfo)
                            {
                                if (!string.IsNullOrEmpty(type.texture))
                                {
                                    string cleanedPath = CleanTexturePath(type.texture);
                                    var texture = Resources.Load<Texture2D>(cleanedPath);
                                    if (texture != null)
                                    {
                                        material.texture = texture;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitializeFurnitureDataParts(Database database)
        {
            foreach (var category in database.modelsDB)
            {
                foreach (var data in category.objects)
                {
                    if (data.morph != null)
                    {
                        foreach (var m in data.morph)
                        {
                            if (!data.parts.ContainsKey(m.label))
                                data.parts[m.label] = new PartData {morphInfo = m};
                            else
                                data.parts[m.label].morphInfo = m;
                        }
                    }

                    if (data.materials != null)
                    {
                        foreach (var material in data.materials)
                        {
                            if (material.typeInfo == null) continue;

                            foreach (var type in material.typeInfo)
                            {
                                if (!string.IsNullOrEmpty(type.texture))
                                {
                                    var texturePath = CleanTexturePath(type.texture);
                                    var loadedTexture = Resources.Load<Texture2D>(texturePath);
                                    Debug.Log(
                                        $"Adding MaterialInfo: {material.label} with texture {loadedTexture?.name} and Texture Path {texturePath}");

                                    data.AddMaterial(
                                        material.label,
                                        new MaterialInfo
                                        {
                                            label = material.label,
                                            nameInModel = material.name_in_model,
                                            texturePath = texturePath,
                                            textureLabel = loadedTexture.name,
                                            //texture = loadedTexture
                                        }
                                    );
                                }
                            }
                        }
                    }
                    
                    if (data.styles != null)
                    {
                        foreach (var style in data.styles)
                        {
                            if (style.typeInfo == null) continue;
                            foreach (var type in style.typeInfo)
                            {
                                data.AddStyle(
                                    style.label,
                                    style.types,
                                    new StyleInfo
                                    {
                                        label = type.label,
                                        nameInModel = type.name_in_model
                                    }
                                );
                            }
                        }
                    }
                }
            }
        }

        private string CleanJavaScript(string jsContent)
        {
            string cleanedContent = jsContent;

            if (cleanedContent.Contains("const database ="))
            {
                cleanedContent = cleanedContent.Replace("const database =", "").Trim();
            }

            if (cleanedContent.Contains("export"))
            {
                int exportIndex = cleanedContent.IndexOf("export", StringComparison.Ordinal);
                cleanedContent = cleanedContent.Substring(0, exportIndex).Trim();
            }

            if (cleanedContent.EndsWith(";"))
            {
                cleanedContent = cleanedContent.Substring(0, cleanedContent.Length - 1).Trim();
            }

            return cleanedContent;
        }

        private string CleanTexturePath(string path)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);

            if (path.EndsWith(".jpg") || path.EndsWith(".png"))
                path = path.Substring(0, path.LastIndexOf('.'));

            return path;
        }
    }
}