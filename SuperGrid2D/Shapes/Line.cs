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
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperGrid2D
{
    public struct Line : IConvex2D
    {
        public readonly Vector2 v;
        public readonly Vector2 w;

        public Line(Vector2 v, Vector2 w)
        {
            this.v = v;
            this.w = w;
        }

        public Line(float vx, float vy, float wx, float wy)
        {
            this.v = new Vector2(vx, vy);
            this.w = new Vector2(wx, wy);
        }

        /// <summary>
        /// Returns the shortest distance squared from a point to this line
        /// </summary>
        public float DistanceSquared(Vector2 position)
        {
            float lengthSquared = Utility.DistanceSquared(v, w);
            if (lengthSquared < float.Epsilon) 
                return Utility.DistanceSquared(v, position); // when we're actually a dot

            float t = (float)Mathf.Max(0, Mathf.Min(1, Vector2.Dot(position - v, w - v) / lengthSquared));
            Vector2 projection = v + ((w - v) * t);

            return Utility.DistanceSquared(position, projection);
        }

        public bool NoContactCertainty(IConvex2D shape)
        {
            float minA = 0;
            float maxA = 0;
            float minB = 0;
            float maxB = 0;

            Vector2 normal = new Vector2(-(w.y - v.y), w.x - v.x).normalized;

            shape.Project(normal, ref minA, ref maxA);
            this.Project(normal, ref minB, ref maxB);

            // Check if the polygon projections are currentlty NOT intersecting
            if (Utility.IntervalDistance(minA, maxA, minB, maxB) > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Modified code from http://playtechs.blogspot.com/2007/03/raytracing-on-grid.html
        /// 
        /// Traces the line segment and returns all cell indexes that are passed
        /// 
        /// </summary>
        public IEnumerable<Vector2Int> Supercover(IGridDimensions2D grid)
        {
            // All flooring is done with a cast to int so this will only work
            // for positive values. Which is fine for our grid since after we normalized our coords
            // to the grid there are no negative values (except when we're out of bounds, but that's not allowed)

            // Set to offset of grid and make each integer correspond to a cell
            Vector2 normalizedV = (v - grid.TopLeft) / grid.CellSize;
            Vector2 normalizedW = (w - grid.TopLeft) / grid.CellSize;

            float lineDeltaX = Math.Abs(normalizedW.x - normalizedV.x);
            float lineDeltaY = Math.Abs(normalizedW.y - normalizedV.y);

            // starting position in grid
            int x = (int) normalizedV.x;
            int y = (int) normalizedV.y;

            int totalSteps = 1;
            int xDirection, yDirection;

            float error;

            if (lineDeltaX < float.Epsilon)
            {
                xDirection = 0;
                error = float.PositiveInfinity;
            }
            else if (w.x > v.x)
            {
                xDirection = 1;
                totalSteps += (int)normalizedW.x - x;
                error = (Mathf.Floor(normalizedV.x) + 1 - normalizedV.x) * lineDeltaY;
            }
            else
            {
                xDirection = -1;
                totalSteps += x - (int)normalizedW.x;
                error = (normalizedV.x - Mathf.Floor(normalizedV.x)) * lineDeltaY;
            }

            if (lineDeltaY < float.Epsilon)
            {
                yDirection = 0;
                error -= float.PositiveInfinity;
            }
            else if (w.y > v.y)
            {
                yDirection = 1;
                totalSteps += (int)normalizedW.y - y;
                error -= (Mathf.Floor(normalizedV.y) + 1 - normalizedV.y) * lineDeltaX;
            }
            else
            {
                yDirection = -1;
                totalSteps += y - (int)normalizedW.y;
                error -= (normalizedV.y - Mathf.Floor(normalizedV.y)) * lineDeltaX;
            }

            for (; totalSteps > 0; --totalSteps)
            {
                // Only return values that are inside the grid
                // we can't clamp the line before hand because that could alter the direction of the line
                if (x >= 0 && x < grid.Columns &&
                    y >= 0 && y < grid.Rows)
                    yield return new Vector2Int(x, y);

                if (error > 0)
                {
                    y += yDirection;
                    error -= lineDeltaX;
                }
                else
                {
                    x += xDirection;
                    error += lineDeltaY;
                }
            }
        }

        public void Project(Vector2 normal, ref float min, ref float max)
        {
            float vProj = Vector2.Dot(normal, v);
            float wProj = Vector2.Dot(normal, w);

            if (vProj > wProj)
            {
                max = vProj;
                min = wProj;
            }
            else
            {
                max = wProj;
                min = vProj;
            }
        }
    }
}