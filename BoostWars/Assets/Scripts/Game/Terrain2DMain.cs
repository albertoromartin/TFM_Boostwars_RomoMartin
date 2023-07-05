﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain2DMain : MonoBehaviour
{
    public Vector3 scale;
    public float PixelsPerUnit = 10;
    public Texture2D texture;
    public Vector2 terrainParts;
    public GameObject terrainPrefab;
    public Terrain2D[,] terrains;
    public LayerMask ly;

    void Start()
    {
        Data.ter = this;
        SplitTerrain();

    }

    public void Dig(Vector3 pos, int radius, bool ReloadCollision)
    {
        if (radius == 1)
        {
            pos.z = -10;
            RaycastHit hit;
            Physics.Raycast(pos, transform.forward, out hit, 1000, ly);
            if (hit.transform == null)
                return;

            //print(hit.transform.name);

            Vector2 pixelUV = hit.textureCoord;

            Terrain2D t = hit.transform.parent.parent.gameObject.GetComponent<Terrain2D>();
            if (t != null)
            {
                Vector2 size = new Vector2(t.texture.width, t.texture.height);
                // print(pixelUV.x * size.x);
                t.texture.SetPixel((int)(pixelUV.x * size.x), (int)(pixelUV.y * size.y), Color.clear);
                float i = (int)(pixelUV.y * size.y) * t.texture.width + (int)(pixelUV.x * size.x);
                t.m_Colors[(int)i] = Color.clear;
                t.texture.Apply();
                if(ReloadCollision)
                    t.GenCollider();
                // t.apc.RecalculatePolygon();
                // t.apc2.RecalculatePolygon();
            }
        }
        else
        {
            pos.z = -10;
            RaycastHit hit;
            Physics.Raycast(pos, transform.forward, out hit, 1000, ly);
            if (hit.transform == null)
                return;

            if (radius % 2 > 0)
            {
                Debug.Log("Radius not dividable");
                return;
            }

            //print(hit.transform.name);

            Vector2 pixelUV = hit.textureCoord;

            Terrain2D t = hit.transform.parent.parent.gameObject.GetComponent<Terrain2D>();
            if (t != null)
            {
                Vector2 size = new Vector2(t.texture.width, t.texture.height);
                // print(pixelUV.x * size.x);

                float w = t.texture.width;
                float h = t.texture.height;
                int l = t.texture.width * t.texture.height;

                // t.apc.RecalculatePolygon();
                // t.apc2.RecalculatePolygon();
                int r = radius / 2;
                for (int y = -r; y < r; y++)
                {
                    for (int x = -r; x < r; x++)
                    {
                        int yC = ((int)(pixelUV.y * size.y) + y);
                        int xC = ((int)(pixelUV.x * size.x) + x);
                        if (yC >= 0 && xC >= 0 && xC <= w && yC <= h && (yC*w+xC) < l)
                        {
                            t.texture.SetPixel(xC, yC, Color.clear);
                            float i = yC * w + xC;
                            t.m_Colors[(int)i] = Color.clear;
                        }
                    }
                }

                t.texture.Apply();
                if(ReloadCollision)
                    t.GenCollider();

            }

        }
    }

    internal void replaceSpace(GameObject go)
    {
        String x = go.name.Substring(0, go.name.IndexOf(','));
        String y = go.name.Substring(go.name.IndexOf(',') + 1);

        Terrain2D t2d = go.GetComponent<Terrain2D>();
        t2d.termain = this;
        Sprite s = Sprite.Create(go.GetComponent<Terrain2D>().texture, new Rect(0, 0, go.GetComponent<Terrain2D>().texture.width, go.GetComponent<Terrain2D>().texture.height), new Vector2(0.5f, 0.5f), PixelsPerUnit);


        terrains[Int32.Parse(x), Int32.Parse(y)] = t2d;

        t2d.sprite.sprite = s;
        t2d.texture = go.GetComponent<Terrain2D>().texture;

        t2d.bc2d = go.AddComponent<BoxCollider2D>();
        t2d.backCollider.localScale = new Vector3(t2d.bc2d.size.x, t2d.bc2d.size.y, 1);
    }

    /**
     * <summary>
     * To see if the position collides with the terrain
     * </summary>
     */
    public bool TryDig(Vector3 pos1, Vector3 pos2)
    {
        Vector3 direction = pos1 - pos2;
        pos1.z = -10;
        direction.z = -10;
        RaycastHit hit;
        Physics.Raycast(pos1, direction, out hit, 100, ly);
        Debug.DrawRay(pos1, direction, Color.green);
        if (hit.transform == null)
        {
            Physics.Raycast(pos1, Vector3.down, out hit, 100, ly);
            if (hit.transform == null)
                return true;
        }

        Vector2 pixelUV = hit.textureCoord;
        Terrain2D t = hit.transform.parent.parent.gameObject.GetComponent<Terrain2D>();
        if (t != null)
        {
            Vector2 size = new Vector2(t.texture.width, t.texture.height);
            // print(pixelUV.x * size.x);
            Color c = t.texture.GetPixelBilinear(pos1.x, pos1.y);
            Debug.Log("Color: " + c);
            if (c.a == 0)
                return true;
        }
        return false;
    }

    void SplitTerrain()
    {

        if (terrainParts.magnitude == 0)
        {
            string dividers = "";
            for (int i = 1; i < texture.height; i++)
            {
                if ((texture.height % i) == 0)
                {
                    dividers += i.ToString();
                    dividers += ",";
                }
            }
            print("Dividers " + dividers);
        }

        if ((texture.width % terrainParts.x + texture.height % terrainParts.y) > 0)
        {
            Debug.LogError("Resolution not dividable by " + terrainParts.ToString());
            Debug.Break();
            return;
        }
        terrains = new Terrain2D[(int)terrainParts.x, (int)terrainParts.y];
        //  List<Terrain2D> terrainz = new List<Terrain2D>();
        for (int x = 0; x < terrainParts.x; x++)
        {
            for (int y = 0; y < terrainParts.y; y++)
            {
                GameObject go = Instantiate(terrainPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                go.name = x.ToString() + " , " + y.ToString();
                go.transform.parent = transform;
                go.transform.localPosition = new Vector3(x * texture.width / terrainParts.x / PixelsPerUnit, y * texture.height / terrainParts.y / PixelsPerUnit);
                go.transform.localScale = scale;
                Terrain2D t2d = go.GetComponent<Terrain2D>();
                t2d.termain = this;
                Texture2D tex = new Texture2D((int)(texture.width / terrainParts.x), (int)(texture.height / terrainParts.y));
                tex.filterMode = FilterMode.Point;
                Color[] c = texture.GetPixels((int)(x * texture.width / terrainParts.x), (int)(y * texture.height / terrainParts.y), (int)(texture.width / terrainParts.x), (int)(texture.height / terrainParts.y));
                tex.SetPixels(c);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply();
                Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
                /*
                BoxCollider2D b = go.AddComponent<BoxCollider2D>();
                Vector3 v = t2d.sprite.bounds.size;
                b.size = v;
    */

                terrains[x, y] = t2d;

                t2d.sprite.sprite = s;
                t2d.texture = tex;

                t2d.bc2d = go.AddComponent<BoxCollider2D>();
                t2d.backCollider.localScale = new Vector3(t2d.bc2d.size.x, t2d.bc2d.size.y, 1);
                Destroy(t2d.bc2d);
                //  terrainz.Add(t2d);

            }
        }

        //terrains = new Terrain2D[terrainz.Count];
        //  terrains = terrainz.ToArray();
    }
}