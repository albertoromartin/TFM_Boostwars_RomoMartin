using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int power;
    bool AlreadyTriggered = false;
    bool ActiveCollisions = false;
    private bool CanDamage = true;
    internal Vector3 ShooterPosition;

    internal string creatorTag;

    [Header("Damage Values")]
    public int[] Damage;
    public float[] Radio;

    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(10, 8, true);
        StartCoroutine(AudioAux());
    }

    void Update()
    {
        float angle;
        Vector3 velocity = GetComponent<Rigidbody>().velocity;
        float hypotenuse = Mathf.Pow(velocity.x, 2) + Mathf.Pow(velocity.y, 2);
        if (hypotenuse != 0)
        {
            if (transform.position.x > ShooterPosition.x)
            {
                float RadAngle = Mathf.Asin(velocity.y / Mathf.Sqrt(hypotenuse));
                angle = RadAngle * Mathf.Rad2Deg;
            }
            else
            {
                float RadAngle = Mathf.Asin(velocity.y / Mathf.Sqrt(hypotenuse));
                angle = (-RadAngle * Mathf.Rad2Deg) + 180;
            }
        }
        else
            angle = 270;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    private IEnumerator AudioAux()
    {
        yield return new WaitForFixedUpdate();
        AudioManager.instance.playSound("launch");
        yield break;
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "Player" && collision.gameObject.tag != "CPU" && collision.gameObject.tag != "GameVoid" && !AlreadyTriggered && collision.gameObject.tag != "WeaponSpawner")
        {
            AlreadyTriggered = true;
            StartCoroutine(Triggered());
        }

        if((((collision.gameObject.tag == creatorTag && ActiveCollisions)) || ((collision.gameObject.tag == "Player" && collision.gameObject.tag != creatorTag) || (collision.gameObject.tag == "CPU" && collision.gameObject.tag != creatorTag))) && collision.gameObject.tag != "GameVoid" && !AlreadyTriggered)
        //if (((collision.gameObject.tag == "Player" || collision.gameObject.tag == "CPU") && ActiveCollisions) && collision.gameObject.tag != "GameVoid" && !AlreadyTriggered)
        {
            collision.gameObject.GetComponent<Player>().health -= Damage[Damage.Length - 1];
            AlreadyTriggered = true;
            StartCoroutine(Triggered());
        }
    }

    private IEnumerator Triggered()
    {
        if (CanDamage)
        {
            Time.timeScale = 1;
            StartCoroutine(GameManager.instance.explosionCalculator(power, transform.position));
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            GetComponent<Collider>().enabled = false;
            //GetComponent<CapsuleCollider>().enabled = false;
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>().ChangeFollowObject(transform.position);

            CreateDamageColliders();
        }
        else
        {
            GameObject.Destroy(this.gameObject);
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>().ChangeFollowObject(transform.position);
        }
        yield break;
    }

    private void CreateDamageColliders()
    {
        //Hijo calclador de danio
        GameObject explosionCollitions = transform.GetChild(1).gameObject;
        for (int i = 0; i < Radio.Length; i++)
        {
            GameObject collition = new GameObject();
            collition.name = i + "";
            Rigidbody rb = collition.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.isKinematic = true;
            SphereCollider sc = collition.AddComponent<SphereCollider>();
            sc.radius = Radio[i] / 1000 * power;
            ExplosionDamager ed = collition.AddComponent<ExplosionDamager>();
            ed.position = i;
            collition.transform.position = explosionCollitions.transform.position;
            collition.transform.parent = explosionCollitions.transform;
        }
        StartCoroutine(Destroy());
    }

    private IEnumerator Destroy()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        GameObject.Destroy(this.gameObject);
        yield break;
    }

    //dont mess with 1st collision
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "CPU")
        {
            ActiveCollisions = true;
            GetComponent<SpriteRenderer>().enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (collision.gameObject.tag == "GameVoid")
        {
            StartCoroutine(Triggered());
        }
    }

}
