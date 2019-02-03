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

namespace SuperGrid2D
{
    /// <summary>
    /// Defines a shape for ContactGrid 
    /// 
    /// Prerequisites: 
    /// - shapes that are evaluated must lie entirely within the grid's bounds
    /// - shapes must be convex
    /// 
    /// </summary>
    public interface IConvex2D
    {
        /// <summary>
        /// Should return the shortest distance^2 from a position to the edge of this shape
        /// and zero if the point is inside this shape
        /// </summary>
        float DistanceSquared(Vector2 position);

        /// <summary>
        /// Should enumerate all cell indices that this shape overlaps with
        /// </summary>
        IEnumerable<Vector2Int> Supercover(IGridDimensions2D grid);

        /// <summary>
        /// Should return if another shape is certain to not make contact with this shape using SAT (Separating Axis Theorem)
        /// When a test is performed, both shapes are tested and if both of these return false, a collision occured
        /// 
        /// For shapes that can't perform SAT test (like a point and a circle) this function can return false using another approach 
        /// (like measuring the distance to the center). This can work in conjunction with SAT.
        /// 
        /// TODO: a priority value can be assigned to a shapes NoContactCertainty function to determine which of the two possibly colliding
        /// shapes has the least expensive function. Then that function can be evaluated first.
        /// 
        /// </summary>
        bool NoContactCertainty(IConvex2D shape);

        /// <summary>
        /// Should return this shape's projection on an axis
        /// </summary>
        void Project(Vector2 normal, ref float min, ref float max);
    }
}