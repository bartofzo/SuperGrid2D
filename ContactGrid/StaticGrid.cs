/*
    Copyright(c) 2018 Bart van de Sande / Nonline, https://www.nonline.nl

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System.Collections.Generic;
using UnityEngine;

namespace ContactGrid
{
    /// <summary>
    /// Faster to query on but slower to remove units from. Positions can not be updated.
    /// </summary>
    public class StaticGrid<T> : GridBase<T, StaticGrid<T>.StaticCell> where T : class
    {
        public StaticGrid(Vector2 topLeft, float width, float height, float cellSize) : base(topLeft, width, height, cellSize)
        {
        }

        public StaticGrid(Vector2 topLeft, float width, float height, Vector2 cellSize) : base(topLeft, width, height, cellSize)
        {
        }

        public StaticGrid(Vector2 center, float radius, float cellSize) : base(center, radius, cellSize)
        {
        }

        /// <summary>
        /// Creates a new static cell
        /// </summary>
        protected override StaticCell _createNewCell(Vector2Int location)
        {
            return new StaticCell();
        }

        /// <summary>
        /// Adds a unit.
        /// </summary>
        public void Add(T unit, IConvexShape shape)
        {
            foreach (var cell in _getOrCreateSupercover(shape))
                cell.Add(new UnitWrapper(unit, shape));

            Count++;
        }

        /// <summary>
        /// Removes a unit (slow)
        /// </summary>
        public bool Remove(T unit)
        {
            bool removed = false;
            foreach (var cell in _allOpenCells())
                removed |= cell.RemoveAll(unit);

            Count -= removed ? 1 : 0;

            return removed;
        }

        /// <summary>
        /// Static search grid uses a list per cell
        /// </summary>
        public class StaticCell : CellBase
        {
            protected override IEnumerable<UnitWrapper> _unitWrappers => _wrappedUnitList;
            protected List<UnitWrapper> _wrappedUnitList = new List<UnitWrapper>();

            public virtual void Add(UnitWrapper wrapper)
            {
                _wrappedUnitList.Add(wrapper);
            }

            public virtual bool RemoveAll(T unit)
            {
                bool removed = false;
                for (int i = _wrappedUnitList.Count - 1; i >= 0; i--)
                {
                    if (_wrappedUnitList[i].Unit == unit)
                    {
                        _wrappedUnitList.RemoveAt(i);
                        removed = true;
                    }
                }

                return removed;
            }
        }
    }
}