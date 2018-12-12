using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperGrid2D
{
    public class ContactManager : MonoBehaviour
    {
        GridManager gridManager;

        private void Awake()
        {
            gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
        }

        private List<DemoUnit> prevContact = new List<DemoUnit>();
        
        public void DoContact(DemoUnit unit)
        {
            foreach (var c in prevContact)
                c.UnmarkAsContacted();

            prevContact.Clear();
            foreach (var u in gridManager.GridInterface.ContactExcept(unit.GetShape(), unit))
            {
                prevContact.Add(u);
                u.MarkAsContacted();
            }
        }

        public void EndContact()
        {
            foreach (var c in prevContact)
                c.UnmarkAsContacted();
            prevContact.Clear();
        }
    }
}