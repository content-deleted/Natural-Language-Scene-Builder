using java.io;
using java.util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.tagger.maxent;
using edu.stanford.nlp.parser.nndep;
using edu.stanford.nlp.process;
using System;
using System.Net;
using System.Linq;

using System.Windows;
using System.Threading.Tasks;
using System.Threading;

using System.IO;
using TriLib;
using edu.stanford.nlp.trees;
using edu.stanford.nlp.ie.crf;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO.Compression;

public class InputParse : MonoBehaviour
{
    public List<GameObject> disableOnLoad;
    public Text debugDisplay;
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController fps;
    public GameObject restartText;
    public string input;
    private static string modelsDirectory = @"./Assets/stanford-corenlp-models";
    private static string model = modelsDirectory + @"/english-left3words-distsim.tagger";

    private static string customModelDirectory = @"./Assets/CustomModels";
        
    private static Dictionary<string, Vector3> directions = new Dictionary<string,Vector3> {
        {"right",Vector3.right},
        {"left",Vector3.left},
        {"up",Vector3.up},
        {"down",Vector3.down},
        {"forward",Vector3.back},
        {"backward",Vector3.forward},
    };
    
    public static InputParse singleton;
    public class sceneObject {
        public String id;
        public bool multiple;
        public String name; 

        public Vector3? direction;
        public bool firstInSentence;
        public static Dictionary<string, GameObject> namedModels = new Dictionary<string, GameObject>();
        public bool unique;
        public sceneObject(String Name, String Id, bool Multiple, bool first, bool Unique, Vector3? dir = null) {   
            name = Name;
            id = Id;
            multiple = Multiple;

            direction = dir;
            firstInSentence = first;
            unique = Unique;
        }
        public async Task DownloadFile()
        {
            using (var client = new WebClient())
            {
                Debug.Log($"Starting download for {name}"); 
                singleton.debugDisplay.text = ($"Starting download for {name}"); 
                var url = $"https://archive3d.net/?a=download&do=get&id={id}";
                
                var location = $"./Assets/Resources/dl/raw/{name}.zip";
                await client.DownloadFileTaskAsync(url,location);
                Debug.Log(name+" download complete");
                singleton.debugDisplay.text = (name+" download complete");

                string extractPath = $"./Assets/Resources/dl/{name}/";
                
                ZipFile.ExtractToDirectory(location,extractPath);

                Debug.Log(name+" unzip complete");
                singleton.debugDisplay.text = (name+" extract complete");
                
                // Check if download and extract was successful
                if(Directory.GetFiles(extractPath, "*.3ds").Any()){
                    var modelLocation = Directory.GetFiles(extractPath, "*.3ds").First();
                    namedModels[name] = LoadModel.load(modelLocation);
                }
            }
        }
    }


    public object spatialClassifier;

    public List<sceneObject> sceneObjects = new List<sceneObject>();
    private bool beginCheck = false;
    private string nounStack = "";
    private bool lastIsPlural = false;
    void Start()
    {
        singleton = this;
        if (!System.IO.File.Exists(model))
            throw new Exception($"Check path to the model file '{model}'");

        // Load our custom directional classifier
        spatialClassifier = CRFClassifier.getClassifier(customModelDirectory + @"/spatialDirectionModel.gaz.ser.gz");

        // Loading POS Tagger
        MaxentTagger tagger = new MaxentTagger(model);

        var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(input)).toArray();
        foreach (java.util.ArrayList sentence in sentences)
        {
            var taggedSentence = tagger.tagSentence(sentence);
            Debug.Log(SentenceUtils.listToString(taggedSentence, false));
            debugDisplay.text = (SentenceUtils.listToString(taggedSentence, false));

            String previousNoun = null;
            String connectingPhrase = "";
            foreach( var x in taggedSentence.toArray()){
                String taggedWord = x.ToString();
                String word = taggedWord.Split('/').FirstOrDefault();
                
                if(taggedWord.Contains("/NN") 
                    && false == (spatialClassifier as CRFClassifier).classifyToString($"{connectingPhrase} {word}")
                    ?.Split(' ').Where(s => !s.Contains("/0")).Where(s2 => s2.Split('/').First() == word).Any()) {
                        nounStack += ((nounStack.ToArray().Any()) ? " " : "") + word;
                        lastIsPlural |= taggedWord.Contains("/NNS");
                }
                else {
                    if(nounStack != "")
                    {
                        var id = getModelId(nounStack);
                        Debug.Log($"Found id for {nounStack}");
                        debugDisplay.text = $"Found id for {nounStack}";

                        if(id != null){
                            sceneObjects.Add(new sceneObject(nounStack, id, lastIsPlural, (previousNoun == null), !sceneObjects.Where( s=> s.name == nounStack).Any(), parseDirection(previousNoun, connectingPhrase)));
                        }
                    
                        connectingPhrase = "";
                        previousNoun = word;
                        nounStack = "";
                    }
                    connectingPhrase += ( (!connectingPhrase.ToCharArray().Any()) ? "" : " ") + word; 
                }
            }

            if(nounStack != "")
            {
                var id = getModelId(nounStack);
                Debug.Log($"Found id for {nounStack}");
                debugDisplay.text = $"Found id for {nounStack}";

                if(id != null){
                    sceneObjects.Add(new sceneObject(nounStack, id, lastIsPlural, (previousNoun == null), !sceneObjects.Where( s=> s.name == nounStack).Any(), parseDirection(previousNoun, connectingPhrase)));
                }
            }
        }
        // Once we have all our ids start downloading the models
        // download = https://archive3d.net/?a=download&do=get&id={modelId}

        // Set up dictonary of locations

        var uniqueOnes = sceneObjects.Where(u => u.unique);
        foreach(var sceneObj in uniqueOnes) sceneObject.namedModels.Add(sceneObj.name, null);

        DownloadFiles();
        beginCheck=true;
    }

    public Vector3? parseDirection (String n, String phrase) {
        // maybe check noun too lets see how it works
        if(phrase == null) return null;
        
        var classifier = spatialClassifier as CRFClassifier;
        string output = classifier.classifyToString(phrase);

        // parse out directional vector from this
        var tags = output.Split(' ').Where(s => !s.Contains("/0")).Select(ss => ss.Split('/').LastOrDefault());

        if(tags == null) return null;
        //comp against dir dict later
        var dir = tags.Where(x=> x != "inverse" && x != "").FirstOrDefault();

        Debug.Log(output);
        debugDisplay.text = output;

        if(dir != null && dir != ""){
            Vector3 outDir = directions[dir];
            if(tags.Where(x => x == "inverse").Any()) outDir *= -1;

            return outDir * (UnityEngine.Random.value*5 + 5);
        }

        return null;
    }
    bool spawned = false;

    // replace this later
    UnityEngine.Random rand = new UnityEngine.Random();
    public static Vector3 previousPos;
    public void Update () {
        //!sceneObjects.ToList().Where(x => x.modelLocation == null).Any()
        if(!spawned && beginCheck && !sceneObject.namedModels.Where(pair => pair.Value == null).Any()) {
            Debug.Log("Spawning... ");
            debugDisplay.text = "Spawning... ";

            int j = 0;
            for(int i = (sceneObjects.Count>1) ? 1 : 0; i < sceneObjects.Count; i++)
            {
                if(sceneObjects[i].firstInSentence || i == sceneObjects.Count-1) {
                    Vector3? d = null;
                    for(int k = i - ((i == sceneObjects.Count-1) ? 0 : 1); k >= j; k--) {
                        var subject = sceneObjects[k];
                        //Debug.Log(subject.modelLocation);
                        //debugDisplay.text = "From " + subject.modelLocation;
                        //var e = Instantiate (Resources.Load ("Empty") as GameObject,new Vector3(UnityEngine.Random.value*10 - 5,0,UnityEngine.Random.value*10-5),Quaternion.identity);
                        //var loader = e.GetComponent<Loader3DS>();
                        //loader.modelPath = subject.modelLocation;
                        //StartCoroutine(loader.Loader(loader.modelPath));

                        // we want to pass a direction and relative location for each object. 
                        // In most case the reletive location will be the center of camera space
                        Vector3 startingLoc = (k == i-((i == sceneObjects.Count-1) ? 0 : 1)) ? Camera.main.transform.position : previousPos;
                        Vector3 dir =  d ?? new Vector3(UnityEngine.Random.value,0,UnityEngine.Random.value);

                        // set the dir for next
                        d = subject.direction;
                        
                        // if its the first thing in the sceen we need to apply the direction to it (from the camera)
                        if(subject.firstInSentence && d!=null) dir = d ?? new Vector3(UnityEngine.Random.value,0,UnityEngine.Random.value);

                        int amt = subject.multiple ? 2 + (int) (5 * UnityEngine.Random.value * UnityEngine.Random.value) : 1;
                        Position.PlaceObject(subject.name, startingLoc, dir, amt);
                    }
                    j = i;
                }
            }
            spawned = true;

            // Remove gui
            foreach(GameObject g in disableOnLoad) g.SetActive(false);
            // activate movement
            fps.enabled = true;
            fps.GetComponent<CharacterController>().enabled = true;
            restartText.SetActive(true);

        }
        if(spawned&& Input.GetKeyDown(KeyCode.Escape)){
            DeleteOld();
            SceneManager.LoadScene("Main");
        }
    }
    /* Some notes on process
    * We take valid object nouns and place them into a stack containing the object and a direction 
    * After we've parsed the sentence we pop the stack and pass in the previous object and direction  
    * 
    *
     */
    String getModelId(string phrase) {
        Debug.Log($"Searching for {phrase}...");
        debugDisplay.text = $"Searching for {phrase}...";

        String result = null;
        string pageContent = null; 
        var myReq = (HttpWebRequest)WebRequest.Create($"https://www.googleapis.com/customsearch/v1?q={phrase}&cx=003829614435954349180:p-jzyrnkg4y&key=AIzaSyBUI44YdolLw_qHbmIY40RktPuDt55rK_w");
        HttpWebResponse myres = (HttpWebResponse)myReq.GetResponse();

        using (System.IO.StreamReader sr = new System.IO.StreamReader(myres.GetResponseStream()))
        {
            pageContent = sr.ReadLine();
            while(pageContent != null){
                if (pageContent.Contains("download"))
                {
                    var id = pageContent.Split('=').LastOrDefault();
                    id = id.Substring(0,id.Length-2);
                    result = id;
                    break;
                }
                pageContent = sr.ReadLine();
            }
        } 
        return result;
    }

    private async Task DownloadFiles()
    {
        await Task.WhenAll(sceneObjects.Where(u => u.unique).Select(s => s.DownloadFile()));
    } 

    public void OnApplicationQuit() {
        DeleteOld();
    }

    public void DeleteOld() {
        Debug.Log("Deleting");
        foreach(var name in sceneObjects.Where(u => u.unique).Select(s => s.name) ) {
            Debug.Log($"Deleting {name}");
            string model = ($"./Assets/Resources/dl/{name}");
            if(System.IO.Directory.Exists(model))
                System.IO.Directory.Delete(model,true);

            string zip = $"./Assets/Resources/dl/raw/{name}.zip";
            if(System.IO.File.Exists(zip))
                System.IO.File.Delete(zip);
        }
        sceneObject.namedModels.Clear();
    }
}
