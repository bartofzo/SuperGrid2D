using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ContactGrid
{
    public class GridManager : MonoBehaviour
    {
        public IGrid<DemoUnit> GridInterface => _grid;

        private DynamicGrid<int, DemoUnit> _grid;

        private int _keyCounter = 0;
        public int NewKey => _keyCounter++;

        private void Awake()
        {
            _grid = new DynamicGrid<int, DemoUnit>(Vector2.zero, 100, 10);
        }

        public void Add(DemoUnit unit)
        {
            _grid.Add(unit.Key, unit, unit.GetShape());
        }

        public void UpdateShape(int key, IConvexShape shape)
        {
            _grid.Update(key, shape);
        }
    }
}