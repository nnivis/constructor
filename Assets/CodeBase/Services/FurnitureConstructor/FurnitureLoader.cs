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
        private Dictionary<string, FurnitureData> _furnitureDataLookup; // Словарь для быстрого доступа
        private Database _database;

        public FurnitureData GetFurnitureData(string furnitureName)
        {
            if (_database == null)
                LoadDatabase();

            _furnitureDataLookup.TryGetValue(furnitureName, out var data);
            return data;
        }

        public Database LoadDatabase()
        {
            if (_database != null)
                return _database;

            TextAsset databaseFile = Resources.Load<TextAsset>(DatabasePath); // Загрузка БД из ресурсов
            if (databaseFile == null)
            {
                Debug.LogError($"Database file not found at path: {DatabasePath}");
                return null;
            }

            string json = CleanJavaScript(databaseFile.text);
            _database = JsonConvert.DeserializeObject<Database>(json);
            if (_database == null)
            {
                Debug.LogError("Failed to parse database from JSON.");
                return null;
            }

            JObject jsonObject = JObject.Parse(json);
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

            InitializeFurnitureLookup();
            ProcessFurnitureData(_database);

            return _database;
        }

        private void InitializeFurnitureLookup()
        {
            _furnitureDataLookup = new Dictionary<string, FurnitureData>();
            foreach (var category in _database.modelsDB)
            {
                foreach (var furniture in category.objects)
                {
                    if (!_furnitureDataLookup.ContainsKey(furniture.name))
                    {
                        _furnitureDataLookup.Add(furniture.name, furniture);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate furniture name detected: {furniture.name}. Overwriting existing entry.");
                        _furnitureDataLookup[furniture.name] = furniture;
                    }
                }
            }
        }

        
        private void ProcessFurnitureData(Database database)
        {
            foreach (var category in database.modelsDB)
            {
                foreach (var data in category.objects)
                {
                    if (data.materials != null)
                    {
                        foreach (var material in data.materials)
                        {
                            if (!string.IsNullOrEmpty(material.types) &&
                                _typeInfoDictionary.TryGetValue(material.types, out var materialList))
                            {
                                material.typeInfo = materialList;
                            }
                        }
                    }

                    if (data.styles != null)
                    {
                        foreach (var style in data.styles)
                        {
                            if (!string.IsNullOrEmpty(style.types) &&
                                _typeInfoDictionary.TryGetValue(style.types, out var styleList))
                            {
                                style.typeInfo = styleList;
                            }
                        }
                    }

                    if (data.morph != null)
                    {
                        foreach (var m in data.morph)
                        {
                            if (!data.Parts.ContainsKey(m.label))
                            {
                                data.Parts[m.label] = new PartData { morphInfo = m };
                            }
                            else
                            {
                                data.Parts[m.label].morphInfo = m;
                            }
                        }
                    }

                    // Инициализация материалов
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

                                    data.AddMaterial(
                                        material.label,
                                        new MaterialInfo
                                        {
                                            label = material.label,
                                            nameInModel = material.name_in_model,
                                            texturePath = texturePath,
                                            textureLabel = type.label
                                        }
                                    );
                                }
                            }
                        }
                    }

                    // Инициализация стилей
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

            int extensionIndex = path.LastIndexOf('.');
            if (extensionIndex > 0)
            {
                path = path.Substring(0, extensionIndex);
            }

            return path;
        }
    }
}
