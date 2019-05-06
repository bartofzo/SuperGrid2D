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
    /// Axis aligned rectangle
    /// </summary>
    public struct AABB : IConvex2D
    {
        public readonly Vector2 topLeft;
        public readonly Vector2 bottomRight;

        public AABB(Vector2 topLeft, float width, float height)
        {
            this.topLeft = topLeft;
            this.bottomRight = new Vector2(topLeft.x + width, topLeft.y + height);
        }

        public AABB(Vector2 topLeft, Vector2 bottomRight)
        {
            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
        }

        public float DistanceSquared(Vector2 position)
        {
            float dX = position.x - Math.Max(Math.Min(position.x, bottomRight.x), topLeft.x);
            float dY = position.y - Math.Max(Math.Min(position.y, bottomRight.y), topLeft.y);

            return Math.Max(0, (dX * dX) + (dY * dY));
        }

        public IEnumerable<Vector2Int> Supercover(IGridDimensions2D grid)
        {
            int minX = (int)Math.Max(0, (topLeft.x - grid.TopLeft.x) / grid.CellSize.x);
            int minY = (int)Math.Max(0, (topLeft.y - grid.TopLeft.y) / grid.CellSize.y);

            int maxX = (int)Math.Min(grid.Columns - 1, (bottomRight.x - grid.TopLeft.x) / grid.CellSize.x);
            int maxY = (int)Math.Min(grid.Rows - 1, (bottomRight.y - grid.TopLeft.y) / grid.CellSize.y);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                    yield return new Vector2Int(x, y);
            }
        }

        // Calculate the projection of this shape on an axis
        // and returns it as a [min, max] interval
        public void Project(Vector2 normal, ref float min, ref float max)
        {
            // First corner
            float proj = Vector2.Dot(normal, topLeft);
            min = max = proj;

            // Other corners
            proj = Utility.Dot(normal, bottomRight.x, topLeft.y);
            if (proj < min)
                min = proj;
            else if (proj > max)
                max = proj;

            proj = Vector2.Dot(normal, bottomRight);
            if (proj < min)
                min = proj;
            else if (proj > max)
                max = proj;
                
            //proj = Utility.Dot(normal, topLeft.x, bottomRight.y);
            //if (proj < min)
            //    min = proj;
            //else if (proj > max)
                //max = proj;
        }

        public bool NoContactCertainty(IConvex2D shape)
        {
            float minA = 0;
            float maxA = 0;
            float minB = 0;
            float maxB = 0;

            // Only check up & right normals for an AABB since down & left are the same axes

            shape.Project(Vector2.up, ref minA, ref maxA);
            this.Project(Vector2.up, ref minB, ref maxB);

            // Check if the polygon projections are currentlty NOT intersecting
            if (Utility.IntervalDistance(minA, maxA, minB, maxB) > 0)
                return true;

            shape.Project(Vector2.right, ref minA, ref maxA);
            this.Project(Vector2.right, ref minB, ref maxB);

            // Check if the polygon projections are currentlty NOT intersecting
            if (Utility.IntervalDistance(minA, maxA, minB, maxB) > 0)
                return true;

            return false;
        }
    }
}