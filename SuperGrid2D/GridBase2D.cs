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

namespace SuperGrid2D
{
    /// <summary>
    /// Interface that shapes use to determine their supercover
    /// </summary>
    public interface IGridDimensions2D
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
    public interface IGrid2D<T> : IGridDimensions2D
    {
        int Count { get; }

        T FirstContact(IConvex2D shape);
        T FirstContactWhich(IConvex2D shape, Predicate<T> predicate);

        IEnumerable<T> Contact(IConvex2D shape);
        IEnumerable<T> ContactWhich(IConvex2D shape, Predicate<T> predicate);

        T GetNearest(Vector2 position);
        T GetNearest(Vector2 position, float maxDistance);
        T GetNearestWhich(Vector2 position, Predicate<T> predicate);
        T GetNearestWhich(Vector2 position, float maxDistance, Predicate<T> predicate);
    }

    public abstract class GridBase2D<T, TCell> : IGrid2D<T> where TCell : GridBase2D<T, TCell>.CellBase
    {
        public int Count { get; protected set; } // Implementations should keep track of count except on base.Clear() count is always set to zero
        public Vector2 TopLeft { get; private set; }
        public Vector2 CellSize { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }

        public float AverageUnitsPerCell { get; private set; }
        public float AverageCellsSearched { get; private set; }

        protected TCell[,] _cells;
        protected abstract TCell _createNewCell(Vector2Int location);

        private readonly float _diagonal; // is the maximum distance units can be away from each other
        private int _queryNumber; // number is incremented each query and unitwrappers check this number to prevent duplicate results for units that span multiple cells

        /// <summary>
        /// Constructs a Grid
        /// An optimal cellSize would be around double the average query radius
        /// </summary>
        protected GridBase2D(Vector2 topLeft, float width, float height, Vector2 cellSize)
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

        protected GridBase2D(Vector2 center, float radius, float cellSize) : this(new Vector2(center.x - radius, center.y - radius), 2 * radius, 2 * radius, new Vector2(cellSize, cellSize))
        {
        }

        protected GridBase2D(Vector2 topLeft, float width, float height, float cellSize) : this(topLeft, width, height, new Vector2(cellSize, cellSize))
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
        protected IEnumerable<TCell> _getOrCreateSupercover(IConvex2D shape)
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
        /// Finds nearest neighbour within maxDistance for which a predicate evaluates to true
        /// </summary>
        protected T _nearest(Vector2 position, float maxDistance, Predicate<T> predicate, out float distSquared)
        {
            UnitWrapper nearestWrapper = null;
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
                    nearestWrapper = nearestInCell;
                }
            };

            // Keep searching and doubling our radius until we've found a unit or when we've searched beyond the limit
            while (nearestWrapper == null && radius <= maxDistance)
            {
                foreach (Vector2Int cellIndex in new Circle(position, radius).Supercover(this))
                {
                    if (_cells[cellIndex.x, cellIndex.y] != null)
                        handleCell(_cells[cellIndex.x, cellIndex.y]);
                }

                radius *= 2;
            }

            // In case our radius expanded beyond the limit we need to search one more time exactly at limit
            if (radius > maxDistance && nearestWrapper == null)
            {
                radius = maxDistance;

                foreach (Vector2Int cellIndex in new Circle(position, radius).Supercover(this))
                    if (_cells[cellIndex.x, cellIndex.y] != null)
                        handleCell(_cells[cellIndex.x, cellIndex.y]);
            }

            distSquared = nearestDist;
            return nearestWrapper == null ? default(T) : nearestWrapper.Unit;
        }

        /// <summary>
        /// Returns the nearest unit that is found from position. 
        /// Beware that this method is potentially costly when the nearest unit is very far outside of the cell size.
        /// Use the overload with maxDistance instead for faster operation. Position must be within bounds.
        /// </summary>
        public T GetNearest(Vector2 position)
        {
            float d;
            return _nearest(position, _diagonal, (u) => true, out d);
        }

        /// <summary>
        /// Returns the nearest unit that is found from position within a given maximum distance
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
        /// Returns the nearest unit for which a predicate evaluates to true
        /// </summary>
        public T GetNearestWhich(Vector2 position, float maxDistance, Predicate<T> predicate)
        {
            float d;
            return _nearest(position, maxDistance, predicate, out d);
        }

        /// <summary>
        /// Returns the first object found that contacts shape. It might speak an alien language to us humans.
        /// </summary>
        public T FirstContact(IConvex2D shape)
        {
            _queryNumber++;

            int cellsSearched = 0;

            foreach (Vector2Int cellIndex in shape.Supercover(this))
            {
                cellsSearched++;
                if (_cells[cellIndex.x, cellIndex.y] != null)
                {
                    foreach (var wrapper in _cells[cellIndex.x, cellIndex.y].Contact(shape, (u) => true, _queryNumber))
                        return wrapper.Unit;
                }
            }

            AverageCellsSearched = Mathf.Lerp(AverageCellsSearched, cellsSearched, 0.5f);
            return default(T);
        }

        /// <summary>
        /// Returns the first object found that contacts shape for which a predicate evaluates to true. It might speak an alien language to us humans.
        /// </summary>
        public T FirstContactWhich(IConvex2D shape, Predicate<T> predicate)
        {
            _queryNumber++;

            foreach (Vector2Int cellIndex in shape.Supercover(this))
            {
                if (_cells[cellIndex.x, cellIndex.y] != null)
                {
                    foreach (var wrapper in _cells[cellIndex.x, cellIndex.y].Contact(shape, predicate, _queryNumber))
                        return wrapper.Unit;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Enumarates all objects that overlap with shape
        /// </summary>
        public IEnumerable<T> Contact(IConvex2D shape)
        {
            // Increment our query number to prevent yielding a unit multiple times if it spans more than one cell
            _queryNumber++;

            foreach (Vector2Int cellIndex in shape.Supercover(this))
            {
                if (_cells[cellIndex.x, cellIndex.y] != null)
                {
                    foreach (var wrapper in _cells[cellIndex.x, cellIndex.y].Contact(shape, (u) => true, _queryNumber))
                        yield return wrapper.Unit;
                }
            }
        }

        /// <summary>
        /// Enumarates all objects that overlap with a convex shape and for which a predicate evaluates to true
        /// </summary>
        public IEnumerable<T> ContactWhich(IConvex2D shape, Predicate<T> predicate)
        {
            // Increment our query number to prevent yielding a unit multiple times if it spans more than one cell
            _queryNumber++;
            int cellsSearched = 0;
            int unitsAcc = 0;

            foreach (Vector2Int cellIndex in shape.Supercover(this))
            {
                cellsSearched++;
                if (_cells[cellIndex.x, cellIndex.y] != null)
                {
                    unitsAcc += _cells[cellIndex.x, cellIndex.y].Count;
                    foreach (var wrapper in _cells[cellIndex.x, cellIndex.y].Contact(shape, predicate, _queryNumber))
                        yield return wrapper.Unit;
                }
            }

            AverageCellsSearched = Mathf.Lerp(AverageCellsSearched, cellsSearched, 0.5f);
            AverageUnitsPerCell = (float)unitsAcc / cellsSearched;
        }


        /// <summary>
        /// Base wrapper for units that are inside a cell
        /// </summary>
        public class UnitWrapper
        {
            public readonly T Unit;
            public readonly IConvex2D Shape;

            private int _lastQueryNumber; // Keeps track of the last query number that has been made to prevent duplicate results

            public UnitWrapper(T unit, IConvex2D shape)
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
            public abstract int Count { get; }

            /// <summary>
            /// Returns the nearest unit wrapper to position that is within limit and conforms to predicate
            /// </summary>
            public UnitWrapper Nearest(Vector2 position, float limit, Predicate<T> predicate, out float nearestDistSquared)
            {
                float limitSquared = limit * limit;
                UnitWrapper nearestWrapper = null;

                nearestDistSquared = float.PositiveInfinity;

                foreach (var wrapper in _unitWrappers)
                {
                    float d = wrapper.Shape.DistanceSquared(position);

                    if (d < nearestDistSquared && d < limitSquared && predicate(wrapper.Unit))
                    {
                        nearestDistSquared = d;
                        nearestWrapper = wrapper;
                    }
                }

                return nearestWrapper;
            }

            /// <summary>
            /// Returns all units in this cell that contact shape
            /// </summary>
            public IEnumerable<UnitWrapper> Contact(IConvex2D shape, Predicate<T> predicate, int queryNumber)
            {
                foreach (var wrapper in _unitWrappers)
                {
                    // Make sure to check each unit only once. Certain shapes might span multiple cells.
                    if (!wrapper.Once(queryNumber))
                        continue;

                    if (!shape.NoContactCertainty(wrapper.Shape) &&
                        !wrapper.Shape.NoContactCertainty(shape) &&
                        predicate(wrapper.Unit))
                        yield return wrapper;
                }
            }
        }
    }
}