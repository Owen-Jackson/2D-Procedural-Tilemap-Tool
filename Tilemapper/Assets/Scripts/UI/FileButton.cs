using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileButton : MonoBehaviour {
    public InputField FileNameField { get; set; }

    public string FileName { get; set; }

    public void SetLoadString()
    {
        FileNameField.text = FileName;
    }
}
