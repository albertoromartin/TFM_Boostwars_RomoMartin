using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgMovement : MonoBehaviour
{
    [RangeAttribute(0.5f,2.5f)]
    public float speed = 1;
    [RangeAttribute(2f, 3f)]
    public float radius = 5f;
    public float prevAngle = 0f;

    private void Start()
    {
        speed = Random.Range(0.5f, 2.5f);
        radius = Random.Range(2f, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        float catetoOpuesto = Mathf.Sin(prevAngle * Mathf.Deg2Rad) * radius;
        float catetoContiguo = Mathf.Cos(prevAngle * Mathf.Deg2Rad) * radius;
        transform.position = new Vector3(catetoContiguo, catetoOpuesto, 0f);
        prevAngle += Time.deltaTime * 5f * speed;
    }
}
