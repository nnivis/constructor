using CodeBase.Services.FurnitureConstructor;
using UnityEngine;

namespace CodeBase.Infrastructure
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private GameObject gameObject1Example;
        [SerializeField] private GameObject gameObject2Example;
        
        [SerializeField] private FurnitureHandler furnitureHandler;

        private void Start()
        {
            furnitureHandler.Initialize();

            //furnitureHandler.GenerateFurniture(gameObject2Example);
            furnitureHandler.GenerateFurniture(gameObject1Example);
        }
    }
}