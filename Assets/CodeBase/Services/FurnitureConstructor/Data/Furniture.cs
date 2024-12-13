using System.Text;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Data
{
    public class Furniture : MonoBehaviour
    {
        public GameObject Prefab { get; private set; }
        public FurnitureData Data { get; private set; }

        private Modifier.Modifier _modifier;

        public void Initialize(GameObject prefab, FurnitureData data, Modifier.Modifier modifier)
        {
            Prefab = prefab;
            Data = data;
            _modifier = modifier;

            StartInitializeModifier();
            
            DebugFurnitureData();
        }

        private void StartInitializeModifier()
        {
            _modifier.SetStartModifier(Data, gameObject);
            _modifier.SetStartSize(Data, Prefab);
        }

        public Vector2[] GetUV(MorphType type, string objectName) => Data.GetUV(type, objectName);

        private void DebugFurnitureData()
        {
            Debug.Log("----------------------------------------------------------");

            if (Data == null)
            {
                Debug.LogError("FurnitureData is null");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Furniture Name: {Data.name}");
            sb.AppendLine($"Start Value: {Data.start}");
            sb.AppendLine("Parts:");

            foreach (var part in Data.parts)
            {
                sb.AppendLine($"  Part Name: {part.Key}");
                if (part.Value.morphInfo != null)
                {
                    sb.AppendLine(
                        $"    Morph Info: Label={part.Value.morphInfo.label}, Min={part.Value.morphInfo.min}, Max={part.Value.morphInfo.max}");
                }

                sb.AppendLine("    Materials:");
                foreach (var material in part.Value.materials)
                {
                    sb.AppendLine(
                        $"      Material: Label={material.label}, NameInModel={material.nameInModel}, Texture={(material.texture != null ? material.texture.name : "None")}");
                }

                sb.AppendLine("    Styles:");
                foreach (var style in part.Value.styles)
                {
                    sb.AppendLine($"      Style Key: {style.Key}");
                    foreach (var styleInfo in style.Value)
                    {
                        sb.AppendLine($"        Label: {styleInfo.label}, NameInModel: {styleInfo.nameInModel}");
                    }
                }
            }

            sb.AppendLine("Morph UVs:");
            foreach (var morphType in Data.morphUVs)
            {
                sb.AppendLine($"  Morph Type: {morphType.Key}");
                foreach (var uv in morphType.Value)
                {
                    sb.AppendLine($"    Object: {uv.Key}, UV Count: {uv.Value.Length}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
