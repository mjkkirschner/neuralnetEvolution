using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using neuralnet1;

public class RobotController : MonoBehaviour
{

    Vector3[] localRayVectors = new Vector3[] { new Vector3(-1, 0, 0), new Vector3(-1, 0, 1), new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0) };
    double[] distances = new double[5];
    public NN brain = new NN(new int[] { 5, 4, 2 });
    Dictionary<string,GameObject> checkpoints = new Dictionary<string,GameObject>();
    public double Fittness { get; private set; }
    private float maxdist = 10;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //everyframe we'll execute the neural net with the 5 inputs
        RaycastHit hit;
        for (var i = 0; i < localRayVectors.Length; i++)
        {
            var dir = localRayVectors[i];
            // Does the ray intersect any objects excluding the player layer, or car layer
            var layerMask = ~((1 << 2) | (1 << 8));

            if (Physics.Raycast(transform.position, transform.TransformDirection(dir), out hit, maxdist,layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(dir) * hit.distance, Color.red);
                //    Debug.Log("Did Hit");
                distances[i] = (maxdist - hit.distance / maxdist);
            }
            else
            {
                distances[i] = 0;

            }
        }

        //inputs calculated - lets feed into network
        brain.Layers[0].Neurons[0].Value = distances[0];
        brain.Layers[0].Neurons[1].Value = distances[1];
        brain.Layers[0].Neurons[2].Value = distances[2];
        brain.Layers[0].Neurons[3].Value = distances[3];
        brain.Layers[0].Neurons[4].Value = distances[4];

        brain.FF();

        //get output values.

        var out1 = brain.Layers[2].Neurons[0].Value;
        var out2 = brain.Layers[2].Neurons[1].Value;

        //control motion using outputs.

       // Debug.Log(out1);
       // Debug.Log(out2);

        //first output is angle to rotate the current vector by.
        var rotation = out1;//Random.Range(-1.0f, 1.0f);
        var rotDeg = rotation * (180.0 / Mathf.PI) * 1.0f;
        var scale = (float)out2;// Random.Range(-.1f, .1f);
                         //TODO if movement vec gets very small or 0 this
                         //acumulation will fail... reset? or start with normalized vector?
        transform.Rotate(0, (float)rotDeg, 0, Space.World);

        //scale the vector by output 2
        //move by the vector
        this.transform.position += this.transform.forward * scale *.1f;

        CalculateFittness();

    }

    void CalculateFittness()
    {
       Fittness = 0;
       foreach(var checkpoint in checkpoints)
        {
            Fittness++;
        }
    }

    void OnTriggerEnter(Collider col)
    {
       // Debug.Log("PASSED A CHECKPOINT");

        if (col.gameObject.name.Contains("checkpoint"))
        {
            var key = col.gameObject.name;

            if (!checkpoints.ContainsKey(key))
            {
                //add it
                //the value doesnt really matter
                checkpoints.Add(key, null);
            }
            else
            {
                //do nothing
            }

            //lap over.
            //if (key.Contains("5"))
            //{
            //    checkpoints.Clear();
           // }

        }
    }

    void OnCollisionEnter(Collision col)
    {
       // Debug.Log("HIT");
       // Debug.Log(col.gameObject.name);

        var colWithWall = (col.transform.parent?.gameObject.name.Contains("raceTrackParts")) ?? false;
        if (colWithWall)
        {
            //we have died!
            this.enabled = false;
            this.gameObject.SetActive(false);
        }


    }




}
