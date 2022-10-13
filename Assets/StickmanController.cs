using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NNet))]
public class StickmanController : MonoBehaviour {
    private Vector3 HeadSP, BodySP, TorsoSP, LLegSP, LFootSP, RLegSP, RFootSP;
    private Quaternion HeadSR, BodySR, TorsoSR, LLegSR, LFootSR, RLegSR, RFootSR;
    private NNet nnet;

    [Range(-1f, 1f)]
    public float Torso, LLeg, LFoot, RLeg, RFoot;

    public float torsoSpeed = 40;
    public float speed = 40;
    public float shoveForce = 10;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float Goal;

    [Header("NNet Options")]
    public int Layers = 1;
    public int Nodes = 10;

    [Header("Parts")]
    public GameObject HeadObj, BodyObj, TorsoObj, LLegObj, LFootObj, RLegObj, RFootObj;

    [Header("Hinges")]
    public HingeJoint2D HeadJoint, BodyJoint, LLegJoint, LFootJoint, RLegJoint, RFootJoint;

    private Rigidbody2D torsoRB, lLegRB, rLegRB, lFootRB, rFootRB;

    private Vector2 stuckPosition;
    private float timer;

    //SENSORS
    private float lFootAngle, rFootAngle, lLegAngle, rLegAngle, torsoAngle, torsoVelocity, rLegVelocity, rFootVelocity, lLegVelcoity, lFootVelocity, height, lLegGrounded = -1, lFootGrounded = -1, rLegGrounded = -1, rFootGrounded = -1;

    private Vector2 LPTorso, LPLLeg, LPRLeg;

    public Text SpeedText;
    public Text DistanceText;
    public Text FitnessText;

    private Vector3 lastPosition;
    private float totalDistanceTraveled;
    private float avgSpeed;

    //private float aSensor, bSensor, cSensor, dSensor, eSensor;

    private void Awake() {
        //startposition = HeadObj.transform.position;
        nnet = GetComponent<NNet>();
        stuckPosition = TorsoObj.transform.position;

        torsoRB = TorsoObj.GetComponent<Rigidbody2D>();
        lLegRB = LLegObj.GetComponent<Rigidbody2D>();
        lFootRB = LFootObj.GetComponent<Rigidbody2D>();
        rLegRB = RLegObj.GetComponent<Rigidbody2D>();
        rFootRB = RFootObj.GetComponent<Rigidbody2D>();

        HeadSP = HeadObj.transform.position;
        HeadSR = HeadObj.transform.localRotation;

        BodySP = BodyObj.transform.position;
        BodySR = BodyObj.transform.localRotation;

        TorsoSP = TorsoObj.transform.position;
        TorsoSR = TorsoObj.transform.localRotation;

        LLegSP = LLegObj.transform.position;
        LLegSR = LLegObj.transform.localRotation;

        LFootSP = LFootObj.transform.position;
        LFootSR = LFootObj.transform.localRotation;

        RLegSP = RLegObj.transform.position;
        RLegSR = RLegObj.transform.localRotation;

        RFootSP = RFootObj.transform.position;
        RFootSR = RFootObj.transform.localRotation;

        LPTorso = TorsoObj.transform.position;
        LPLLeg = LLegObj.transform.position;
        LPRLeg = RLegObj.transform.position;

        //torsoRB.AddForce(Vector2.right * shoveForce);
    }

    public void GroundCheck(string id, float value) {
        if (id == "ll") {
            lLegGrounded = value;
        } else if (id == "lf") {
            lFootGrounded = value;
        } else if (id == "rl") {
            rLegGrounded = value;
        } else if (id == "rf") {
            rFootGrounded = value;
        }
    }

    public void ResetWithNetwork(NNet net) {
        nnet = net;
        Reset();
    }

    public void Reset() {
        timer = 0f;
        timeSinceStart = 0f;
        totalDistanceTraveled = 0f;
        avgSpeed = 0f;
        lastPosition = HeadObj.transform.position;
        stuckPosition = TorsoObj.transform.position;
        overallFitness = 0f;

        HeadObj.transform.position = HeadSP;
        HeadObj.transform.localRotation = HeadSR;

        BodyObj.transform.position = BodySP;
        BodyObj.transform.localRotation = BodySR;

        TorsoObj.transform.position = TorsoSP;
        TorsoObj.transform.localRotation = TorsoSR;

        LLegObj.transform.position = LLegSP;
        LLegObj.transform.localRotation = LLegSR;

        LFootObj.transform.position = LFootSP;
        LFootObj.transform.localRotation = LFootSR;

        RLegObj.transform.position = RLegSP;
        RLegObj.transform.localRotation = RLegSR;

        RFootObj.transform.position = RFootSP;
        RFootObj.transform.localRotation = RFootSR;

        //torsoRB.AddForce(Vector2.right * shoveForce);
    }

    public void Death() {
        FindObjectOfType<GeneticManager>().Death(overallFitness, nnet);
    }

    private void FixedUpdate() {
        InputSensors();
        lastPosition = HeadObj.transform.position;
        //lFootAngle, rFootAngle, lLegAngle, rLegAngle, torsoAngle, torsoVelocity, rLegVelocity, rFootVelocity, lLegVelcoity, lFootVelocity, height, lLegGrounded , lFootGrounded , rLegGrounded , rFootGrounded;
        (Torso, LLeg, LFoot, RLeg, RFoot) = nnet.RunNetwork(lFootAngle, rFootAngle, lLegAngle, rLegAngle, torsoAngle, torsoVelocity, rLegVelocity, rFootVelocity, lLegVelcoity, lFootVelocity, height, lLegGrounded, lFootGrounded, rLegGrounded, rFootGrounded);
        Move();

        timeSinceStart += Time.deltaTime;

        CalculateFitness();
    }

    private void CalculateFitness() {
        totalDistanceTraveled = BodyObj.transform.position.x;
        avgSpeed = BodyObj.transform.position.x / timeSinceStart;

        SpeedText.text = "Speed: " + avgSpeed.ToString("F2");
        DistanceText.text = "Distance: " + totalDistanceTraveled.ToString("F2");

        overallFitness = (totalDistanceTraveled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier);
        if (overallFitness <= 0) overallFitness = 0;

        FitnessText.text = "Fitness: " + overallFitness.ToString("F2");

        if (timeSinceStart > 20 && overallFitness < 10) {
            Death();
        }

        if (BodyObj.transform.position.x >= Goal) {
            Death();
        }
        if (timer >= 20) {
            if (Vector2.Distance(stuckPosition, TorsoObj.transform.position) < 0.5f) {
                Debug.Log("STUCK");
                Death();
            }
            stuckPosition = TorsoObj.transform.position;
            timer = 0;
        }
        timer += Time.deltaTime;
        //if (overallFitness >= 10000) {
            //save network
            //Death();
        //}
    }
    //Setting values of Sensors
    private void InputSensors() {
        lFootAngle = GetAngle(LFootObj.transform.rotation.eulerAngles.z);
        rFootAngle = GetAngle(RFootObj.transform.rotation.eulerAngles.z);
        lLegAngle = GetAngle(LLegObj.transform.rotation.eulerAngles.z);
        rLegAngle = GetAngle(RLegObj.transform.rotation.eulerAngles.z);
        torsoAngle = GetAngle(TorsoObj.transform.rotation.eulerAngles.z);

        torsoVelocity = GetVel(torsoRB.angularVelocity);
        lLegVelcoity = GetVel(lLegRB.angularVelocity);
        rLegVelocity = GetVel(rLegRB.angularVelocity);
        lFootVelocity = GetVel(lFootRB.angularVelocity);
        rFootVelocity = GetVel(rLegRB.angularVelocity);

        height = TorsoObj.transform.position.y + 1;
        if (height < -1) height = -1;
        if (height > 1) height = 1;
    }
    float GetAngle(float start) {
        float v;
        if (start <= 0) v = 0;
        else {
            v = start / 360;
        }
        return start;
    }
    float GetVel(float start) {
        float v;
        if (start > 1000) v = 1;
        else if (start < -1000) v = -1;
        else {
            v = start / 1000;
        }
        return v;
    }

    public void Move() {
        torsoRB.AddTorque(Torso * torsoSpeed *-1);
        JointMotor2D lLegM = LLegJoint.motor;
        lLegM.motorSpeed = LLeg * speed;
        LLegJoint.motor = lLegM;

        JointMotor2D lFootM = LFootJoint.motor;
        lFootM.motorSpeed = LFoot * speed;
        LFootJoint.motor = lFootM;

        JointMotor2D rLegM = RLegJoint.motor;
        rLegM.motorSpeed = RLeg * speed;
        RLegJoint.motor = rLegM;

        JointMotor2D rFootM = RFootJoint.motor;
        rFootM.motorSpeed = RFoot * speed;
        RFootJoint.motor = rFootM;
    }
}
