using UnityEngine;

namespace Mkey
{
    public class RayCaster : MonoBehaviour
    {
        public SlotSymbol Symbol { get; private set; }
        #region temp vars
        private SlotController controller;
        private SlotGroupBehavior sGB;
        #endregion temp vars

        private void Start()
        {
            controller = GetComponentInParent<SlotController>();
            sGB = GetComponentInParent<SlotGroupBehavior>();

            if (controller)
            {
                controller.StartSpinEvent += StartSpinEventHandler;
                controller.EndSpinEvent += EndSpinEventHandler;
            }
        }

        private void OnDestroy()
        {
            if (controller)
            {
                controller.StartSpinEvent -= StartSpinEventHandler;
                controller.EndSpinEvent -= EndSpinEventHandler;
            }
        }

        public int ID { get; set; } // for calcs

        private void StartSpinEventHandler()
        {
            Symbol = null;
        }

        private void EndSpinEventHandler()
        {
            //Collider2D hit = Physics2D.OverlapPoint(new Vector2(transform.position.x, transform.position.y));
            //Symbol = (hit) ? hit.GetComponent<SlotSymbol>() : null;

            Collider2D[] hits = Physics2D.OverlapPointAll(new Vector2(transform.position.x, transform.position.y));

            float dist = 0;
            SlotSymbol s = null;
            foreach (var item in hits)
            {
                SlotSymbol t = item.GetComponent<SlotSymbol>();
                if (!s && t)
                {
                    s = t;
                    dist = Vector3.Magnitude(t.transform.position - transform.position);
                }
                else if (t)
                {
                    float d = Vector3.Magnitude(t.transform.position - transform.position);
                    if (d < dist)
                    {
                        s = t;
                        dist = d;
                    }
                }
            }
            Symbol = s;
           // Debug.Log(name + " ; symb: " + ((Symbol != null) ? Symbol.Icon.ToString() : ""));

        }
    }
}