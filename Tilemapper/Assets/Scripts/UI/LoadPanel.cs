using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadPanel : MonoBehaviour {
    public InputField SaveFileName;

    public void SetSaveFileName(string name)
    {
        SaveFileName.textComponent.text = name;
    }
}
