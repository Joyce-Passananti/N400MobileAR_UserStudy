using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using System.Text;
using System.IO;
using System;
using System.Linq;


public class control : MonoBehaviour
{
    public Camera Camera;
    public GameObject models;
    public Text label;
    public Button start;

    private MLInput.Controller _controller;
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
    private string filePath;
    private string dirPath = "assets/";
    private string mlDirPath = "/documents/C1/";

    private Vector3 heading;
    private List<Vector3> eyeTracking = new List<Vector3>();
    private List<Vector3>[] eyeTrackingData;

    void Start()
    {
        models.SetActive(false);
        for (int i = 0; i < models.transform.childCount; i++)
        {
            objects.Add(models.transform.GetChild(i).gameObject);
            objects[i].SetActive(false);
        }
        /*for (int i = 0; i < objects.Count; i++)
        {
            int r = i;
            do{
                r = UnityEngine.Random.Range(0, models.transform.childCount);
            }while (r == i);
            objects.Add(models.transform.GetChild(i).gameObject);
            objects[i + models.transform.childCount].name = objects[r].name;
            objects[i + models.transform.childCount].SetActive(false);
        }*/
        objects = objects.OrderBy(x => UnityEngine.Random.value).ToList();
        for (int i = 0; i < objects.Count; i++)
        {
            Debug.Log(objects[i].name);
        }
        MLInput.OnControllerButtonDown += OnButtonDown;
        _controller = MLInput.GetController(MLInput.Hand.Left);
        if (!Directory.Exists("assets/"))
            dirPath = mlDirPath;
    }

    // Update is called once per frame
    void Update()
    {
        heading = MLEyes.FixationPoint - Camera.transform.position;
        //Debug.Log(heading.ToString("F6"));
        RaycastHit _hit;
        if (Physics.Raycast(Camera.transform.position, heading, out _hit))
        {
            //Debug.Log(_hit.point);
        }

        if (started && counter < models.transform.childCount)
        {
            if (Time.time - startTime > nextText)
            {
                if (counter > 0)
                {
                    if (stage == 2)
                    {
                        if (counter % 2 != 0)
                        { dir = "\n→"; }
                        else
                        { dir = "\n←"; }
                    }
                    objects[counter - 1].SetActive(false);
                    eyeTrackingData[counter - 1] = eyeTracking;
                    eyeTracking.Clear();
                }
                label.text = objects[counter].name + dir;
                nextText += 1f;
            }
            else if (Time.time - startTime > nextModel)
            {
                label.text = "";
                objects[counter].SetActive(true);
                nextModel += 2f;
                counter++;
            }
            eyeTracking.Add(heading);
            if (counter == models.transform.childCount) complete = true;
        }
        else if (complete == true)
        {
            complete = false;
            started = false;
            eyeTrackingData[counter - 1] = eyeTracking;
            writeResults();
            stage++;
            objects[counter - 1].SetActive(false);
            label.text = "Press contol button \n to start the next stage";
        }
    }

    private void OnButtonDown(byte controllerId, MLInput.Controller.Button button)
    {
        if (button == MLInput.Controller.Button.Bumper)
        {
            if (started == false)
            {
                startStudy(stage);
                Debug.Log("study" + stage + "started");
                startTime = Time.time;
                started = true;
            }
            else
            {
                record[counter] = 1;
            }
            Debug.Log("button pressed");
        }
    }
    private void writeResults()
    {
        StreamWriter writer = new StreamWriter(dirPath + filePath);

        writer.WriteLine("Object, Clicked, Eye tracking");
        for (int i = 0; i < models.transform.childCount; ++i)
        {
            writer.Write(objects[i].name + ", " + record[i] + ", ");
            for (int j = 0; j < eyeTrackingData[i].Count; ++j)
            {
                writer.Write(eyeTrackingData[i][j].ToString("F6"));
            }
            writer.WriteLine("");
        }
        writer.Close();
    }

    void OnDestroy()
    {
        MLInput.OnControllerButtonDown -= OnButtonDown;
    }

    private void startStudy(int stage)
    {
        if (stage == 1)
        {
            Debug.Log("stage1");
            for (int i = 0; i < objects.Count; i++)
            {
                //objects[i].transform.localPosition = new Vector3(0, 0, 0);
            }
            //label.GetComponent<RectTransform>().position = models.transform.position;
            label.text = "";

            record = new int[models.transform.childCount];
            eyeTrackingData = new List<Vector3>[models.transform.childCount];

            filePath = "data1.txt";
        }
        else if (stage == 2)
        {
            Debug.Log("stage2");
            int x = 500;
            for (int i = 0; i < objects.Count; i++)
            {
                x *= -1;
                objects[i].SetActive(false);
                objects[i].transform.localPosition = new Vector3(x, 0, 0);
            }
            label.GetComponent<RectTransform>().position = models.transform.position;
            label.text = "";

            record = new int[models.transform.childCount];
            eyeTrackingData = new List<Vector3>[models.transform.childCount];

            filePath = "data2.txt";

        }
        else if (stage == 3)
        {
            dir = "";
            Debug.Log("stage3");
            label.GetComponent<RectTransform>().LookAt(Camera.main.transform.position);
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].transform.localPosition = new Vector3(0, 0, 0);
            }
            label.GetComponent<RectTransform>().position = models.transform.position;
            label.text = "";

            record = new int[models.transform.childCount];
            eyeTrackingData = new List<Vector3>[models.transform.childCount];

            filePath = "data3.txt";
        }
        models.SetActive(true);
        nextModel = 1f;
        nextText = 0f;
        counter = 0;
    }

}
