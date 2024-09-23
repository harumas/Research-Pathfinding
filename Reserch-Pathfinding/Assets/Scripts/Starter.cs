using UnityEngine;

namespace DefaultNamespace
{
    public class Starter : MonoBehaviour
    {
        [SerializeField] private Vector2[] vertices;
        
        private void Start()
        {
            var triangulator = new Triangulator();
            triangulator.CreateInfluencePolygon(vertices);
        }
    }
}