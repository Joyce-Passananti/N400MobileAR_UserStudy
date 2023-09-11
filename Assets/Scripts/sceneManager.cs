using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine.XR;

public class sceneManager : MonoBehaviour
{
    public GameObject cube;
    public GameObject sphere;

    AudioSource audioSource;

    public int position = 0;
    public int samplerate = 44100;
    public float frequency = 4400;

    private float timerInc = 2.0f;
    private float flash = 0.8f;
    private float next = 2.0f;

    private string filePath = "assets/audiotestdata";
    private string mlFilePath = "/documents/C1/audiotestdata.txt";

    private List<String> times = new List<String>();
    private List<String> obj = new List<string>();
    private float t;
    // Start is called before the first frame update
    void Start()
    {
        AudioClip myClip = AudioClip.Create("MySinusoid", samplerate/2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
        cube.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = myClip;
    }

    // Update is called once per frame
    void Update()
    {
      
        if (Time.time > 300)
        {
            writeResults();
        }
        else if (Time.time > next)
        {
            next += timerInc;

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
            audioSource.Play();
        }
        else if (Time.time > next - flash)
        {
            cube.SetActive(false);
            sphere.SetActive(false);
        }
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
