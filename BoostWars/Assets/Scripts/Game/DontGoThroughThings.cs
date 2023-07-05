// http://wiki.unity3d.com/index.php?title=DontGoThroughThings
// https://github.com/omgwtfgames/unity-bowerbird/blob/master/Scripts/General/DontGoThroughThings.cs
using UnityEngine;
using System.Collections;

public class DontGoThroughThings : MonoBehaviour
{
	public LayerMask layerMask; //make sure we aren't in this layer 

	private Vector3 prevPosition;


    private void Awake()
    {
		prevPosition = transform.position;
    }

    void FixedUpdate()
	{
		RaycastHit[] hits = Physics.RaycastAll(new Ray(prevPosition, transform.position - prevPosition), Vector2.Distance(prevPosition, transform.position));
		Debug.DrawLine(prevPosition, transform.position, Color.red, Time.deltaTime);
		foreach (RaycastHit item in hits)
		{
			if (item.collider.name.Equals("Quad"))
            {
				GetComponent<Projectile>().OnTriggerEnter(item.collider);
				transform.position = item.point;
            }
		}
		prevPosition = transform.position;
	}
}
