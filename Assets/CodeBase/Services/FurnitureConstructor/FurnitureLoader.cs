using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;

namespace CodeBase.Services.FurnitureConstructor
{
    public class FurnitureLoader
    {
        private const string DatabasePath = "DataBase/database";
        private Dictionary<string, List<TypeInfo>> typeInfoDictionary;

        public Database LoadDatabase()
        {
            TextAsset databaseFile = Resources.Load<TextAsset>(DatabasePath);
            if (databaseFile == null)
            {
                // Debug.LogError($"Database file not found at path: {DatabasePath}");
                return null;
            }

            string json = CleanJavaScript(databaseFile.text);

            JObject jsonObject = JObject.Parse(json);
            Database database = JsonConvert.DeserializeObject<Database>(jsonObject.ToString());
            if (database == null)
            {
                // Debug.LogError("Failed to parse database from JSON.");
                return null;
            }

            //Debug.Log("Database loaded successfully.");

            JObject typesObject = jsonObject["types"] as JObject;
            typeInfoDictionary = new Dictionary<string, List<TypeInfo>>();
            if (typesObject != null)
            {
                foreach (var property in typesObject.Properties())
                {
                    string key = property.Name;
                    List<TypeInfo> list = property.Value.ToObject<List<TypeInfo>>();
                    typeInfoDictionary[key] = list;
                }
            }

            BindTypesToMaterialsAndStyles(database);
            LoadTexturesForMaterials(database);
            PrintDatabase(database);

            return database;
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
                int exportIndex = cleanedContent.IndexOf("export");
                cleanedContent = cleanedContent.Substring(0, exportIndex).Trim();
            }

            if (cleanedContent.EndsWith(";"))
            {
                cleanedContent = cleanedContent.Substring(0, cleanedContent.Length - 1).Trim();
            }

            return cleanedContent;
        }

        private void BindTypesToMaterialsAndStyles(Database database)
        {
            foreach (var category in database.modelsDB)
            {
                foreach (var obj in category.objects)
                {
                    foreach (var material in obj.materials)
                        // Связываем typeInfo, если указаны types
                        if (!string.IsNullOrEmpty(material.types) &&
                            typeInfoDictionary.TryGetValue(material.types, out var materialList))
                            material.typeInfo = materialList;

                    if (obj.styles != null)
                        foreach (var style in obj.styles)
                            if (!string.IsNullOrEmpty(style.types) &&
                                typeInfoDictionary.TryGetValue(style.types, out var styleList))
                                style.typeInfo = styleList;
                }
            }
        }

        private void LoadTexturesForMaterials(Database database)
        {
            foreach (var category in database.modelsDB)
            {
                foreach (var obj in category.objects)
                {
                    foreach (var material in obj.materials)
                    {
                        if (material.typeInfo != null)
                        {
                            foreach (var type in material.typeInfo)
                            {
                                if (!string.IsNullOrEmpty(type.texture))
                                {
                                    // Очистим путь, удалив ведущий слэш и расширение
                                    string cleanedPath = CleanTexturePath(type.texture);
                                    var texture = Resources.Load<Texture2D>(cleanedPath);

                                    if (texture != null)
                                    {
                                        material.texture = texture;
                                        //                                       Debug.Log($"Loaded texture for material '{material.label}': {texture.name}");
                                    }
                                    else
                                    {
                                        // Debug.LogWarning(
                                        //    $"Texture not found for '{material.label}' at path: {type.texture}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string CleanTexturePath(string path)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);

            if (path.EndsWith(".jpg"))
                path = path.Substring(0, path.Length - 4);
            else if (path.EndsWith(".png"))
                path = path.Substring(0, path.Length - 4);

            return path;
        }

        private void PrintDatabase(Database database)
        {
            // Debug.Log("Printing Database Contents:");

            foreach (var category in database.modelsDB)
            {
                // Debug.Log($"Category: {category.category}");

                foreach (var obj in category.objects)
                {
                    //  Debug.Log($"  Object Name: {obj.name}");
                    //   Debug.Log($"    Model Path: {obj.model}");
                    //  Debug.Log($"    Start Value: {obj.start}");

                    foreach (var morph in obj.morph)
                    {
                        // Debug.Log($"      Morph: {morph.label}, Min: {morph.min}, Max: {morph.max}");
                    }

                    foreach (var material in obj.materials)
                    {
                        // Debug.Log(
                        //  $"      Material: {material.label}, Types: {material.types}, Texture: {(material.texture != null ? material.texture.name : "None")}");
                        if (material.typeInfo != null)
                        {
                            foreach (var type in material.typeInfo)
                            {
                                //   Debug.Log($"        Type: {type.label}, Texture: {type.texture}, NameInModel: {type.name_in_model}");
                            }
                        }
                    }

                    if (obj.styles != null)
                    {
                        foreach (var style in obj.styles)
                        {
                            // Debug.Log($"      Style: {style.label}, Types: {style.types}");
                            if (style.typeInfo != null)
                            {
                                foreach (var type in style.typeInfo)
                                {
//                                     Debug.Log($"        Type: {type.label}, Texture: {type.texture}, NameInModel: {type.name_in_model}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class Database
    {
        public List<Category> modelsDB;
    }

    [Serializable]
    public class Category
    {
        public string category;
        public List<FurnitureObject> objects;
    }

    [Serializable]
    public class FurnitureObject
    {
        public string name;
        public string model;
        public float start;
        public List<Morph> morph;
        public List<CustomMaterial> materials;
        public List<Style> styles;
    }

    [Serializable]
    public class Morph
    {
        public string label;
        public float min;
        public float max;
    }

    [Serializable] public class CustomMaterial {
        public string label, types, name_in_model;
        public List<TypeInfo> typeInfo;
        public Texture2D texture;
    }

    [Serializable]
    public class Style
    {
        public string label, types;
        public List<TypeInfo> typeInfo;
    }

    [Serializable]
    public class TypeInfo
    {
        public string label;
        public string name_in_model;
        public string texture;
    }
}