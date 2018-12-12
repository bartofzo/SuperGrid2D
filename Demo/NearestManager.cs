using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Supergrid2D
{
    public class NearestManager : MonoBehaviour
    {
        public GridManager gridManager;
        private DemoUnit prevNearest;

        // Update is called once per frame
        void Update()
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var nearest = gridManager.GridInterface.GetNearest(mouseWorldPos);

            if (prevNearest != null)
            {
                prevNearest.UnmarkAsNearest();
            }

            if (nearest != null)
            {
                nearest.MarkAsNearest();
                prevNearest = nearest;
            }
        }
    }
}