using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CpuManager : MonoBehaviour
{
    public float length;
    public Transform p1;
    public Transform p2;
    public bool flying = false;
    public bool fstTime = true;
    public bool onParabolTop = false;
    public int weaponTier = 0;

    void Update()
    {
        bool rch = Raycast(p1.position, -this.gameObject.transform.up, length);
        bool rch2 = Raycast(p2.position, -this.gameObject.transform.up, length);
    }

    bool Raycast(Vector2 position, Vector2 rayDirection, float length)
    {

        RaycastHit[] hit = Physics.RaycastAll(position, rayDirection, length, GameManager.instance.ground);
        Color c = Color.red;

        bool collide = false;

        foreach (RaycastHit item in hit)
        {
            if (item.collider.gameObject.name.Equals("Quad"))
            {
                collide = true;
                c = Color.green;
            }
        }
        Debug.DrawRay(position, rayDirection * length, c);
        
        return collide;
    }

    public IEnumerator stopCPU()
    {
        yield return new WaitForSeconds(0.25f);
        flying = true;
        while (flying)
        {
            if (GetComponent<Rigidbody>().velocity.y < 0.05f && GetComponent<Rigidbody>().velocity.y > -0.05f)
            {
                if (fstTime)
                {
                    onParabolTop = true;
                    fstTime = false;
                }
                if (!onParabolTop)
                {
                    flying = false;
                    GetComponent<Rigidbody>().drag = 2;
                }
            }
            else
            {
                onParabolTop = false;
            }
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(1.0f);
        fstTime = true;
        GetComponent<Rigidbody>().drag = 0;

        yield return new WaitForSeconds(3f);
        Time.timeScale = 1f;
        GameManager.instance.cpuHasMove = true;
        yield break;
    }
}
