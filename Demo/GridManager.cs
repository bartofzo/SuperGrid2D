using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supergrid2D
{
    public class GridManager : MonoBehaviour
    {
        public IGrid2D<DemoUnit> GridInterface => _grid;

        private DynamicGrid2D<int, DemoUnit> _grid;

        private int _keyCounter = 0;
        public int NewKey => _keyCounter++;

        private void Awake()
        {
            _grid = new DynamicGrid2D<int, DemoUnit>(Vector2.zero, 100, 10);
        }

        public void Add(DemoUnit unit)
        {
            _grid.Add(unit.Key, unit, unit.GetShape());
        }

        public void UpdateShape(int key, IConvex2D shape)
        {
            _grid.Update(key, shape);
        }
    }
}