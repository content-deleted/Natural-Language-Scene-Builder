using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TypingInput : MonoBehaviour
{

    public InputParse inputControl;
    public Text textDisplay;

    void Start()
    {
        textDisplay = GetComponent<Text>();
    }

    private string userInput;
    private float counter=0;
    public bool entered= false;
    void Update()
    {
        if(!entered){
            counter+=0.03f;
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // has backspace/delete been pressed?
                {
                    if (userInput.Length != 0)
                    {
                        userInput = userInput.Substring(0, userInput.Length - 1);
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    // This starts the program
                    inputControl.input = userInput;
                    entered = true;
                    textDisplay.text =  userInput + " ";
                    inputControl.gameObject.SetActive(true);

                }
                else
                {
                    userInput += c;
                }
            }
            textDisplay.text =  userInput + ((((int)counter)%2 ==0 ) ? " " : "_");
            //(new DirectoryInfo(@"./Assets/stanford-corenlp-models/english-left3words-distsim.tagger")).FullName.ToString() + "\n"+ File.Exists(@"./Assets/stanford-corenlp-models/english-left3words-distsim.tagger");
        }
    }
}
