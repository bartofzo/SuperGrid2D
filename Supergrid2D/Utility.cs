using System.Runtime.CompilerServices;
using UnityEngine;

namespace SuperGrid2D
{
    public static class Utility 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Vector2 a, Vector2 b)
        {
            Vector2 d = a - b;
            return (d.x * d.x) + (d.y * d.y);
        }

        /// <summary>
        /// A helper function that does a dot product where v is op Vector2 type but the other Vector is supplied with 2 float values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector2 v, float x2, float y2)
        {
            return v.x * x2 + v.y * y2;
        }

        // Calculate the distance between [minA, maxA] and [minB, maxB]
        // The distance will be negative if the intervals overlap
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            return minA < minB ? minB - maxA : minA - maxB;
        }
    }
}