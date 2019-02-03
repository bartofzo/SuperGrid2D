using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperGrid2D
{
    public class DragManager : MonoBehaviour
    {
        GridManager gridManager;
        Vector2 prevMouseWorldPos;
        DemoUnit selectedUnit;

        void Start()
        {
            gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldDelta = prevMouseWorldPos - mouseWorldPos;
            prevMouseWorldPos = mouseWorldPos;

            if (Input.GetMouseButtonDown(0))
            {
                // We sample with a little circle, we could also use a point here
                // but then we would never be able to drag points because they never truly contact
                selectedUnit = gridManager.GridInterface.FirstContact(new Circle(mouseWorldPos, 0.1f));
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (selectedUnit != null)
                {
                    selectedUnit.EndDrag();
                    selectedUnit = null;


                }
            }

            if (selectedUnit != null)
                selectedUnit.Drag(mouseWorldPos);

        }
    }
}