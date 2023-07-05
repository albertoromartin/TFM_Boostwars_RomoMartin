using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeManager : MonoBehaviour
{
    
    List<GameObject> nukeColliders = new List<GameObject>();
    GameObject nukeParticle;

    public int power = 20;

    [Header("Damage Values")]
    public int[] Damage;
    public float[] Radio;
    // Start is called before the first frame update
    void OnEnable()
    {
        nukeParticle = GameObject.FindWithTag("NukeParticle");
        int numExplosion = 20;
        Vector2 miniSize = Data.ter.terrains[0, 0].gameObject.transform.GetChild(0).localScale;
        Vector2 dimension = new Vector2(miniSize.x * Data.ter.terrainParts.x, miniSize.y * Data.ter.terrainParts.y);
        float x;
        float y;
        for (int i = 0; i < numExplosion; i++)
        {
            x = Random.Range(0f, dimension.x) - miniSize.x / 2;
            y = Random.Range(0f, dimension.y) - miniSize.y / 2;
            GameObject explosionCollitions = new GameObject(i + "");
            explosionCollitions.transform.position = new Vector3(x, y, 0f);
            explosionCollitions.transform.parent = this.transform;
            for (int j = 0; j < Radio.Length; j++)
            {
                GameObject collition = new GameObject();
                collition.name = j + "";
                Rigidbody rb = collition.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.isKinematic = true;
                SphereCollider sc = collition.AddComponent<SphereCollider>();
                sc.radius = Radio[j] / 1000 * power;
                NukeExplosionDamager ed = collition.AddComponent<NukeExplosionDamager>();
                ed.position = j;
                collition.transform.position = explosionCollitions.transform.position;
                collition.transform.parent = explosionCollitions.transform;
            }
            StartCoroutine(GameManager.instance.explosionCalculator(power, new Vector2(x, y)));
        }
        nukeParticle.GetComponent<ParticleSystem>().Play();
        StartCoroutine(setAllActive());
        
    }

    private IEnumerator setAllActive()
    {
        yield return new WaitForSeconds(2.2f);
        if(GameManager.instance.turn == 0)        {
            GameManager.instance.mainCamera.GetComponent<CameraManager>().ChangeFollowObject(GameManager.instance.player , GameManager.instance.player);
        }
        else if(GameManager.instance.turn == 2)
        {
            GameManager.instance.mainCamera.GetComponent<CameraManager>().ChangeFollowObject(GameManager.instance.cpu, GameManager.instance.cpu);
        }
        yield return new WaitForSeconds(2.5f);
        GameManager.instance.nukeLock = false;
        yield return StartCoroutine(DestroyCollisions());
        this.enabled = false;
    }

    private IEnumerator DestroyCollisions()
    {
        nukeColliders.RemoveRange(0, nukeColliders.Count);
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject go = transform.GetChild(i).gameObject;
            GameObject.Destroy(go);
        }
        yield return null;
    }
}
