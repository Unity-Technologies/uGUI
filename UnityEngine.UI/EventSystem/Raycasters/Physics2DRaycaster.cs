using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Simple event system using physics raycasts.
    /// </summary>
    [AddComponentMenu("Event/Physics 2D Raycaster")]
    [RequireComponent(typeof(Camera))]
    public class Physics2DRaycaster : PhysicsRaycaster
    {
        protected Physics2DRaycaster()
        { }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (eventCamera == null)
                return;

            var ray = eventCamera.ScreenPointToRay(eventData.position);

            float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;

            var hits = Physics2D.GetRayIntersectionAll(ray, dist, finalEventMask);

            if (hits.Length != 0)
            {
                eventData.worldPosition = hits[0].point;
                eventData.worldNormal = hits[0].normal;
                for (int b = 0, bmax = hits.Length; b < bmax; ++b)
                {
                    var result = new RaycastResult
                    {
                        gameObject = hits[b].collider.gameObject,
                        module = this,
                        distance = Vector3.Distance(eventCamera.transform.position, hits[b].transform.position),
                        index = resultAppendList.Count
                    };
                    resultAppendList.Add(result);
                }
            }
        }
    }
}
