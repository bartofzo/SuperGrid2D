using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supergrid2D {
    public class DemoLineUnit : DemoUnit {

        Transform pointA;
        Transform pointB;
        Transform conn;

        protected override void Start()
        {

            pointA = this.transform.GetChild(0);
            pointB = this.transform.GetChild(1);
            conn = this.transform.GetChild(2);

            UpdateLineShape();

            base.Start();
        }

        public override IConvex2D GetShape()
        {
            return new Line(pointA.position, pointB.position);
        }

        public override void Drag(Vector2 worldPos)
        {
            // See to which point we're closest
            if (Utility.DistanceSquared(worldPos, pointA.position) < Utility.DistanceSquared(worldPos, pointB.position))
                pointA.position = worldPos;
            else
                pointB.position = worldPos;

            // Debug.Log("v: " + pointA.position + " w: " + pointB.position);

            UpdateLineShape();
            gridManager.UpdateShape(this.Key, this.GetShape());

            contactManager.DoContact(this);
        }

        /// <summary>
        /// Kind of hacky solution to display a line in Unity
        /// </summary>
        void UpdateLineShape()
        {
            conn.position = 0.5f * (pointA.position + pointB.position);
            float angle = Mathf.Atan2(pointA.position.y - pointB.position.y, pointA.position.x - pointB.position.x);

            var rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            conn.rotation = rotation;
            pointA.rotation = rotation;
            pointB.rotation = rotation;

            conn.localScale = new Vector3(Vector2.Distance(pointA.position, pointB.position), 0.025f, 1f);
        }
    }
}