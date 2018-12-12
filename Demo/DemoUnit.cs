using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Supergrid2D
{
    /// <summary>
    /// Everything except line
    /// </summary>
    public class DemoUnit : MonoBehaviour
    {
        public DemoUnitShape ShapeType;

        public enum DemoUnitShape
        {
            Point,
            Line,
            AABB,
            Circle
        }

        public int Key { get; private set; }

        public virtual Color Color
        {
            get
            {
                return spriteRenderers[0].color;
            }
            set
            {
                foreach (var r in spriteRenderers)
                    r.color = value;
            }
        }


        protected ContactManager contactManager;
        protected GridManager gridManager;
        protected SpriteRenderer[] spriteRenderers;

        protected virtual void Start()
        {
            gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
            contactManager = GameObject.Find("ContactManager").GetComponent<ContactManager>();

            // Add myself to the grid
            Key = gridManager.NewKey;
            gridManager.Add(this);

            this.spriteRenderers = this.GetComponentsInChildren<SpriteRenderer>();
            Color = new Color(0, 0, 0, 0.5f);
        }

        public virtual IConvex2D GetShape()
        {
            switch (ShapeType)
            {
                case DemoUnitShape.AABB:
                    return new AABB(transform.position - transform.localScale / 2, transform.position + transform.localScale / 2);
                case DemoUnitShape.Circle:
                    return new Circle(transform.position, transform.localScale.x / 2);
                case DemoUnitShape.Point:
                    return new Point(transform.position);
                default:
                    return new Point(transform.position);
            }
        }

        bool isNearest;
        bool isContacted;
        bool isDragging;

        public void MarkAsNearest()
        {
            isNearest = true;
            PickColor();
        }

        public void UnmarkAsNearest()
        {
            isNearest = false;
            PickColor();
        }

        public void MarkAsContacted()
        {
            isContacted = true;
            this.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            PickColor();
        }

        public void UnmarkAsContacted()
        {
            isContacted = false;
            this.transform.position = new Vector3(transform.position.x, transform.position.y, -1);
            PickColor();
        }

        private void PickColor()
        {
            if (isDragging)
            {
                Color = new Color(0, 0, 0, 0.25f);
            }
            else if (isContacted)
            {
                Color = new Color(0, 0, 1, 0.5f);
            }
            else if (isNearest)
            {
                Color = new Color(0, 0, 0, 0.75f);
            }
            else
            {
                Color = new Color(0, 0, 0, 0.5f);
            }
        }

        public virtual void Drag(Vector2 worldPos)
        {
            isDragging = true;
            transform.position = worldPos;

            gridManager.UpdateShape(this.Key, this.GetShape());
            contactManager.DoContact(this);

            PickColor();
        }

        public void EndDrag()
        {
            isDragging = false;
            PickColor();
            contactManager.EndContact();
        }

    }
}