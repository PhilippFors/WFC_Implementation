using UnityEngine;
using DeBroglie;

namespace MyWFC
{
    
    public abstract class CustomConstraint : MonoBehaviour
    {
        public bool useConstraint = true;
        public abstract void SetConstraint(RuntimeTile[] tileSet, TilePropagator propagator);
    }
}
