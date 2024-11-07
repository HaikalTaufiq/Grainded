using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    [SerializeField] private List<Floaters> floaters = new List<Floaters>();
    [SerializeField] private float waterLine = 0f;
    [SerializeField] private float underWaterDrag = 3f;
    [SerializeField] private float underWaterAngularDrag = 1f;
    [SerializeField] private float defaultDrag = 0f;
    [SerializeField] private float defaultAngularDrag = 0.05f;

    private bool isUnderWater = false;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        isUnderWater = false;
        for (int i = 0; i < floaters.Count; i++)
        {
            if (floaters[i].FloaterUpdate(rb, waterLine))
                isUnderWater = true;
        }
        SetState(isUnderWater);
    }

    private void SetState(bool isUnderWater)
    {
        if (isUnderWater)
        {
            rb.drag = underWaterDrag;
            rb.angularDrag = underWaterAngularDrag;
        }
        else
        {
            rb.drag = defaultDrag;
            rb.angularDrag = defaultAngularDrag;
        }
    }

    public bool GetIsUnderWater()
    {
        return isUnderWater;
    }
}

[System.Serializable]
public class Floaters
{
    [SerializeField] private float floatingPower = 20f;
    [SerializeField] private Transform floater;

    private bool underWater;

    public bool FloaterUpdate(Rigidbody rb, float waterLine)
    {
        float difference = floater.position.y - waterLine;

        if (difference < 0)
        {
            rb.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(difference), floater.position, ForceMode.Force);
            if (!underWater)
                underWater = true;
        }
        else if (underWater)
        {
            underWater = false;
        }
        return underWater;
    }
}
