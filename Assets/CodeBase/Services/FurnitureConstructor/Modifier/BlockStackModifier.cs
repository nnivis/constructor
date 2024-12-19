using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Modifier
{
    public class BlockStackModifier
    {
        private const string BlockKeyword = "(block)";
        private const string BlockTopKeyword = "(block-top)";
        private const string TempBlockKeyword = "TempBlock";

        public void UpdateBlocksByHeight(GameObject parent, float targetHeight)
        {
            GameObject baseBlock = null;
            GameObject topBlock = null;

            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.transform.GetChild(i);

                if (child.name.Contains(BlockKeyword)) 
                    child.gameObject.SetActive(true);

                if (child.name.Contains(TempBlockKeyword))
                {
                    Object.Destroy(child.gameObject);
                }

                if (child.name.Contains(BlockKeyword) && !child.name.Contains(TempBlockKeyword))
                {
                    baseBlock = child.gameObject;
                }

                if (child.name.Contains(BlockTopKeyword))
                {
                    topBlock = child.gameObject;
                }
            }

            if (baseBlock != null)
            {
                float topHeight = 0;

                if (topBlock != null)
                {
                    var topBounds = GetBounds(topBlock);
                    topHeight = topBounds.max.y - topBounds.min.y;
                }

                var baseBounds = GetBounds(baseBlock);
                float blockHeight = baseBounds.max.y - baseBounds.min.y;
                float currentY = baseBlock.transform.position.y;

                int numBlocks = Mathf.FloorToInt((targetHeight - topHeight - currentY) / blockHeight) + 4;

                var newBlocks = new List<GameObject> { baseBlock };

                for (int i = 0; i < numBlocks - 1; i++)
                {
                    GameObject newBlock = GameObject.Instantiate(baseBlock, parent.transform);
                    newBlock.name = $"{baseBlock.name}{TempBlockKeyword}{i}";
                    var position = baseBlock.transform.position;
                    newBlock.transform.position = new Vector3(
                        position.x,
                        currentY + blockHeight,
                        position.z
                    );
                    currentY = newBlock.transform.position.y;
                    newBlocks.Add(newBlock);
                }

                GameObject lastBlock = newBlocks[newBlocks.Count - 1];
                if (lastBlock.transform.position.y > targetHeight - topHeight - 2 * blockHeight)
                {
                    lastBlock.SetActive(false);
                }
            }
        }

        private Bounds GetBounds(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds;
            }

            return new Bounds(obj.transform.position, Vector3.zero);
        }
    }
}
