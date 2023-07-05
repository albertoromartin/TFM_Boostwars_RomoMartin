using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

/**
* <summary>
* Previsualize Line Colliders
* </summary>
*/
public class LineManager : MonoBehaviour
{
    private int i;
    public bool triggered = false;
    private bool started = false;

    public LayerMask lm;
    internal bool ForceCollision = false;

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        i = Int32.Parse(name);
        for (int i = 0; i < GameManager.LinePointAmount; i++)
        {
            if(!(i+"").Equals(name))
                Physics.IgnoreCollision(GameManager.instance.PointsColliders.ElementAt(i).GetComponent<BoxCollider>(), GetComponent<BoxCollider>());
        }
        StartCoroutine(EnableTrigger());
    }

    private IEnumerator EnableTrigger()
    {
        yield return new WaitForFixedUpdate();
        triggered = false;
    }

    void FixedUpdate()
    {
        //Cuando es el turno de la CPU, este método se encarga de calcular su propia posición según los valores que se van calculando en el GameManager,
        //así, se ejecutan todo el código de ca da fragmento de golpe, sin ir uno a uno (como estaba antes, era mucho más lento y gastaba más recursos)
        if (GameManager.instance.turn == 3 && !started && GameManager.instance.canStart)
        {
            Time.timeScale = 20;
            started = true;
            if (GameManager.instance.actualWeaponCpu != 4)
                StartCoroutine(MoveCpu());
            else
                StartCoroutine(SniperAim());
                }
            triggered = false;
    }

    /**
    * <summary>
    * Usa trigonometría y física para calcular gracias a los valores calculados en el GameManager, su posición, ángulo y longitud
    * </summary>
    */
    private IEnumerator MoveCpu() {
        float xPos = GameManager.instance.parabolXStep * i + GameManager.instance.parabolP1.x;
        float yPos = 0.5f * (-9.8f) * math.pow(GameManager.instance.parabolTime * i, 2) + GameManager.instance.parabolVy * (GameManager.instance.parabolTime * i) + GameManager.instance.parabolP1.y;

        //Calcula el nuevo valor para actualizar la posición y ángulo del collider
        float xPosLate = (GameManager.instance.parabolXStep * (i + 1)) + GameManager.instance.parabolP1.x;
        float yPosLate = 0.5f * (-9.8f) * math.pow((GameManager.instance.parabolTime * (i + 1)), 2) + GameManager.instance.parabolVy * (GameManager.instance.parabolTime * (i + 1)) + GameManager.instance.parabolP1.y;

        float size = math.sqrt(math.pow(xPosLate - xPos, 2) + math.pow(yPosLate - yPos, 2));

        GameManager.instance.PathPoints.Add(new Vector3(xPos, yPos, 0));

        float angle = Mathf.Rad2Deg * Mathf.Atan((xPosLate - xPos) / (yPosLate - yPos));
        GameManager.instance.PointsColliders.ElementAt(i).transform.eulerAngles = new Vector3(0, 0, -angle);
        GameManager.instance.PointsColliders.ElementAt(i).transform.position = new Vector3(xPos, yPos, 0);
        GameManager.instance.PointsColliders.ElementAt(i).GetComponent<BoxCollider>().size = new Vector2(GameManager.instance.BulletTypes[0].GetComponent<SpriteRenderer>().size.y * 5, size);

        if (i == 5)
        {
            //Puntos para saber si el jugador está mirando a la izquierda, o a la derecha
            Vector3 point1 = GameManager.instance.cpu.transform.GetChild(3).position;
            Vector3 point2 = GameManager.instance.cpu.transform.GetChild(4).position;

            //Puntos para saber si el jugador está mirando arriba o abajo
            Vector3 point3 = GameManager.instance.cpu.transform.GetChild(5).position;
            Vector3 point4 = GameManager.instance.cpu.transform.GetChild(6).position;

            //Para dar la vuelta al sprite y que parezca que mira a donde apunta el jugador
            Vector2 V1 = new Vector2(point2.x - point1.x, point2.y - point1.y);
            Vector2 Point1 = point2;
            float m1 = V1.y / V1.x;
            float b1 = Point1.y - m1 * Point1.x;

            Vector2 VPerpendicular1 = new Vector2(-1 * V1.y, V1.x);
            Vector2 Point2 = new Vector2(GameManager.instance.PointsColliders.ElementAt(i).transform.position.x, GameManager.instance.PointsColliders.ElementAt(i).transform.position.y);
            float mPerpendicular1 = VPerpendicular1.y / VPerpendicular1.x;
            float bPerpendicular1 = Point2.y - mPerpendicular1 * Point2.x;

            float x1 = (b1 - bPerpendicular1) / (mPerpendicular1 - m1);

            float y1 = m1 * x1 + b1;

            Vector3 CatetoContiguo1 = GameManager.instance.PointsColliders.ElementAt(i).transform.position - new Vector3(x1, y1, GameManager.instance.PointsColliders.ElementAt(i).transform.position.z);

            Vector2 V2 = new Vector2(point4.x - point3.x, point4.y - point3.y);
            Vector2 Point3 = point4;
            float m2 = V2.y / V2.x;
            float b2 = Point3.y - m2 * Point3.x;

            Vector2 VPerpendicular2 = new Vector2(-1 * V2.y, V2.x);
            float mPerpendicular2 = VPerpendicular2.y / VPerpendicular2.x;
            float bPerpendicular2 = Point2.y - mPerpendicular2 * Point2.x;

            float x2 = (b2 - bPerpendicular2) / (mPerpendicular2 - m2);

            float y2 = m2 * x2 + b2;


            Vector3 CatetoContiguo2 = GameManager.instance.PointsColliders.ElementAt(i).transform.position - new Vector3(x2, y2, GameManager.instance.PointsColliders.ElementAt(i).transform.position.z);

            Vector3 CentralPoint = new Vector3(GameManager.instance.PointsColliders.ElementAt(0).transform.position.x, GameManager.instance.PointsColliders.ElementAt(0).transform.position.y, GameManager.instance.PointsColliders.ElementAt(0).transform.position.z);

            double num = CentralPoint.x - x1;
            float numB = CentralPoint.x - x1;

            Vector3 CatetoOpuesto = CentralPoint - new Vector3(x1, y1, CentralPoint.z);

            float num1 = Mathf.Sqrt(Mathf.Pow(CatetoOpuesto.x, 2) + Mathf.Pow(CatetoOpuesto.y, 2));
            float num2 = Mathf.Sqrt(Mathf.Pow(CatetoContiguo1.x, 2) + Mathf.Pow(CatetoContiguo1.y, 2));

            float InclinationAngle = Mathf.Atan(num1 / num2);

            float VaritationAngle = GameManager.instance.cpu.transform.rotation.eulerAngles.z * Mathf.Deg2Rad < 0.001f ? 0f : GameManager.instance.cpu.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;

            //Guess the best weapon inclintaion
            switch ((CatetoContiguo2.y, CatetoContiguo1.x))
            {
                case ( < 0f, < 0f):
                    InclinationAngle += VaritationAngle;
                    GameManager.instance.cpu.transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
                    GameManager.instance.cpu.transform.GetChild(2).transform.eulerAngles = new Vector3(0, 0, InclinationAngle * Mathf.Rad2Deg);
                    break;
                case ( < 0f, >= 0f):
                    InclinationAngle -= VaritationAngle;
                    GameManager.instance.cpu.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    GameManager.instance.cpu.transform.GetChild(2).transform.eulerAngles = new Vector3(0, 0, 90 - ((InclinationAngle * Mathf.Rad2Deg) - 270));
                    break;
                case ( >= 0f, < 0f):
                    InclinationAngle -= VaritationAngle;
                    GameManager.instance.cpu.transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
                    GameManager.instance.cpu.transform.GetChild(2).transform.eulerAngles = new Vector3(0, 0, 90 - ((InclinationAngle * Mathf.Rad2Deg) - 270));
                    break;
                case ( >= 0f, >= 0f):
                    InclinationAngle += VaritationAngle;
                    GameManager.instance.cpu.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    GameManager.instance.cpu.transform.GetChild(2).transform.eulerAngles = new Vector3(0, 0, InclinationAngle * Mathf.Rad2Deg);
                    break;
            }
        }
        yield return new WaitWhile(() => GameManager.instance.PointsColliders.ElementAt(i).transform.position != new Vector3(xPos, yPos, 0));
        Time.timeScale = 1;
        started = false;
    }

    private IEnumerator SniperAim()
    {
        Vector2 v = GameManager.instance.cpu.transform.position - GameManager.instance.player.transform.position;
        if(v.x > 0)
            GameManager.instance.cpu.transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
        else
            GameManager.instance.cpu.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        started = false;
        yield break;
    }

    /**
    * <summary>
    * Como solo se quiere que sean trigereados por el terreno, se hace que solo reaccionen al mismo
    * </summary>
    */
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.name.Equals("Quad"))
        {
            triggered = true;
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (!triggered)
        {
            if (collision.name.Equals("Quad"))
            {
                triggered = true;
            }
        }
    }

    internal bool IsTriggered()
    {
        return triggered;
    }
}
