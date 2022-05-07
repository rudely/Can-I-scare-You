using Fove.Unity;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
public class Patrol : FOVEBehavior
{
    public Transform waypoint; //change to s for set waypoints
    private int currentwp;
    private int random;
    private float speed = 2.8f;
    private float wait;
    public float startWait;

    public Animator anim;

    //delimiting params

    public float minX;
    public float minY;
    public float maxX;
    public float maxY;

    // inner and outter circles
    public float R;
    public float r;

    // type of spider
    public float emotionGen;
    public float emotion;

    // probability setters
    public float where;
    public float velocity;

    //throwaway trigonometry maths variables
    public float m, cR, sR;

    //throwaway waypoint copy
    public float tempwpx, tempwpy;
    public float tempcameraDist;

    //fucking camera
    Camera mainCam;

    //conditional boolean
    public bool intersect;

    // EYE FUCKING TRACKING
    private Collider my_collider;

    //output logging
    string filename = "";
    public int n;
    StreamWriter tw;
    public int fn = 0;




    void Start()
    {
        n = 0;

        my_collider = GetComponent<Collider>();
        wait = startWait;
        anim = GetComponent<Animator>();
        mainCam = Camera.main;
        mainCam.enabled = true;

        //TO DO: implement starting points for each type of spider

        waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));
        tempwpx = waypoint.position.x;
        tempwpy = waypoint.position.z;
        emotionGen = Random.Range(1, 100);

        if (emotionGen <= 60)
                emotion = 1;
        else
        if (emotionGen <= 90 && emotionGen > 60)
            emotion = 2;
        else
        if (emotionGen > 90 && emotionGen <= 100)
            emotion = 3;
        R = 7;
        r = 1.0f;
        velocity = 0.0f;
        filename = Application.dataPath + "/test" + fn + ".csv";

        while (File.Exists(filename))
        {
            fn += 1;
            filename = Application.dataPath + "/test" + fn + ".csv";
        }
        tw = new StreamWriter(filename, false);
        tw.WriteLine("Type, x pos, y pos, look at");
        tw.Close();
       // TextWriter tw = new StreamWriter(filename, true);
    }


    void Update()
    {
        n += 1;

        var isGazed = FoveSettings.AutomaticObjectRegistration
            ? FoveManager.GetGazedObject() == gameObject // use the Object detection API
            : FoveInterface.Gazecast(my_collider); // Manually perform gaze cast on scene colliders
        bool wasGazed = false;

        anim.SetFloat("speed", Mathf.Abs(speed));
        if( isGazed && wasGazed == false )
        {
            tw = new StreamWriter(filename, true);
            tw.WriteLine(emotion + "," + transform.position.x + "," + transform.position.z + "," + Time.time );
            tw.Close();
            wasGazed = true;
        }
        else 
        if( isGazed == false && wasGazed == true)
        {
            tw = new StreamWriter(filename, true);
            tw.WriteLine(emotion + "," + transform.position.x + "," + transform.position.z + "," + Time.time);
            tw.Close();
            wasGazed = false;
        }
        else
        if (n % 10 == 0)
        {
            tw = new StreamWriter(filename, true);
            tw.WriteLine(emotion + "," + transform.position.x + "," + transform.position.z + "," + "no");
            tw.Close();
        }

        if (emotion == 3)
        {
            //dumb
            //No regards of human x
            //moves random speed 
            //moves random distances x
            //moves rantom places x 
            //has random wait times x
            if (isGazed == false)
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoint.position, speed * Time.deltaTime);
                transform.LookAt(waypoint.position);


                anim.SetFloat("speed", Mathf.Abs(speed));

                if (Vector3.Distance(transform.position, waypoint.position) < 0.2f)
                    if (wait <= 0)
                    {

                        speed = Random.Range(2.0f, 3.0f);
                        anim.speed = speed - 1.0f;
                        waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));
                        wait = Random.Range(0, 0.9f);
                    }
                    else
                    {
                        anim.SetFloat("speed", 0);
                        wait -= Time.deltaTime;
                    }
            }
            else
            {
               
               
                tw = new StreamWriter(filename, true);
                tw.WriteLine(emotion + "," + transform.position.x + "," + transform.position.z + "," + Time.time);
                tw.Close();
                anim.SetFloat("speed", 0);
                //transform.LookAt(mainCam.transform.position);
            }

        }
        else
        if (emotion == 2)
        {
            //aggressive
            //moves fast towards human  (velocity)      X ( LINEAR AND CONSTANT )
            //moves max acceleration when furthest      X
            //normal speed when in R                    X
            //FAST towards r
            //if in R, low chance of it coming in r     X
            //if in r, high chance of it going in R     X
            //wants to stay in r                        X
            //disregards human presence                 X
            //walks small distances around human        REDACTED due to DiST REQUIREMENTS TO STAY IN r ARE SMALL
            //short wait time everywhere, erratic
            if (isGazed == false)
            {

                transform.position = Vector3.MoveTowards(transform.position, waypoint.position, speed * Time.deltaTime);
                transform.LookAt(waypoint.position);


                anim.SetFloat("speed", Mathf.Abs(speed));

                intersect = true;
                if (Vector3.Distance(transform.position, waypoint.position) < 0.2f)
                    if (wait <= 0)
                    {
                        tempwpx = waypoint.position.x;
                        tempwpy = waypoint.position.z;
                        waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));


                        //current distance between the spider and camera, before moving to new wp
                        tempcameraDist = Mathf.Sqrt(Mathf.Pow(tempwpx - mainCam.transform.position.x, 2) + Mathf.Pow(tempwpy - mainCam.transform.position.z, 2));

                        where = Random.Range(0, 100); // 70 chance it wants to stay in r, 30 in R
                        intersect = true;

                        if (where <= 88)
                        {
                            //in r
                            //fast speed
                            speed = Random.Range(2.9f, 3.3f);
                            anim.speed = speed;

                            //short decision time
                            wait = Random.Range(0.1f, 0.2f);


                            while (Vector3.Distance(mainCam.transform.position, waypoint.position) < r)
                                waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));

                        }
                        else
                        {
                            //in R
                            //normal speed
                            speed = Random.Range(2.25f, 2.45f);
                            anim.speed = speed;

                            //always has short wait time
                            wait = Random.Range(0.1f, 0.3f);


                            while (Vector3.Distance(mainCam.transform.position, waypoint.position) >= r && Vector3.Distance(mainCam.transform.position, waypoint.position) < R)

                                waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));

                        }

                        //linear velocity, can be made quadratic, ideally would follow a sinusoidal scaling


                        if (tempcameraDist >= r)
                        {
                            //case where spider is in R
                            //if moving further
                            if (tempcameraDist < Vector3.Distance(waypoint.position, mainCam.transform.position))
                                velocity = 0.0f;
                            else
                            //if moving closer
                            {
                                cR = R - r;
                                sR = tempcameraDist - r;
                                velocity = sR / cR;
                            }

                            //compute speed accordingly
                            speed += velocity + 0.2f;

                        }
                        else
                        {
                            //case where spider is in r
                            //if moving further
                            if (tempcameraDist < Vector3.Distance(waypoint.position, mainCam.transform.position))
                            {
                                //if moving out of r
                                if (Vector3.Distance(waypoint.position, mainCam.transform.position) >= r)
                                    velocity = 0.0f;

                                //if moving within r
                                else
                                    velocity = 1 - tempcameraDist / r;
                            }

                            else
                            //if moving closer 
                            {
                                velocity = 1 - tempcameraDist / r;
                            }

                            //spider is fast to move in
                            speed += velocity;

                        }
                        anim.speed = 1 ;


                    }
                    else
                    {
                        anim.SetFloat("speed", 0);
                        wait -= Time.deltaTime;
                    }
            }
            else
            {
                anim.SetFloat("speed", 0);
            }

        }


        else
        if (emotion == 1)
        {
            //afraid
            //moves slow towards human  (velocity)      X ( LINEAR AND CONSTANT )
            //moves at max acceleration when in r       X
            //slowest speed when in R                   X
            //if in R, low chance of it coming in r     X
            //if in r, high chance of it going in R     X
            //wants to stay in R                        X
            //will never walk thru human                X
            //walks small distances around human        REDACTED due to DOST REQUIREMENTS TO STAY IN r ARE SMALL
            //high wait time in R, small wait time in r X
            if (isGazed == false)
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoint.position, speed * Time.deltaTime);
                transform.LookAt(waypoint.position);


                anim.SetFloat("speed", Mathf.Abs(speed));

                intersect = true;
                if (Vector3.Distance(transform.position, waypoint.position) < 0.2f)
                    if (wait <= 0)
                    {

                        tempwpx = waypoint.position.x;
                        tempwpy = waypoint.position.z;
                        waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));

                        tempcameraDist = Mathf.Sqrt(Mathf.Pow(tempwpx - mainCam.transform.position.x, 2) + Mathf.Pow(tempwpy - mainCam.transform.position.z, 2));

                        where = Random.Range(0, 100); // 70 chance it wants to stay in R, 30 in r
                        intersect = true;

                        if (where <= 90)
                        {
                            //in R
                            //slower speed
                            speed = Random.Range(2.1f, 2.5f);
                            anim.speed = speed;

                            //longer decision time
                            wait = Random.Range(0.6f, 1f);


                            while (Vector3.Distance(mainCam.transform.position, waypoint.position) < r || intersect == true)
                            {
                                waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));

                                m = (tempwpx - waypoint.position.x) / (tempwpy - waypoint.position.z);
                                if (mainCam.transform.position.y == m * (mainCam.transform.position.x - waypoint.position.x) + waypoint.position.y)
                                    intersect = false;
                                else
                                    intersect = true;

                            }
                        }
                        else
                        {
                            //in r
                            //faster speed
                            speed = Random.Range(2.9f, 3.2f);
                            anim.speed = speed;

                            //shorter wait time
                            wait = Random.Range(0.1f, 0.2f);


                            while ((Vector3.Distance(mainCam.transform.position, waypoint.position) < R && Vector3.Distance(mainCam.transform.position, waypoint.position) >= r) || intersect == true)
                            {
                                waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));
                                m = (tempwpx - waypoint.position.x) / (tempwpy - waypoint.position.z);
                                if (mainCam.transform.position.y == m * (mainCam.transform.position.x - waypoint.position.x) + waypoint.position.y)
                                    intersect = false;
                                else
                                    intersect = true;

                            }


                        }

                        //linear velocity, can be made quadratic, ideally would follow a sinusoidal scaling

                        if (tempcameraDist >= r)
                        {
                            //case where spider is in R to begin w/
                            //if moving further-
                            if (tempcameraDist < Vector3.Distance(waypoint.position, mainCam.transform.position))
                            {
                                cR = R - r;
                                sR = tempcameraDist - r;
                                velocity = sR / cR;
                            }
                            else
                                //if moving closer
                                velocity = -0.5f;

                            //compute speed accordingly
                            speed += velocity + 0.2f;
                        }
                        else
                        {

                            //case where spider is in r
                            //if moving further
                            if (tempcameraDist < Vector3.Distance(waypoint.position, mainCam.transform.position))
                            {
                                //if moving out of r
                                if (Vector3.Distance(waypoint.position, mainCam.transform.position) >= r)
                                    velocity = 1 - tempcameraDist / r;

                                //if moving within r
                                else
                                    velocity = 0.2f;
                            }

                            else
                            //if moving closer 
                            {
                                velocity = 0.2f;
                            }

                            //spider is fast to move in
                            speed += velocity;
                        }


                    }
                    else
                    {
                        anim.SetFloat("speed", 0);
                        wait -= Time.deltaTime;
                    }
                anim.speed = 1;
            }
            else
            {
                anim.SetFloat("speed", 0);
            }
        }

    }

  
}
/*
* FULLY RANDOM MOVEMENT, CODE #2
* 
* transform.position = Vector3.MoveTowards(transform.position, waypoint.position, speed * Time.deltaTime);
    transform.LookAt(waypoint.position);

    anim.SetFloat("speed", Mathf.Abs(speed));

    if (Vector3.Distance(transform.position, waypoint.position) < 0.2f)
        if (wait <= 0)
        {
            waypoint.position = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY));
            wait = startWait;
        }
        else
        {
            anim.SetFloat("speed", 0);
            wait -= Time.deltaTime;
        }*/

/* 
* SET WAYPOINTS, CODE #1
* 
* private void Update()
    {
        Transform wp = waypoints[currentwp];
        if( Vector3.Distance(transform.position, wp.position) < 0.01f)
        {
            currentwp += 1;
            currentwp %= waypoints.Length;

        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, wp.position, speed * Time.deltaTime);
            transform.LookAt(wp.position);
        }
    }*/