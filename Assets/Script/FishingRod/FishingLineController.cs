using System.Collections.Generic;
using UnityEngine;

public class FishingLineController : MonoBehaviour
{
    [SerializeField] private Transform whatTheRopeIsConnectedTo;
    [SerializeField] private Transform whatIsHangingFromTheRope;

    private LineRenderer lineRenderer;

    public List<Vector3> allRopeSections = new List<Vector3>();

    private float ropeLength = 2f;
    [SerializeField] private float readyRopeLength = 0.1f;
    [SerializeField] private float minRopeLength = 0.1f;
    [SerializeField] private float maxRopeLength = 20f;
    private float loadMass = 0.3f;
    private float winchSpeed = 2f;

    [SerializeField] private bool isFLoaterFishingRod = false;

    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegmentLength = 0.2f;
    private int segmentCount = 20;
    private float lineWidth = 0.01f;

    private Rigidbody rbwhatIsHangingFromTheRope;
    private Rigidbody rbCast;

    private SpringJoint springJoint;

    private bool isCastFishingRod = false;
    private Buoyancy buoyancyController = null;
    private bool isReadyFishingRod = true;

    void Start()
    {
        ropeLength = readyRopeLength;

        rbwhatIsHangingFromTheRope = whatIsHangingFromTheRope.GetComponentInParent<Rigidbody>();

        springJoint = whatTheRopeIsConnectedTo.GetComponentInParent<SpringJoint>();
        springJoint.anchor = whatTheRopeIsConnectedTo.localPosition;
        springJoint.connectedAnchor = whatIsHangingFromTheRope.localPosition;

        buoyancyController = whatIsHangingFromTheRope.GetComponentInParent<Buoyancy>();
        rbCast = rbwhatIsHangingFromTheRope;
        if (isFLoaterFishingRod)
        {
            rbCast = whatIsHangingFromTheRope.gameObject.GetComponentInParent<FishingLineController>().whatIsHangingFromTheRope.gameObject.GetComponentInParent<Rigidbody>();

            buoyancyController = rbCast.gameObject.GetComponent<Buoyancy>();
        }


        lineRenderer = GetComponentInParent<LineRenderer>();

        Vector3 ropeStartPoint = Vector3.zero;
        segmentCount = (int)(ropeLength * (1f / ropeSegmentLength)) + 1;
        for (int i = 0; i < segmentCount; i++)
        {
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y += ropeSegmentLength;
        }

        UpdateSpring();

        rbwhatIsHangingFromTheRope.mass = loadMass;
    }

    void Update()
    {
        if (isReadyFishingRod)
        {
            if (rbwhatIsHangingFromTheRope.velocity.magnitude > 5f)
                rbwhatIsHangingFromTheRope.drag = 0.5f;
            else
                rbwhatIsHangingFromTheRope.drag = 0f;


        }

        if (buoyancyController != null && buoyancyController.GetIsUnderWater() && isCastFishingRod)
        {
            isCastFishingRod = false;
            float distance = Vector3.Distance(whatTheRopeIsConnectedTo.position, whatIsHangingFromTheRope.position);

            ropeLength = distance + 1f;
            ropeLength = Mathf.Clamp(ropeLength, minRopeLength, maxRopeLength);


            UpdateSpring();
        }

        InitRope();

        DisplayRope();
    }

    private void FixedUpdate()
    {
        UpdateWinch();
        Simulation();
    }

    #region Rope
    private void InitRope()
    {
        float dist = ropeLength;

        int tempSegmentCount = (int)(ropeLength * (1f / ropeSegmentLength)) + 1;

        if (tempSegmentCount > ropeSegments.Count)
        {
            Vector3 ropeStartPoint = ropeSegments[ropeSegments.Count - 1].posNow;
            segmentCount = tempSegmentCount;
            ropeStartPoint.y += ropeSegmentLength;
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
        }
        else if (tempSegmentCount < ropeSegments.Count)
        {
            segmentCount = tempSegmentCount;
            ropeSegments.RemoveAt(ropeSegments.Count - 1);
        }
    }

    private void Simulation()
    {
        Vector3 forceGravity = new Vector3(0f, -5f, 0f);

        for (int i = 1; i < ropeSegments.Count; i++)
        {
            RopeSegment segment = ropeSegments[i];
            Vector3 velocity = segment.posNow - segment.posOld;
            segment.posOld = segment.posNow;

            RaycastHit hit;
            if (Physics.Raycast(segment.posNow, -Vector3.up, out hit, 0.1f))
            {
                if (hit.collider != null)
                {
                    velocity = Vector3.zero;
                    forceGravity.y = 0f;
                }
            }
            segment.posNow += velocity + forceGravity * Time.fixedDeltaTime;
            ropeSegments[i] = segment;
        }

        for (int i = 0; i < 20; i++)
        {
            ApplyConstraints();
        }
    }

    private void ApplyConstraints()
    {
        RopeSegment firstSegment = ropeSegments[0];
        firstSegment.posNow = whatTheRopeIsConnectedTo.position;
        ropeSegments[0] = firstSegment;

        RopeSegment endSegment = ropeSegments[ropeSegments.Count - 1];
        endSegment.posNow = whatIsHangingFromTheRope.position;
        ropeSegments[ropeSegments.Count - 1] = endSegment;

        for (int i = 0; i < ropeSegments.Count - 1; i++)
        {
            RopeSegment firstSeg = ropeSegments[i];
            RopeSegment seccondSeg = ropeSegments[i + 1];

            float dist = (firstSeg.posNow - seccondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - ropeSegmentLength);
            Vector3 changeDir = Vector3.zero;

            if (dist > ropeSegmentLength)
            {
                changeDir = (firstSeg.posNow - seccondSeg.posNow).normalized;
            }
            else if (dist < ropeSegmentLength)
            {
                changeDir = (seccondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector3 changeAmount = changeDir * error;

            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                ropeSegments[i] = firstSeg;
                seccondSeg.posNow += changeAmount * 0.5f;
                ropeSegments[i + 1] = seccondSeg;
            }
            else
            {
                seccondSeg.posNow += changeAmount;
                ropeSegments[i + 1] = seccondSeg;
            }
        }
    }
    private void DisplayRope()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] positions = new Vector3[ropeSegments.Count];
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            positions[i] = ropeSegments[i].posNow;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    private void UpdateSpring()
    {
        float density = 7750f;
        float radius = 0.02f;
        float volume = Mathf.PI * Mathf.Pow(radius, 2) * ropeLength;
        float ropeMass = volume * density + loadMass;

        float ropeForce = ropeMass * 9.81f;
        float kRope = 1000f;

        springJoint.spring = kRope * 0.1f;
        springJoint.damper = kRope * 0.05f;
        springJoint.maxDistance = ropeLength;
    }

    #endregion

    public void Hooking()
    {
        bool hasChangedRope = false;


        if (Input.GetKey(KeyCode.H) && ropeLength > minRopeLength)
        {
            ropeLength -= winchSpeed * 2;
            InitRope();

            hasChangedRope = true;
            rbwhatIsHangingFromTheRope.WakeUp();

        }

        if (hasChangedRope)
        {
            ropeLength = Mathf.Clamp(ropeLength, minRopeLength, maxRopeLength);
            UpdateSpring();
        }
    }
    public void UpdateWinch()
    {
        bool hasChangedRope = false;

        if (Input.GetKey(KeyCode.I) && ropeLength < maxRopeLength)
        {
            ropeLength += winchSpeed * Time.deltaTime;
            InitRope();

            hasChangedRope = true;
            rbwhatIsHangingFromTheRope.WakeUp();

        }
        else if (Input.GetKey(KeyCode.U) && ropeLength > minRopeLength)
        {
            ropeLength -= winchSpeed * Time.deltaTime;
            InitRope();

            hasChangedRope = true;
            rbwhatIsHangingFromTheRope.WakeUp();

        }

        if (hasChangedRope)
        {
            ropeLength = Mathf.Clamp(ropeLength, minRopeLength, maxRopeLength);
            UpdateSpring();
        }
    }

    public void CastFishingRod(float percentCast, Vector3 vec)
    {
        if (isCastFishingRod)
            return;

        float castPower = 1500f;
        float currentCastPower = castPower * percentCast / 100f;

        float maxCastLength = 50f;
        ropeLength = maxCastLength * percentCast / 100f;
        ropeLength = Mathf.Clamp(ropeLength, minRopeLength, maxRopeLength);

        InitRope();
        UpdateSpring();

        rbCast.AddForce((vec) * currentCastPower);

        isCastFishingRod = true;
    }

    public bool GetIsReady()
    {
        if (isFLoaterFishingRod)
        {
            if (ropeLength + rbwhatIsHangingFromTheRope.gameObject.GetComponent<FishingLineController>().GetRopeLength()
                <= readyRopeLength)
                return true;
            return false;

        }

        if (ropeLength <= readyRopeLength)
            return true;
        return false;
    }

    public void SetIsReadyFishingRod(bool isReady)
    {
        isReadyFishingRod = isReady;
    }

    #region Floater

    public float GetRopeLength()
    {
        return ropeLength;
    }

    public void SetFishingRodFloaterDepth(float val)
    {
        ropeLength = readyRopeLength - val;
        InitRope();
        UpdateSpring();
    }

    public void UpdateFloaterDepth()
    {
        bool hasChangedRope = false;

        if (Input.GetKey(KeyCode.P) && ropeLength < maxRopeLength)
        {
            ropeLength += winchSpeed * Time.deltaTime;
            InitRope();

            hasChangedRope = true;
            rbwhatIsHangingFromTheRope.WakeUp();

        }
        else if (Input.GetKey(KeyCode.O) && ropeLength > minRopeLength)
        {
            ropeLength -= winchSpeed * Time.deltaTime;
            InitRope();

            hasChangedRope = true;
            rbwhatIsHangingFromTheRope.WakeUp();

        }

        if (hasChangedRope)
        {
            ropeLength = Mathf.Clamp(ropeLength, minRopeLength, maxRopeLength);
            UpdateSpring();
        }
    }
    #endregion


    public struct RopeSegment
    {
        public Vector3 posNow;
        public Vector3 posOld;

        public RopeSegment(Vector3 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }
}
