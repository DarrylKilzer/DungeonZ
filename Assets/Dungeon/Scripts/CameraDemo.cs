using UnityEngine;
using System;
using System.Collections;

public class CameraDemo : MonoBehaviour
{

    public GameObject goDungeon;

    private Dungeon dungeon;
    private int dungeonId, oldId;
    private int sizeX, sizeY;
    private bool isShowHelp = true;
    private bool isShowMap = true;

    private static Vector3 position;
    private static Vector3 angle;

    private static float distance = 30, maxDist = 100;


    // Use this for initialization
    void Start()
    {
        position = new Vector3(64, 0, 64);
        angle = new Vector3(0, 180, 0);
        dungeon = (Dungeon)goDungeon.GetComponent(typeof(Dungeon));
        sizeX = dungeon.setSizeX(32);
        sizeY = dungeon.setSizeY(32);
        dungeon.generator();
    }

    // Update is called once per frame
    void Update()
    {

        distance += Input.GetAxis("Mouse ScrollWheel") * 10;

        if (distance < 10) distance = 10;
        if (distance > maxDist) distance = maxDist;

        if (Input.GetMouseButton(1))  // rotate
        {
            angle.x += Input.GetAxis("Mouse Y") * 1.5f;
            angle.y += Input.GetAxis("Mouse X") * 1.5f;
        }

        if (angle.x < 30) angle.x = 30;
        if (angle.x > 85) angle.x = 85;


        if (Input.GetMouseButton(0)) // moved
        {
            float moveSpeed = 1.0f;
            float ax = Input.GetAxis("Mouse X") * moveSpeed;
            float ay = Input.GetAxis("Mouse Y") * moveSpeed;
            Vector3 pos = Quaternion.Euler(angle) * new Vector3(-ax, 0.0f, -ay) + position;
            position.x = pos.x;
            position.z = pos.z;
        }

        Vector3 v = new Vector3(0.0f, 0.0f, -distance);

        transform.rotation = Quaternion.Euler(angle);
        transform.position = transform.rotation * v + position;
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 190, 230), "");
        GUI.Label(new Rect(20, 20, 100, 20), "Dungeon ID:");
        dungeonId = Convert.ToInt32(GUI.TextField(new Rect(100, 20, 80, 20), "" + dungeonId));

        GUI.Label(new Rect(20, 50, 120, 20), "Size X(16...64):");
        sizeX=Convert.ToInt32(GUI.TextField(new Rect(120, 50, 60, 20), "" + sizeX));

        GUI.Label(new Rect(20, 80, 120, 20), "Size Y(16...64):");
        sizeY=Convert.ToInt32(GUI.TextField(new Rect(120, 80, 60, 20), "" + sizeY));

        GUI.Label(new Rect(20, 110, 120, 20), "Density: " + dungeon.density);
        dungeon.density = (int)GUI.HorizontalSlider(new Rect(100, 110, 80, 10), dungeon.density, 1, 10);

        isShowHelp = GUI.Toggle(new Rect(20, 140, 100, 20), isShowHelp, "Show Help");

        isShowMap = GUI.Toggle(new Rect(20, 170, 100, 20), isShowMap, "Show map");


        if (GUI.Button(new Rect(20, 190, 100, 30), "New dungeon"))
        {
            if (oldId == dungeonId)
            {
                dungeonId++;
            }
            oldId = dungeonId;
            dungeon.iRND = dungeonId;
            sizeX = dungeon.setSizeX(sizeX);
            sizeY = dungeon.setSizeY(sizeY);
            dungeon.generator();
        }

        if (isShowMap) dungeon.drawMap(position, transform.rotation);


        if (isShowHelp)
        {
            GUI.Label(new Rect(Screen.width/2-300, Screen.height-50, 600, 20), "Mouse whell - camera distance, Mouse Left - camera move, Mouse Right - camera rotate");
        }

    }

    

}