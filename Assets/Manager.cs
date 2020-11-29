using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Manager : MonoBehaviour
{

    public int PopulationSize = 300;
    public GameObject startPos;
    public GameObject robot;
    private float maxGentime = 15;
    private float currentGenTime = 0;
    public float generation = 1;
    public int maxFittness;

    // Start is called before the first frame update
    void Start()
    {
        Enumerable.Range(0, PopulationSize).ToList().ForEach(x =>
        {
            Instantiate(robot, startPos.transform.localPosition, Quaternion.identity);
        });
    }

    // Update is called once per frame
    void Update()
    {
        currentGenTime += Time.deltaTime;
       if(currentGenTime> maxGentime)
        {
            currentGenTime = 0;
            SetupNextGeneration();
        }
    }

    void SetupNextGeneration()
    {
        //disable any cars that are not disabled already.
        var oldRobots = Resources.FindObjectsOfTypeAll(typeof(RobotController)).Where(x => (x as RobotController).gameObject.scene.name != null).Cast<RobotController>().ToList();
        oldRobots.ForEach(x => x.gameObject.SetActive(false));

        //sort them.
        var sorted = oldRobots.OrderByDescending(x => x.Fittness).Select(y => y.brain).ToList();
        maxFittness = (int)oldRobots.OrderByDescending(x => x.Fittness).FirstOrDefault().Fittness;
        var topHalf = sorted.Take(sorted.Count() / 2).ToList();
        //copy the nets
        var newNets = topHalf.Select(x => neuralnet1.NN.Serialize(x)).Select(json => neuralnet1.NN.Deserialize(json)).ToList(); ;
        newNets.ForEach(x => x.Mutate(.2, .5));

        //create a new population using the old nets and new nets
        //concat list

        //delete the old cars before making more.
        oldRobots.ForEach(x => GameObject.DestroyImmediate(x.gameObject));

        var latestNets = topHalf.Concat(newNets).ToList();

        Enumerable.Range(0, PopulationSize).ToList().ForEach(x =>
        {
            var obj = Instantiate(robot, startPos.transform.localPosition, Quaternion.identity);
            obj.GetComponent<RobotController>().brain = latestNets[x];
        });

        generation++;
        //
    }

}
