using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System.Text;
using System.IO;
using System;
using System.Linq;

public class n400comparison_models : MonoBehaviour
{
    private MLInput.Controller _controller;

    // n400 config:
    public Camera Camera;
    public GameObject models;
    public Text label;

    private List<GameObject> objects = new List<GameObject>();
    int nModels;

    IEnumerable<int> ranOrder;
    private float startTime;
    private float nextModel;
    private float nextPause;
    private float nextTrial;
    private float nextText;
    private int counter = 40;
    private string dir = "";

    private bool started = false;
    private bool complete = false;
    private int stage = 0;
    private int section = 1;
    private bool pause = false;

    List<bool> booleans = new List<bool>();
    List<bool> truth = new List<bool>();


    private int[] record;

    //private string filePath;
    private string dirPath = "assets/";
    private string mlDirPath = "/documents/C1/";


    // control_audiotest config:
    public GameObject cube;
    public GameObject sphere;

    AudioSource audioSource;

    public int position = 0;
    public int samplerate = 44100;
    public float frequency = 4400;

    private string filePath = "assets/n400comparison";
    private string mlFilePath = "/documents/C1/n400comparison";

    private List<String> times = new List<String>();
    private List<String> obj = new List<string>();
    private float t;

    // Start is called before the first frame update
    void Start()
    {
        //n400
        models.SetActive(false);
        for (int i = 0; i < models.transform.childCount; i++)
        {
            /*objects.Add(models.transform.GetChild(i).gameObject);
            objects[i].SetActive(false);*/
            models.transform.GetChild(counter).gameObject.SetActive(false);
        }
        nModels = models.transform.childCount;
        models.SetActive(true);

        print(nModels);
        record = new int[nModels * 2];

        ranOrder = Enumerable.Range(0, models.transform.childCount).OrderBy(x => UnityEngine.Random.value).ToList();
        Debug.Log(ranOrder.Count());

        for (int i = 0; i < nModels; i++)
        {
            bool randomBool = UnityEngine.Random.Range(0, 2) < 1;
            booleans.Add(randomBool);
        }

        foreach (bool b in booleans)
        {
            truth.Add(b);
        }
        foreach (bool b in booleans)
        {
            truth.Add(!b);
        }

        //audio
        AudioClip myClip = AudioClip.Create("MySinusoid", samplerate / 2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
        cube.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = myClip;

        /*MLInput.OnControllerButtonDown += OnButtonDown;
        _controller = MLInput.GetController(MLInput.Hand.Left);
        */

        startStudy();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            record[counter + (nModels * stage)] = 1;
        if (Input.GetKeyDown(KeyCode.Z))
            record[counter + (nModels * stage)] = 2;

        if (counter == models.transform.childCount && stage == 0)
        {
            stage = 1;
            label.text = "Done with stage 1. The next stage will start shortly.";
            started = false;
            startStudy();
        }
        

        if (started && counter < models.transform.childCount)
        {
            if (Time.time - startTime > nextModel)
            {
                print("model");

                label.text = "";
                models.transform.GetChild(ranOrder.ElementAt(counter)).gameObject.SetActive(true);

                nextModel += 5f;
            }
            else if (Time.time - startTime > nextPause)
            {
                print("pause");

                models.transform.GetChild(ranOrder.ElementAt(counter)).gameObject.SetActive(false);

                nextPause += 5f;
            }
            else if (Time.time - startTime > nextText)
            {
                print("text");

                if ((stage == 0) == truth[counter])
                {
                    label.text = models.transform.GetChild(ranOrder.ElementAt(counter)).gameObject.name;
                }
                else
                {
                    int ran = (int)UnityEngine.Random.Range(0, models.transform.childCount);
                    label.text = models.transform.GetChild(ran).gameObject.name;
                }
                audioSource.Play();
                times.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

                nextText += 5f;
            }
            else if (Time.time - startTime > nextTrial)
            {
                print("trial");

                label.text = "";

                nextTrial += 5f;
                counter++;
            }
            if (counter == models.transform.childCount && stage == 1) complete = true;
        }
        else if (complete == true)
        {
            complete = false;
            started = false;
            writeResults(Time.time);
            models.transform.GetChild(ranOrder.ElementAt(counter - 1)).gameObject.SetActive(false);
            label.text = "Done";
        }

    }


    void startStudy()
    {
        nextModel = 11f;
        nextPause = 12f;
        nextText = 13f;
        nextTrial = 14f;

        startTime = Time.time;
        counter = 0;
        started = true;
    }

    /* methods to handle button press on magic leap controller
    private void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
    {
        if (button == MLInput.Controller.Button.Bumper)
        {
            if (started == false)
            {
                startStudy();
                Debug.Log("study" + section + "started");
                startTime = Time.time;
                started = true;
            }
            else
            {
                record[counter*(stage+1)] = 1;
            }
            Debug.Log("button pressed");
        }
    }
    
    void OnDestroy()
    {
        MLInput.OnControllerButtonDown -= OnButtonDown;
    }*/

    private void writeResults(float t)
    {
        if (!Directory.Exists("assets/"))
        {
            print("assets not found");
            filePath = mlFilePath;
        }

        StreamWriter writer = new StreamWriter(filePath + t + ".txt");
        writer.WriteLine("Object, Time, Clicked, truth");
        for (int i = 0; i < times.Count; ++i)
        {
            writer.WriteLine(i + ", " + times[i] + ", " + record[i] + ", " + truth[i]);
        }
        writer.Close();
    }

    //Creates a sinewave
    public float CreateSine(int timeIndex, float frequency, float sampleRate)
    {
        return Mathf.Sin(2 * Mathf.PI * timeIndex * frequency / sampleRate);
    }

    void OnAudioRead(float[] data)
    {
        int count = 0;
        while (count < data.Length)
        {
            data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate);
            position++;
            count++;
        }
    }

    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }

}
