using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System.Text;
using System.IO;
using System;
using System.Linq;

public class p300_audiotest : MonoBehaviour
{
    // n400 config:
    public Camera Camera;
    public GameObject models;
    public Text label;

    private List<GameObject> objects = new List<GameObject>();

    private float startTime;
    private float nextModel;
    private float nextText;
    private int counter;
    private string dir = "";

    private bool started = false;
    private bool complete = false;
    private int stage = 1;

    private int[] record;
    //private string filePath;
    private string dirPath = "assets/";
    private string mlDirPath = "/documents/C1/";


    // control_audiotest config:
    public GameObject cube;
    public GameObject sphere;
    public int trials;

    AudioSource audioSource;

    public int position = 0;
    public int samplerate = 44100;
    public float frequency = 4400;

    private float timerInc = 1.0f;
    private float flash = 0.6f;
    private float next = 1.0f;
    private float variation = 0.0f;

    private string filePath = "assets/p300audiotestdata";
    private string mlFilePath = "/documents/C1/p300audiotestdata.txt";

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
            objects.Add(models.transform.GetChild(i).gameObject);
            objects[i].SetActive(false);
        }

        objects = objects.OrderBy(x => UnityEngine.Random.value).ToList();
        for (int i = 0; i < objects.Count; i++)
        {
            Debug.Log(objects[i].name);
        }

        //models.SetActive(true);
        nextModel = 1f;
        nextText = 0f;
        counter = 0;

        startTime = Time.time;
        started = true;

        //audio
        AudioClip myClip = AudioClip.Create("MySinusoid", samplerate / 2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
        cube.SetActive(false);
        sphere.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = myClip;
    }

    // Update is called once per frame
    void Update()
    {
        //n400
        if (started && counter < trials)
        {
            variation = UnityEngine.Random.Range(0.1f, 0.2f);
            flash = 0.6f + variation;
            if (Time.time > next - flash)
            {
                if (counter > 0)
                {
                    //objects[counter].SetActive(true);
                    cube.SetActive(false);
                    sphere.SetActive(false);

                }
                label.text = "";
                nextText += 1f;
            }
            if (Time.time > next)
            {
                //label.text = objects[counter].name + dir;
                //objects[counter].SetActive(false);
                next += timerInc;

                counter++;
                audioSource.Play();
                if (UnityEngine.Random.Range(1, 10) > 8)
                {
                    sphere.SetActive(true);
                    obj.Add("sphere");
                }
                else
                {
                    cube.SetActive(true);
                    obj.Add("cube");
                }
                times.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
            }
            if (counter == trials) complete = true;
        }
        else if (complete == true)
        {
            complete = false;
            started = false;
            writeResults();
            label.text = "Press contol button \n to start the next stage";
        }

        //audio
        /*if (Time.time > 300)
        {
            writeResults();
        }
        else if (Time.time > next)
        {
            next += timerInc;

            cube.SetActive(true);
            obj.Add("cube");

            times.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
            audioSource.Play();
        }
        else if (Time.time > next - flash)
        {
            cube.SetActive(false);
        }*/
    }
    private void writeResults()
    {
        if (!Directory.Exists("assets/"))
        {
            print("assets not found");
            filePath = mlFilePath;
        }
        //System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
        //StreamWriter writer = new System.IO.StreamWriter(fs);
        StreamWriter writer = new StreamWriter(filePath);

        writer.WriteLine("Object, Time");
        for (int i = 0; i < obj.Count; ++i)
        {
            writer.WriteLine(obj[i] + ", " + times[i]);
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
