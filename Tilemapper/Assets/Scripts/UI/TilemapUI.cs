using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class TilemapUI : MonoBehaviour {

    public Tilemap tilemap;
    public Slider widthSlider;
    public Slider heightSlider;
    public Image LMBSprite;
    public Image RMBSprite;
    public int CurrentFloorNum { get; set; }

    private void Awake()
    {
        tilemap.GridWidth = (int)widthSlider.value;
        tilemap.GridHeight = (int)heightSlider.value;
        UITileOption[] tileOptions = GetComponentsInChildren<UITileOption>();
        for(int i = 0; i < tileOptions.Length; i++)
        {
            tileOptions[i].tilemap = tilemap;
        }
    }

    public void SetGridWidth(float width)
    {
        tilemap.GridWidth = (int)width;
        tilemap.ResizeGrid();
    }

    public void SetGridHeight(float height)
    {
        tilemap.GridHeight = (int)height;
        tilemap.ResizeGrid();
    }

    public void ResizeGrid()
    {
        tilemap.ResizeGrid();
    }

    public void SetLMBSprite(Sprite sprite)
    {
        LMBSprite.sprite = sprite;
    }

    public void SetRMBSprite(Sprite sprite)
    {
        RMBSprite.sprite = sprite;
    }

    public void SetFloorNum(int newNum)
    {
        CurrentFloorNum = newNum;
    }

    public void LoadDungeon(string dungeonName)
    {
        string path = "Assets/MapSaves/" + dungeonName + ".json";
        if(File.Exists(path))
        {
            Debug.Log("file found");
            string jsonString = File.ReadAllText(path);
            tilemap.ThisDungeon = JsonUtility.FromJson<Dungeon>(jsonString);
        }
        else
        {
            Debug.Log("file not found, does " + path + " exist?");
        }
        tilemap.CurrentFloor = tilemap.ThisDungeon.Floors[0];
        tilemap.LoadFromFile();
    }

    public void SaveDungeon(string dungeonName)
    {
        string path = "Assets/MapSaves/" + dungeonName +".json";
        Debug.Log("saving to: " + path);
        string toWrite = JsonUtility.ToJson(tilemap.ThisDungeon, false);
        Debug.Log("adding this:\n" + toWrite);
        File.WriteAllText(path, toWrite);
        /*
        StreamWriter file = File.CreateText(path);
        file.WriteLine(toWrite);
        file.Close();
        */
    }

    public void GenerateFloorLayout()
    {
        //tilemap.BSPGenerate();
        LoadDungeon("test");
        //SaveDungeon("test");
    }
}
