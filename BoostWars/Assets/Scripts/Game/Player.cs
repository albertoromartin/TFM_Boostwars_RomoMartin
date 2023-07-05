using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody rb;
    public LayerMask lm;
    public Camera cam;
    public GameObject lr;
    private bool MoveAngle = false;
    private bool CoroutineStarted = false;

    [Header("Player Status")]
    public int health = 250;
    public bool dead = false;

    void Start () {
        rb.centerOfMass = new Vector3(0, -0.7f, 0);
	}

    void Update ()
    {
        if (GetComponent<Rigidbody>().velocity.x < 0.1f && GetComponent<Rigidbody>().velocity.y < 0.1f && GetComponent<Rigidbody>().velocity.z < 0.1f && (transform.eulerAngles.z > 50 || transform.eulerAngles.z < -50))
        {
            MoveAngle = true;
            if (!CoroutineStarted && !dead)
            {
                CoroutineStarted = true;
                StartCoroutine(WaitMoveAngle());
            }
        }
        else
        {
            MoveAngle = false;
        }

    }

    public LineRenderer getLineRenderer()
    {
        return lr.GetComponent<LineRenderer>();
    }

    //Alingn sprite
    private IEnumerator WaitMoveAngle()
    {
        yield return new WaitForSeconds(1f);
        if (MoveAngle)
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            float MoveAngle;
            if (transform.eulerAngles.z < 180)
            {
                MoveAngle = transform.eulerAngles.z / 50;
            }
            else
            {
                MoveAngle = (transform.eulerAngles.z - 180) / 50;
            }
            for (int i = 0; i < 30; i++)
            {
                if (transform.eulerAngles.z > 50 && transform.eulerAngles.z < 150)
                {
                    transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z - MoveAngle);
                }
                else if (transform.eulerAngles.z > 230 && transform.eulerAngles.z < 330)
                {
                    transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z + MoveAngle);
                }
                yield return new WaitForSeconds(0.01f);
            }
        }
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

        CoroutineStarted = false;
    }

    internal void Die()
    {
        dead = true;
        GameManager.instance.turn = 7;
        transform.GetChild(2).gameObject.AddComponent<Rigidbody>();
        rb.AddRelativeTorque(new Vector3(0, 0, 5), ForceMode.VelocityChange);

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("GameVoid"))
        {
            health = 0;
            dead = true;
            GameManager.instance.turn = 7;
            Die();
        }
    }
}
