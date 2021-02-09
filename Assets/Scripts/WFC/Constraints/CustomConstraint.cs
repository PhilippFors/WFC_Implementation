using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Constraints;

namespace MyWFC
{
    public abstract class CustomConstraint : MonoBehaviour
    {
        public bool useConstraint = true;
        public abstract void SetConstraint(TilePropagator propagator, RuntimeTile[] tileSet);
    }
}
