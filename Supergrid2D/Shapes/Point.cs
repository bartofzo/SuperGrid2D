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
    public struct Point : IConvex2D
    {
        public readonly float x;
        public readonly float y;

        public Point(Vector2 position)
        {
            this.x = position.x;
            this.y = position.y;
        }

        public float DistanceSquared(Vector2 position)
        {
            float dX = position.x - x;
            float dY = position.y - y;

            return (dX * dX) + (dY * dY);
        }

        public bool NoContactCertainty(IConvex2D shape)
        {
            return shape.DistanceSquared(new Vector2(x, y)) > 0;
        }

        public IEnumerable<Vector2Int> Supercover(IGridDimensions2D grid)
        {
            yield return new Vector2Int(
                (int)(Math.Min(grid.Columns - 1, Math.Max(0, (x - grid.TopLeft.x) / grid.CellSize.x))),
                (int)(Math.Min(grid.Rows - 1, Math.Max(0, (y - grid.TopLeft.y) / grid.CellSize.y))));
        }

        public void Project(Vector2 normal, ref float min, ref float max)
        {
            float dot = Utility.Dot(normal, x, y);
            min = max = dot;
        }
    }
}