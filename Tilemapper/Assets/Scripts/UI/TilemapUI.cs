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
    public GameObject EditPanel;
    public GameObject SavePanel;
    public int CurrentFloorNum { get; set; }
    public string DungeonFileName { get; set; }

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

    public void LoadDungeon()
    {
        string path = "Assets/MapSaves/Dungeons/" + DungeonFileName + ".json";
        //Debug.Log(DungeonFileName);
        if(File.Exists(path))
        {
            //Debug.Log("file found");
            string jsonString = File.ReadAllText(path);
            tilemap.ThisDungeon = JsonUtility.FromJson<Dungeon>(jsonString);
        }
        else
        {
            Debug.Log("file not found, does " + path + " exist?");
        }
        tilemap.CurrentFloor = tilemap.ThisDungeon.Floors[0];
        tilemap.LoadFloor(0);
    }

    public void SaveDungeon()
    {
        string path = "Assets/MapSaves/Dungeons/" + DungeonFileName + ".json";
        Debug.Log("saving to: " + path);
        string toWrite = JsonUtility.ToJson(tilemap.ThisDungeon);
        Debug.Log("adding this:\n" + toWrite);
        File.WriteAllText(path, toWrite);
    }

    public void GenerateFloorLayout()
    {
        tilemap.BSPGenerate();
        //LoadDungeon();
        //SaveDungeon("test");
    }

    public void SwitchToEditPanel()
    {
        if(SavePanel.GetComponent<SaveOptionsPanel>().SavePanel.activeSelf)
        {
            SavePanel.GetComponent<SaveOptionsPanel>().SavePanel.SetActive(false);
        }
        if (SavePanel.GetComponent<SaveOptionsPanel>().LoadPanel.activeSelf)
        {
            SavePanel.GetComponent<SaveOptionsPanel>().LoadPanel.SetActive(false);
        }
        EditPanel.transform.SetAsLastSibling();
    }
}
