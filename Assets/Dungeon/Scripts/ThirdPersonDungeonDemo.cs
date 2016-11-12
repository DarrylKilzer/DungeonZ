using UnityEngine;
using System.Collections;

public class ThirdPersonDungeonDemo : MonoBehaviour {

    public Dungeon dungeon;

    private bool isShowMap = false;

    // Use this for initialization
    void Start () {
        CreateDungeon();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.M))
        {
            isShowMap = !isShowMap;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 100, Screen.width, Screen.height), "Press M for map\nPress Escape to quit");
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (GUI.Button(new Rect(0, 20, 100, 30), "New dungeon"))
        {
            CreateDungeon();
        }

        if (GUI.Button(new Rect(0, 60, 100, 30), "Show Map"))
        {
            isShowMap = !isShowMap;
        }
        if (isShowMap)
        {
            dungeon.drawMap(transform.position, transform.rotation);
        }
        int ex = (int)(dungeon.getExit().x / 4 + 2);
        int ey = (int)(dungeon.getExit().z / 4 + 2);
        int cx = (int)(transform.position.x / 4 + 2);
        int cy = (int)(transform.position.z / 4 + 2);
        if ((ex == cx) && (ey == cy)) CreateDungeon();
        //Debug.Log(ex+" "+ey+" "+cx+" "+cy);
    }

    private void CreateDungeon()
    {
        dungeon.generator();
        transform.position = dungeon.getEntry();
    }
}
