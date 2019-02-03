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
using System.Linq;
using UnityEngine;

namespace SuperGrid2D
{
    public class CachedShape : IConvex2D
    {
        private readonly Vector2Int[] supercover;
        private readonly IConvex2D shape;

        private HashSet<long> noContactCache = new HashSet<long>();

        public CachedShape(IGridDimensions2D grid, IConvex2D shape)
        {
            this.shape = shape;
            this.supercover = shape.Supercover(grid).ToArray();
        }

        public float DistanceSquared(Vector2 position)
        {
            return this.shape.DistanceSquared(position);
        }

        public IEnumerable<Vector2Int> Supercover(IGridDimensions2D grid)
        {
            return supercover;
        }

        public bool NoContactCertainty(IConvex2D shape)
        {
            return this.shape.NoContactCertainty(shape);
        }

        public void Project(Vector2 normal, ref float min, ref float max)
        {
            shape.Project(normal, ref min, ref max);
        }
    }
}