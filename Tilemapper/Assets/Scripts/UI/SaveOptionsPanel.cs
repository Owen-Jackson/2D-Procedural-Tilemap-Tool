using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveOptionsPanel : MonoBehaviour {
    public GameObject SavePanel;
    public GameObject LoadPanel;

    private void Start()
    {
        SavePanel.SetActive(false);
        LoadPanel.SetActive(false);
    }

    public void OpenLoadPanel()
    {
        if(SavePanel.activeSelf)
        {
            SavePanel.SetActive(false);
        }
        LoadPanel.SetActive(true);
    }

    public void OpenSavePanel()
    {
        if(LoadPanel.activeSelf)
        {
            LoadPanel.SetActive(false);
        }
        SavePanel.SetActive(true);
    }
}
