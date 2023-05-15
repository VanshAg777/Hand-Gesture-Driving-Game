using ProceduralRoadTool;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxColliderExclusionZone : MonoBehaviour
{
    [SerializeField] private List<GameObject> filters = new List<GameObject>();

    List<GameObject> m_objectsInside = new List<GameObject>();
    private BoxCollider m_collider;
    private ProceduralObject m_proceduralObject;

    // Start is called before the first frame update
    void Start()
    {
        GameObject generator = null;
        m_proceduralObject = GetComponent<ProceduralObject>();
        if (m_proceduralObject != null)
            generator = m_proceduralObject.generator;

        m_collider = GetComponent<BoxCollider>();
        var objects = GameObject.FindObjectsOfType<ProceduralObject>();
        foreach (var obj in objects.Where(obj => obj != null && obj != this.gameObject && (generator == null || obj.generator != generator)))
        {
            if (CanFilter(obj.gameObject) && IsWithinBoxBounds(obj.transform.position, m_collider))
                m_objectsInside.Add(obj.gameObject);
        }

        SetObjectsVisibility(false);
    }

    void SetObjectsVisibility(bool isVisible)
	{
        foreach (var go in m_objectsInside)
            go.SetActive(isVisible);
    }

    bool CanFilter(GameObject go)
	{
        if (filters.Count == 0 || go == null)
            return true;

        foreach (var obj in filters)
            if (go.name.Contains(obj.name))
                return true;

        return false;
	}
    
	//private void OnDrawGizmos()
	//{
 //       Gizmos.color = Color.red;
	//    foreach(var go in m_objectsInside)
	//	{
 //           Gizmos.DrawSphere(go.transform.position, 0.1f);
	//	}
	//}

	[ContextMenu("check")]
    public void CheckForObjectsInside()
    {
        m_objectsInside.Clear();
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
            return;

        var objects = GameObject.FindObjectsOfType<ProceduralObject>();
        foreach(var obj in objects.Where(obj => obj != null))
		{
            if (IsWithinBoxBounds(obj.transform.position, boxCollider))
                m_objectsInside.Add(obj.gameObject);
		}
    }


    ///<summary>
    ///returns true if the point is within our BoxCollider
    ///</summary>
    public static bool IsWithinBoxBounds(Vector3 point, BoxCollider boxCollider)
    {
        point = boxCollider.transform.InverseTransformPoint(point) - boxCollider.center;

        float halfX = (boxCollider.size.x * 0.5f);
        float halfY = (boxCollider.size.y * 0.5f);
        float halfZ = (boxCollider.size.z * 0.5f);
        if (point.x < halfX && point.x > -halfX &&
           point.y < halfY && point.y > -halfY &&
           point.z < halfZ && point.z > -halfZ)
            return true;
        else
            return false;
    } //end func IsWithinBoxBounds

    ///<summary>
    ///returns all colliders within BoxCollider
    ///</summary>
    public static Collider[] BoxColliderOverlaps(BoxCollider target, int layerMask, bool hitTriggers)
    {
        return Physics.OverlapBox(target.transform.position + target.center, target.size * 0.5f, target.transform.rotation, layerMask, hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);

    } //end func BoxColliderOverlaps
}
