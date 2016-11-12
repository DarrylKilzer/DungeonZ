/*********************************************************************************/
/* Dungeon Generator
/* - version 1.01
/* - author Sergey Dotsenko
/* - created 26.08.2016
/*********************************************************************************/
using UnityEngine;
using System.Collections;

public class Dungeon : MonoBehaviour
{

    public GameObject dan_1;
    public GameObject dan_2;
    public GameObject dan_3;
    public GameObject dan_4;
    public GameObject dan_5;
    public GameObject dan_6;
    public GameObject dan_7;
    public GameObject dan_8;
    public GameObject dan_9;
    public GameObject dan_10;
    public GameObject dan_11;
    public GameObject dan_12;
    public GameObject dan_13;
    public GameObject dan_14;

    public GameObject[] around;
    public GameObject[] ground;
    public GameObject[] random;


    public int SIZE_X = 32, SIZE_Y = 32;
    public int iRND = 0;
    public int density;

    public Texture2D miniMapBackTex;
    public Texture2D miniMapArrowTex;

    private Vector3 entry;
    private Vector3 exit;

    private int[,] map; // flags(bit) 1-left wall 2-up wall 4-right wall 8-down wall
    private int[,] imap;
    private bool active;

    // Use this for initialization
    void Start()
    {
        density = 10;
        //Debug.Log(density);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnGUI()
    {
    }

    /*********************************************************************************/
    /* Drawing dungeon scheme
    /*********************************************************************************/
    public void drawMap(Vector3 pos, Quaternion rot)
    {
        int max = (SIZE_X > SIZE_Y) ? SIZE_X : SIZE_Y;
        int size = (Screen.height-300) / max;
        int xp = 1435;
        int yp = (Screen.height - SIZE_Y * size) / 2;
        GUI.DrawTextureWithTexCoords(new Rect(xp - 20, yp - 20, SIZE_X * size + 20, SIZE_Y * size + 20), miniMapBackTex, new Rect(0, 0, 1, 1));
        GUI.color = new Color(0, 0, 1);
        GUI.DrawTextureWithTexCoords(new Rect(xp + size/4 + (SIZE_X - entry.x / 4) * size, yp + entry.z / 4 * size, size, size), miniMapBackTex, new Rect(0, 0, 1, 1));
        GUI.color = new Color(1, 0, 0);
        GUI.DrawTextureWithTexCoords(new Rect(xp + size/4 + (SIZE_X - exit.x / 4) * size, yp + exit.z / 4 * size, size, size), miniMapBackTex, new Rect(0, 0, 1, 1));
        GUI.color = new Color(1, 1, 1);
        //GUI.Box(new Rect(xp-20, yp-20, SIZE_X * 20+20, SIZE_Y * 20+20), "");
        for (int y = 0; y < SIZE_Y; y++)
        {
            for (int x = 0; x < SIZE_X; x++)
            {
                int xx = xp + x * size;
                int yy = yp + y * size;

                if (imap[x, y] != 0) GUI.Box(new Rect(xx, yy, size, size), "");
                if ((map[x, y] & 1) != 0) GUI.Box(new Rect(xx, yy, 2, size), "");
                if ((map[x, y] & 2) != 0) GUI.Box(new Rect(xx, yy, size, 2), "");
                //if (imap[x, y] != 0) GUI.Label(new Rect(xx+2, yy, 20, 20), ""+ imap[x, y]);
            }
        }

        Matrix4x4 matrixBackup = GUI.matrix;
        float rt = rot.eulerAngles.y-90;
        float cx = xp + SIZE_X * size - pos.x / 4 * size;
        float cy = yp + pos.z / 4 * size;
        GUIUtility.RotateAroundPivot(rt, new Vector2(cx+8, cy+8));
        GUI.DrawTexture(new Rect(cx, cy, 16, 16), miniMapArrowTex);
        GUI.matrix = matrixBackup;

    }

    public int setSizeX(int sx)
    {
        if (sx < 16) sx = 16;
        if (sx > 64) sx = 64;
        if (SIZE_X != sx)
        {
            SIZE_X = sx;
            generator();
        }
        return SIZE_X;
    }

    public int setSizeY(int sy)
    {
        if (sy < 16) sy = 16;
        if (sy > 64) sy = 64;
        if (SIZE_Y != sy) {
            SIZE_Y = sy;
            generator();
        }
        return SIZE_Y;
    }

    public Vector3 getEntry()
    {
        return entry;
    }

    public Vector3 getExit()
    {
        return exit;
    }

    /*********************************************************************************/
    /* Create dungeon
    /*********************************************************************************/
    public void generator()
    {
        entry = Vector3.zero;
        exit = Vector3.zero;

        map = new int[SIZE_X, SIZE_Y];
        imap = new int[SIZE_X, SIZE_Y];

        // clear old object 
        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
            GameObject.Destroy(transform.GetChild(i).gameObject);

        // installation of rectangular areas
        int maxbox = (SIZE_X * SIZE_Y)/(14-density); // 128;
        for (int i = 1; i < maxbox; i++)
        {
            int x = rndX();
            int y = rndY();
            int sx = (geti() & 0x7) + 2;
            int sy = (geti() & 0x7) + 2;
            putBox(x, y, sx, sy, i);
        }

        // join of regions and the establishment of a single passage
        for (int i = 1; i < 2048; i++)
        {
            int x = rndX();
            int y = rndY();
            int i1 = imap[x, y];
            if (i1 > 0)
            {
                int ox = x;
                x++; if (x >= SIZE_X) x -= SIZE_X;
                int i2 = imap[x, y];
                if ((i1 != i2) && (i2 > 0))
                {
                    map[x, y] &= 2;
                    joinRegion(x, y, i2, i1);
                }
                else
                {
                    x = ox;
                    y++; if (y >= SIZE_Y) y -= SIZE_Y;
                    i2 = imap[x, y];
                    if ((i1 != i2) && (i2 > 0))
                    {
                        map[x, y] &= 1;
                        joinRegion(x, y, i2, i1);
                    }
                }
            }
        }

        // Search larger region
        int[] max = new int[maxbox];
        for (int y = 0; y < SIZE_Y; y++)
            for (int x = 0; x < SIZE_X; x++)
                max[imap[x, y]]++;

        int mi = 1;
        for (int i = 2; i < maxbox; i++)
            if (max[mi] < max[i]) mi = i;

        for (int y = 0; y < SIZE_Y; y++)
            for (int x = 0; x < SIZE_X; x++)
                if (imap[x, y] != mi) imap[x, y] = 0; else imap[x, y] = 1;

        clearRegion(); // remove any disconnected areas

        // create 3d dungeon
        for (int y = 0; y < SIZE_Y; y++)
        {
            for (int x = 0; x < SIZE_X; x++)
            {
                if (imap[x, y] != 0)
                {
                    float rotate = 0;
                    GameObject seg = dan_9;

                    int id = map[x, y];
                    if (x < SIZE_X - 1) id |= (map[x + 1, y] & 1) << 2;
                    if (y < SIZE_Y - 1) id |= (map[x, y + 1] & 2) << 2;
                    //map[x, y] = id;

                    int type = 9;

                    switch (id)
                    {
                        case 0x3: type = 2; rotate = 90.0f; break;
                        case 0x6: type = 2; rotate = 180.0f; break;
                        case 0x9: type = 2; rotate = 0.0f; break;
                        case 0xC: type = 2; rotate = 270.0f; break;

                        case 0x1: type = 1; rotate = 0.0f; break;
                        case 0x2: type = 1; rotate = 90.0f; break;
                        case 0x4: type = 1; rotate = 180.0f; break;
                        case 0x8: type = 1; rotate = 270.0f; break;

                        case 0x5: type = 3; rotate = 0.0f; break;
                        case 0xA: type = 3; rotate = 90.0f; break;

                        case 0x7: type = 4; rotate = 180.0f; break;
                        case 0xB: type = 4; rotate = 90.0f; break;
                        case 0xD: type = 4; rotate = 0.0f; break;
                        case 0xE: type = 4; rotate = 270.0f; break;
                    }


                    int id2 = 0;
                    if (x - 1 >= 0)
                    {
                        id2 |= (map[x - 1, y] & 2) >> 1;    // bit 1
                        id2 |= (map[x - 1, y + 1] & 2) << 2;  // bit 8
                    }
                    if (y - 1 >= 0)
                    {
                        id2 |= (map[x, y - 1] & 1);         // bit 1
                        id2 |= (map[x + 1, y - 1] & 1) << 1;// bit 2
                    }
                    id2 |= (map[x + 1, y] & 2);             // bit 2
                    id2 |= (map[x + 1, y + 1] & 1) << 2;    // bit 4
                    id2 |= (map[x + 1, y + 1] & 2) << 1;    // bit 4
                    id2 |= (map[x, y + 1] & 1) << 3;        // bit 8

                    map[x, y] = id + (id2 << 8);

                    if (type == 9)
                    {
                        switch (id2)
                        {
                            case 0x1: type = 6; rotate = 0.0f; break;
                            case 0x2: type = 6; rotate = 90.0f; break;
                            case 0x4: type = 6; rotate = 180.0f; break;
                            case 0x8: type = 6; rotate = 270.0f; break;

                            case 0x3: type = 7; rotate = 0.0f; break;
                            case 0x6: type = 7; rotate = 90.0f; break;
                            case 0x9: type = 7; rotate = 270.0f; break;
                            case 0xC: type = 7; rotate = 180.0f; break;

                            case 0x5: type = 5; rotate = 0.0f; break;
                            case 0xA: type = 5; rotate = 90.0f; break;

                            case 0x7: type = 8; rotate = 0.0f; break;
                            case 0xB: type = 8; rotate = 270.0f; break;
                            case 0xD: type = 8; rotate = 180.0f; break;
                            case 0xE: type = 8; rotate = 90.0f; break;

                            case 0xF: type = 14; rotate = 0.0f; break;
                        }
                    }

                    if (type == 1)
                    {
                        if ((rotate == 0.0f) && ((id2 & 6) == 4)) type = 10;
                        if ((rotate == 90.0f) && ((id2 & 0xC) == 8)) type = 10;
                        if ((rotate == 180.0f) && ((id2 & 9) == 1)) type = 10;
                        if ((rotate == 270.0f) && ((id2 & 3) == 2)) type = 10;

                        if ((rotate == 0.0f) && ((id2 & 6) == 2)) type = 12;
                        if ((rotate == 90.0f) && ((id2 & 0xC) == 4)) type = 12;
                        if ((rotate == 180.0f) && ((id2 & 9) == 8)) type = 12;
                        if ((rotate == 270.0f) && ((id2 & 3) == 1)) type = 12;

                        if ((rotate == 0.0f) && ((id2 & 6) == 6)) type = 11;
                        if ((rotate == 90.0f) && ((id2 & 0xC) == 0xC)) type = 11;
                        if ((rotate == 180.0f) && ((id2 & 9) == 9)) type = 11;
                        if ((rotate == 270.0f) && ((id2 & 3) == 3)) type = 11;
                    }

                    if (type == 2)
                    {
                        if ((rotate == 0.0f) && ((id2 & 2) == 2)) type = 13;
                        if ((rotate == 90.0f) && ((id2 & 4) == 4)) type = 13;
                        if ((rotate == 180.0f) && ((id2 & 8) == 8)) type = 13;
                        if ((rotate == 270.0f) && ((id2 & 1) == 1)) type = 13;
                    }

                    switch (type)
                    {
                        case 1: seg = dan_1; break;
                        case 2: seg = dan_2; break;
                        case 3: seg = dan_3; break;
                        case 4: seg = dan_4; break;
                        case 5: seg = dan_5; break;
                        case 6: seg = dan_6; break;
                        case 7: seg = dan_7; break;
                        case 8: seg = dan_8; break;
                        case 9: seg = dan_9; break;
                        case 10: seg = dan_10; break;
                        case 11: seg = dan_11; break;
                        case 12: seg = dan_12; break;
                        case 13: seg = dan_13; break;
                        case 14: seg = dan_14; break;
                    }

                    GameObject obj = (GameObject)Instantiate(seg, new Vector3(SIZE_X * 4 - x * 4, 0, y * 4), Quaternion.Euler(new Vector3(0, rotate, 0)));
                    obj.name = "seg_" + x + "_" + y;
                    obj.transform.parent = transform;


                    // ----------------------------------------------------------------------------
                    // placement of objects on the cave
                    // ----------------------------------------------------------------------------
                    if ((id & 1) != 0)
                    {
                        Vector3 pos = new Vector3(SIZE_X * 4 - x * 4 + 1, 0, y * 4);
                        if (entry.x == 0)
                        {
                            entry = pos;
                            exit = pos;
                        }

                        if (Vector3.Distance(entry, exit) < Vector3.Distance(entry, pos))
                        {
                            exit = pos;
                        }

                        if ((geti() & 1) == 0)
                        {
                            placeAroundObject(x, y, 1, 0);
                        }
                        else {
                            placeAroundObject(x, y, 1, -1);
                            placeAroundObject(x, y, 1, 1);
                        }
                    }
                    if ((id & 4) != 0)
                    {
                        if ((geti() & 1) == 0)
                        {
                            placeAroundObject(x, y, -1, 0);
                        }
                        else {
                            placeAroundObject(x, y, -1, -1);
                            placeAroundObject(x, y, -1, 1);
                        }
                    }

                    if ((id & 2) != 0)
                    {
                        if ((geti() & 1) == 0)
                        {
                            placeAroundObject(x, y, 0, -1);
                        }
                        else {
                            placeAroundObject(x, y, -1, -1);
                            placeAroundObject(x, y, 1, -1);
                        }
                    }
                    if ((id & 8) != 0)
                    {
                        if ((geti() & 1) == 0)
                        {
                            placeAroundObject(x, y, 0, 1);
                        }
                        else {
                            placeAroundObject(x, y, -1, 1);
                            placeAroundObject(x, y, 1, 1);
                        }
                    }

                    if (type == 9)
                    {
                        if ((geti() & 1) == 0) placeRandomObject(x, y);
                        placeGroundRandomObject(x, y);
                    }


                }
            }
        }

    }

    private void placeAroundObject(float x, float y, float dx, float dy)
    {
        GameObject obj = (GameObject)Instantiate(around[geti() % around.Length], new Vector3(SIZE_X * 4 - x * 4 + dx, 0, y * 4 + dy), Quaternion.Euler(new Vector3(0, geti() & 0x1FF, 0)));
        obj.transform.parent = transform;
    }

    private void placeRandomObject(int x, int y)
    {
        if ((imap[x, y] == 1) && (map[x, y] == 0))
        {
            imap[x, y] = 2;
            float xf = x + 0.5f - 1.0f / 16.0f * (geti() & 0xF);
            float yf = y + 0.5f - 1.0f / 16.0f * (geti() & 0xF);
            GameObject obj = (GameObject)Instantiate(random[geti() % random.Length], new Vector3(SIZE_X * 4 - xf * 4, 0, yf * 4), Quaternion.Euler(new Vector3(0, geti() & 0x1FF, 0)));
            obj.transform.parent = transform;
        }
    }

    private void placeGroundRandomObject(int x, int y)
    {
        if ((imap[x, y] == 1) && (map[x, y] == 0))
        {
            imap[x, y] = 2;
            float xf = x + 0.5f - 1.0f / 16.0f * (geti() & 0xF);
            float yf = y + 0.5f - 1.0f / 16.0f * (geti() & 0xF);
            GameObject obj = (GameObject)Instantiate(ground[geti() % ground.Length], new Vector3(SIZE_X * 4 - xf * 4, 0, yf * 4), Quaternion.Euler(new Vector3(0, geti() & 0x1FF, 0)));
            obj.transform.parent = transform;
        }
    }

    private void joinRegion(int cx, int cy, int i1, int i2)
    {
        bool isNext = true;
        while (isNext)
        {
            isNext = false;
            for (int y = 0; y < SIZE_Y; y++)
                for (int x = 0; x < SIZE_X; x++)
                    if (imap[x, y] == i1)
                    {
                        int ix = x - 1;
                        if ((ix >= 0) && (imap[ix, y] == i2) && ((map[x, y] & 1) == 0)) { imap[x, y] = i2; isNext = true; }

                        ix = x + 1;
                        if ((ix < SIZE_X) && (imap[ix, y] == i2) && ((map[ix, y] & 1) == 0)) { imap[x, y] = i2; isNext = true; }


                        int iy = y - 1;
                        if ((iy >= 0) && (imap[x, iy] == i2) && ((map[x, y] & 2) == 0)) { imap[x, y] = i2; isNext = true; }

                        iy = y + 1;
                        if ((iy < SIZE_Y) && (imap[x, iy] == i2) && ((map[x, iy] & 2) == 0)) { imap[x, y] = i2; isNext = true; }
                    }
        }
    }

    private void clearRegion()
    {
        for (int y = 0; y < SIZE_Y; y++)
            for (int x = 0; x < SIZE_X; x++)
                if ((imap[x, y] == 0) && (map[x, y] != 0))
                {
                    if ((x == 0) || (imap[x - 1, y] == 0)) map[x, y] &= 2;
                    if ((y == 0) || (imap[x, y - 1] == 0)) map[x, y] &= 1;
                }
    }

    private void putBox(int x, int y, int sx, int sy, int index)
    {
        if ((x >= 0) && (y >= 0) && ((x + sx) < SIZE_X) && ((y + sy) < SIZE_Y))
        {
            for (int ix = 0; ix < sx; ix++)
            {
                for (int iy = 0; iy < sy; iy++)
                {
                    imap[x + ix, y + iy] = index;
                    map[x + ix, y + iy] = 0;
                    if (ix == 0) map[x + ix, y + iy] |= 1;
                    if (iy == 0) map[x + ix, y + iy] |= 2;
                    if (ix == sx - 1) map[x + ix + 1, y + iy] |= 1;
                    if (iy == sy - 1) map[x + ix, y + iy + 1] |= 2;
                }
            }
        }
    }

    private int rndX()
    {
        int x = (geti() + geti() * 3 + geti() * 7) & 0x3F;
        while (x >= SIZE_X) { x -= SIZE_X; }
        return x;
    }

    private int rndY()
    {
        int y = (geti() + geti() * 3 + geti() * 7) & 0x3F;
        while (y >= SIZE_Y) { y -= SIZE_Y; }
        return y;
    }

    private int geti()
    {
        int[] crnd = { 0x40000000, 0, 0, 0x40000000, 0, 0x40000000, 0x40000000, 0 };
        iRND = crnd[(iRND & 7)] | (iRND >> 1);
        return iRND & 0x7FFFFFFF;
    }





}
