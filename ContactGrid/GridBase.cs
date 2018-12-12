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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContactGrid
{
    /// <summary>
    /// Interface that shapes use to determine their supercover
    /// </summary>
    public interface IGridDimensions
    {
        Vector2 TopLeft { get; }
        float Width { get; }
        float Height { get; }
        Vector2 CellSize { get; }
        int Columns { get; }
        int Rows { get; }
    }

    /// <summary>
    /// Interface for performing queries on a general grid
    /// </summary>
    public interface IGrid<T> : IGridDimensions
    {
        int Count { get; }

        T FirstContact(IConvexShape shape);
        IEnumerable<T> Contact(IConvexShape shape);
        IEnumerable<T> ContactWhich(IConvexShape shape, Predicate<T> predicate);
        IEnumerable<T> ContactExcept(IConvexShape shape, T except);

        T GetNearest(Vector2 position);
        T GetNearest(Vector2 position, float maxDistance);
        T GetNearestWhich(Vector2 position, Predicate<T> predicate);
        T GetNearestWhich(Vector2 position, float maxDistance, Predicate<T> predicate);
        T GetNearestExcept(Vector2 position, T except);
        T GetNearestExcept(Vector2 position, float maxDistance, T except);
    }

    public abstract class GridBase<T, TCell> : IGrid<T> where T : class where TCell : GridBase<T, TCell>.CellBase
    {
        public int Count { get; protected set; } // Implementations should keep track of count except on base.Clear() count is always set to zero
        public Vector2 TopLeft { get; private set; }
        public Vector2 CellSize { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }

        protected TCell[,] _cells;
        protected abstract TCell _createNewCell(Vector2Int location);

        private readonly float _diagonal; // is the maximum distance units can be away from each other
        private int _queryNumber; // number is incremented each query and unitwrappers check this number to prevent duplicate results for units that span multiple cells

        /// <summary>
        /// Constructs a Grid
        /// An optimal cellSize would be around double the average query radius
        /// </summary>
        protected GridBase(Vector2 topLeft, float width, float height, Vector2 cellSize)
        {
            if (width <= 0 || height <= 0)
                throw new Exception("Width and height must be greater than zero");
            if (cellSize.x <= 0 || cellSize.y <= 0)
                throw new Exception("Cell width and height must be greater than zero");
            if (float.IsPositiveInfinity(width) || float.IsPositiveInfinity(height))
                throw new Exception("Width and height must can't be infinite");
            if (float.IsPositiveInfinity(cellSize.x) || float.IsPositiveInfinity(cellSize.y))
                throw new Exception("Cell size can't be infinte");

            // Set the bounds of this grid
            TopLeft = topLeft;
            CellSize = cellSize;

            Width = width;
            Height = height;

            // The maximum distance possible within the grid
            _diagonal = Mathf.Sqrt(width * width + height * height);

            // Create our cell array
            Columns = Mathf.CeilToInt(width / cellSize.x);
            Rows = Mathf.CeilToInt(height / cellSize.y);
            _cells = new TCell[Columns, Rows];
        }

        protected GridBase(Vector2 center, float radius, float cellSize) : this(new Vector2(center.x - radius, center.y - radius), 2 * radius, 2 * radius, new Vector2(cellSize, cellSize))
        {
        }

        protected GridBase(Vector2 topLeft, float width, float height, float cellSize) : this(topLeft, width, height, new Vector2(cellSize, cellSize))
        {
        }

        /// <summary>
        /// Clears the grid and sets count to zero
        /// </summary>
        public virtual void Clear()
        {
            _cells = new TCell[Columns, Rows];
            Count = 0;
        }

        /// <summary>
        /// Enumerates all cells that are in the supercover of shape and creates a new cell if that cell was null
        /// </summary>
        protected IEnumerable<TCell> _getOrCreateSupercover(IConvexShape shape)
        {
            foreach (var location in shape.Supercover(this))
            {
                if (_cells[location.x, location.y] == null)
                    _cells[location.x, location.y] = _createNewCell(location);

                yield return _cells[location.x, location.y];
            }
        }


        /// <summary>
        /// Enumerates all cells that are not null. Meaning they have been accessed before.
        /// </summary>
        protected IEnumerable<TCell> _allOpenCells()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (_cells[x, y] != null)
                        yield return _cells[x, y];
                }
            }
        }

        /// <summary>
        /// Finds nearest neighbour within maxDistance
        /// </summary>
        protected T _nearest(Vector2 position, float maxDistance, Predicate<T> predicate, out float distSquared)
        {
            T nearestUnit = null;
            float nearestDist = float.PositiveInfinity;
            float radius = Mathf.Min(CellSize.x, CellSize.y); // Intial search radius

            // Handles a cell
            Action<TCell> handleCell = (cell) =>
            {
                float nearestDistInCell;
                var nearestInCell = cell.Nearest(position, radius, predicate, out nearestDistInCell);

                if (nearestDistInCell < nearestDist)
                {
                    nearestDist = nearestDistInCell;
                    nearestUnit = nearestInCell;
                }
            };

            // Keep searching and doubling our radius until we've found a unit or when we've searched beyond the limit
            while (nearestUnit == null && radius <= maxDistance)
            {
                foreach (Vector2Int cellIndex in new Circle(position, radius).Supercover(this))
                {
                    if (_cells[cellIndex.x, cellIndex.y] != null)
                        handleCell(_cells[cellIndex.x, cellIndex.y]);
                }

                radius *= 2;
            }

            // In case our radius expanded beyond the limit we need to search one more time exactly at limit
            if (radius > maxDistance && nearestUnit == null)
            {
                radius = maxDistance;

                foreach (Vector2Int cellIndex in new Circle(position, radius).Supercover(this))
                    if (_cells[cellIndex.x, cellIndex.y] != null)
                        handleCell(_cells[cellIndex.x, cellIndex.y]);
            }

            distSquared = nearestDist;
            return nearestUnit;
        }

        /// <summary>
        /// Returns the nearest unit that is found from position. Without limit.
        /// Beware that this method is potentially costly when the nearest unit is very far outside of the cell size.
        /// Use the overload with maxDistance instead for faster operation. Position must be within bounds.
        /// </summary>
        public T GetNearest(Vector2 position)
        {
            float d;
            return _nearest(position, _diagonal, (u) => true, out d);
        }

        /// <summary>
        /// Returns the nearest unit that is found from position within a given limit search radius.
        /// </summary>
        public T GetNearest(Vector2 position, float maxDistance)
        {
            float d;
            return _nearest(position, maxDistance, (u) => true, out d);
        }

        /// <summary>
        /// Nearest unit that conforms to a predicate
        /// </summary>
        public T GetNearestWhich(Vector2 position, Predicate<T> predicate)
        {
            float d;
            return _nearest(position, _diagonal, predicate, out d);
        }

        /// <summary>
        /// Returns the nearest unit that conforms to a predicate.
        /// </summary>
        public T GetNearestWhich(Vector2 position, float maxDistance, Predicate<T> predicate)
        {
            float d;
            return _nearest(position, maxDistance, predicate, out d);
        }

        /// <summary>
        /// Returns the nearest unit that is not unit
        /// </summary>
        public T GetNearestExcept(Vector2 position, T except)
        {
            float d;
            return _nearest(position, _diagonal, (unit) => unit != except, out d);
        }

        /// <summary>
        /// Returns the nearest unit that is not unit
        /// </summary>
        public T GetNearestExcept(Vector2 position, float maxDistance, T except)
        {
            float d;
            return _nearest(position, maxDistance, (unit) => unit != except, out d);
        }

        /// <summary>
        /// Returns the first object found that contacts shape. It might speak an alien language to us humans.
        /// </summary>
        public T FirstContact(IConvexShape shape)
        {
            _queryNumber++;

            foreach (Vector2Int cellIndex in shape.Supercover(this))
            {
                if (_cells[cellIndex.x, cellIndex.y] != null)
                {
                    foreach (var unit in _cells[cellIndex.x, cellIndex.y].Contact(shape, (u) => true, _queryNumber))
                        return unit;
                }
            }

            return null;
        }

        /// <summary>
        /// Enumarates all objects that overlap with shape
        /// </summary>
        public IEnumerable<T> Contact(IConvexShape shape)
        {
            // Increment our query number to prevent yielding a unit multiple times if it spans more than one cell
            _queryNumber++;

            foreach (Vector2Int cellIndex in shape.Supercover(this))
            {
                if (_cells[cellIndex.x, cellIndex.y] != null)
                {
                    foreach (var unit in _cells[cellIndex.x, cellIndex.y].Contact(shape, (u) => true, _queryNumber))
                        yield return unit;
                }
            }
        }

        /// <summary>
        /// Enumarates all objects that overlap with shape and that conform to a predicate
        /// </summary>
        public IEnumerable<T> ContactWhich(IConvexShape shape, Predicate<T> predicate)
        {
            // Increment our query number to prevent yielding a unit multiple times if it spans more than one cell
            _queryNumber++;

            foreach (Vector2Int cellIndex in shape.Supercover(this))
            {
                if (_cells[cellIndex.x, cellIndex.y] != null)
                {
                    foreach (var unit in _cells[cellIndex.x, cellIndex.y].Contact(shape, predicate, _queryNumber))
                        yield return unit;
                }
            }
        }

        /// <summary>
        /// Enumarates all objects that overlap with shape
        /// </summary>
        public IEnumerable<T> ContactExcept(IConvexShape shape, T except)
        {
            // Increment our query number to prevent yielding a unit multiple times if it spans more than one cell
            _queryNumber++;

            foreach (Vector2Int cellIndex in shape.Supercover(this))
            {
                if (_cells[cellIndex.x, cellIndex.y] != null)
                {
                    foreach (var unit in _cells[cellIndex.x, cellIndex.y].Contact(shape, (unit) => unit != except, _queryNumber))
                        yield return unit;
                }
            }
        }

        /// <summary>
        /// Base wrapper for units that are inside a cell
        /// </summary>
        public class UnitWrapper
        {
            public readonly T Unit;
            public readonly IConvexShape Shape;

            private int _lastQueryNumber; // Keeps track of the last query number that has been made to prevent duplicate results

            public UnitWrapper(T unit, IConvexShape shape)
            {
                Shape = shape;
                Unit = unit;
            }

            /// <summary>
            /// Method to determine if a unit has already been returned for the current query
            /// </summary>
            public bool Once(int queryNumber)
            {
                bool isOnce = this._lastQueryNumber != queryNumber;
                _lastQueryNumber = queryNumber;
                return isOnce;
            }
        }

        /// <summary>
        /// Blueprint for searching within a cell
        /// </summary>
        public abstract class CellBase
        {
            protected abstract IEnumerable<UnitWrapper> _unitWrappers { get; }

            /// <summary>
            /// Returns the nearest unit to position that is within limit and conforms to predicate
            /// </summary>
            public T Nearest(Vector2 position, float limit, Predicate<T> predicate, out float nearestDistSquared)
            {
                float limitSquared = limit * limit;
                T nearestUnit = null;

                nearestDistSquared = float.PositiveInfinity;

                foreach (var wrapper in _unitWrappers)
                {
                    float d = wrapper.Shape.DistanceSquared(position);

                    if (d < nearestDistSquared && d < limitSquared && predicate(wrapper.Unit))
                    {
                        nearestDistSquared = d;
                        nearestUnit = wrapper.Unit;
                    }
                }

                return nearestUnit;
            }

            /// <summary>
            /// Returns all units in this cell that contact shape
            /// </summary>
            public IEnumerable<T> Contact(IConvexShape shape, Predicate<T> predicate, int queryNumber)
            {
                foreach (var wrapper in _unitWrappers)
                {
                    // Make sure to check each unit only once. Certain shapes might span multiple cells.
                    if (!wrapper.Once(queryNumber))
                        continue;

                    if (!shape.NoContactCertainty(wrapper.Shape) && 
                        !wrapper.Shape.NoContactCertainty(shape) &&
                        predicate(wrapper.Unit))
                        yield return wrapper.Unit;
                }
            }
        }
    }
}