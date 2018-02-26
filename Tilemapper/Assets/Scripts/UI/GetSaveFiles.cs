using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GetSaveFiles : MonoBehaviour {

    List<Button> SaveFileButtons;
    public Button FileButtonPrefab;
    public InputField FileNameInputField;

    private void OnEnable()
    {
        SaveFileButtons = new List<Button>();
        
        //get all json files in the dungeons directory
        foreach(string file in Directory.GetFiles("Assets/MapSaves/Dungeons/", "*.json"))
        {
            string jsonString = File.ReadAllText(file);
            Dungeon temp = JsonUtility.FromJson<Dungeon>(jsonString);
            Button toAdd = Instantiate(FileButtonPrefab, transform);
            toAdd.GetComponent<FileButton>().FileName = Path.GetFileNameWithoutExtension(file);
            toAdd.GetComponent<FileButton>().FileNameField = FileNameInputField;
            toAdd.GetComponentInChildren<Text>().text = "Name: "+ Path.GetFileNameWithoutExtension(file) + "    Floors: " + temp.Floors.Count;
            SaveFileButtons.Add(toAdd);
        }

    }

    private void OnDisable()
    {
        for(int i = 0; i < SaveFileButtons.Count; i++)
        {
            Destroy(SaveFileButtons[i].gameObject);
        }
        SaveFileButtons.Clear();
    }
}
