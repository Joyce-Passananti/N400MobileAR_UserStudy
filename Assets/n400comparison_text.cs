using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System.Text;
using System.IO;
using System;
using System.Linq;

public class n400comparison_text : MonoBehaviour
{
    private MLInput.Controller _controller;

    // n400 config:
    public Camera Camera;
    public Text label;

    public bool scene;

    private List<GameObject> objects = new List<GameObject>();
    private List<String> labels = new List<String>();
    private List<String> labels2 = new List<String>();

    int nWords;

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
    private List<float> times = new List<float>();
    private List<string> words = new List<string>();
    private int[] record;
    private float[] responseTime;

    //private string filePath;
    private string dirPath = "assets/";
    private string mlDirPath = "/documents/C1/";

    AudioSource audioSource;

    public int position = 0;
    public int samplerate = 44100;
    public float frequency = 4400;

    private string textFilePath = "assets/word-word_label.txt";
    private string textFilePath2 = "assets/word-word_association.txt";
    private string ml_textFilePath = "/documents/C1/n400comparison/word-word_label.txt";
    private string ml_textFilePath2 = "/documents/C1/n400comparison/word-word_association.txt";

    private string filePath = "assets/n400comparison";
    private string mlFilePath = "/documents/C1/n400comparison";

    // Start is called before the first frame update
    void Start()
    {
        // read words from text files into labels array
        if (!Directory.Exists("assets/"))
        {
            textFilePath = ml_textFilePath;
            textFilePath2 = ml_textFilePath2;
        }

        const Int32 BufferSize = 128;
        using (var fileStream = File.OpenRead(textFilePath))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        {
            String line;
            while ((line = streamReader.ReadLine()) != null)
            {
                labels.Add(line);
            }
        }
        if (scene)
            labels2 = labels;
        else
        {
            using (var fileStream = File.OpenRead(textFilePath2))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    labels2.Add(line);
                }
            }
        }

        nWords = labels.Count;
        record = new int[nWords * 2];
        responseTime = new float[nWords * 2];

        ranOrder = Enumerable.Range(0, nWords).OrderBy(x => UnityEngine.Random.value).ToList();
        Debug.Log(ranOrder.Count());

        for (int i = 0; i < nWords; i++)
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
        AudioClip myClip = AudioClip.Create("MySinus oid", samplerate / 2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = myClip;

        /* Handle magic leap controller
         * MLInput.OnControllerButtonDown += OnButtonDown;
        _controller = MLInput.GetController(MLInput.Hand.Left);
        */
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            if (counter == 0)
                startStudy();
        if (Input.GetKeyDown(KeyCode.M))
        {
            record[counter + (nWords * stage) - 1] = 1;
            responseTime[counter + (nWords * stage) - 1] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - times[times.Count - 1];
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            record[counter + (nWords * stage) - 1] = 2;
            responseTime[counter + (nWords * stage) - 1] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - times[times.Count - 1];
        }

        if (counter == nWords && stage == 0)
        {
            stage = 1;
            label.text = "Done with stage 1. Press 'Space' to begin the next stage. ";
            started = false;
            counter = 0;
        }

        if (started && counter < nWords)
        {
            // show word 1s
            if (Time.time - startTime > nextModel)
            {
                label.text = labels[ranOrder.ElementAt(counter)];
                words.Add(labels[ranOrder.ElementAt(counter)]);
                nextModel += 5f;
            }

            // pause 1s
            else if (Time.time - startTime > nextPause)
            {
                label.text = "";
                nextPause += 5f;
            }

            // show word 1s
            else if (Time.time - startTime > nextText)
            {
                if ((stage == 0) == truth[counter])
                {
                    label.text = labels2[ranOrder.ElementAt(counter)];
                }
                else
                {
                    int ran = (int)UnityEngine.Random.Range(0, nWords);
                    label.text = labels2[ran];
                }
                audioSource.Play();
                times.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                nextText += 5f;
            }

            // pause 2s
            else if (Time.time - startTime > nextTrial)
            {
                label.text = "";

                nextTrial += 5f;
                counter++;
            }

            // check to see if study complete
            if (counter == nWords && stage == 1) complete = true;
        }

        // ends the study + output results
        else if (complete == true)
        {
            complete = false;
            started = false;
            writeResults(Time.time);
            label.text = "Done";
        }

    }

    // start study section
    void startStudy()
    {
        nextModel = 1f;
        nextPause = 2f;
        nextText = 3f;
        nextTrial = 4f;

        startTime = Time.time;
        counter = 0;
        started = true;
    }

    // output results to textfile
    private void writeResults(float t)
    {
        if (!Directory.Exists("assets/"))
        {
            filePath = mlFilePath;
        }

        StreamWriter writer = new StreamWriter(filePath + t + ".txt");
        writer.WriteLine("Word, Time, RespponseTime, Clicked, truth");
        for (int i = 0; i < times.Count; ++i)
        {
            writer.WriteLine(words[i] + ", " + times[i].ToString() + ", "  + responseTime[i] + ", " + record[i] + ", " + truth[i]);
        }
        writer.Close();
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
