using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicBalistic : MonoBehaviour
{
    public float maxStretch = 3.0f;
    public LineRenderer lineRenderer;
    public Rigidbody projectile;
    public Transform shotPoint;
    public float shotPower = 5.0f;

    private SpringJoint springJoint;
    private Rigidbody draggedProjectile;
    private Ray rayToMouse;
    private float maxStretchSqr;
    private float circleRadius;
    private bool clickedOn;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        springJoint = GetComponent<SpringJoint>();
        springJoint.connectedBody = null;

        maxStretchSqr = maxStretch * maxStretch;
        SphereCollider sphere = projectile.GetComponent<SphereCollider>();
        circleRadius = sphere.radius;
    }

    void Update()
    {
        if (clickedOn)
            Dragging();

        if (springJoint.connectedBody == null && clickedOn)
            Release();

        if (springJoint.connectedBody != null)
            lineRenderer.SetPosition(0, shotPoint.position);
        else
            lineRenderer.enabled = false;
    }

    void Dragging()
    {
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        Vector3 catapultToMouse = mouseWorldPoint - shotPoint.position;

        if (catapultToMouse.sqrMagnitude > maxStretchSqr)
            rayToMouse.direction = catapultToMouse.normalized * maxStretch;
        else
            rayToMouse.direction = catapultToMouse;

        rayToMouse.origin = shotPoint.position;
        Vector3 leftCatapultToProjectile = rayToMouse.direction;
        leftCatapultToProjectile.Normalize();
        Vector3 newPos = shotPoint.position + leftCatapultToProjectile * Mathf.Clamp(catapultToMouse.magnitude, 0, maxStretch);
        lineRenderer.SetPosition(1, newPos);

        float distance = Mathf.Clamp(catapultToMouse.magnitude, 0, maxStretch);
        draggedProjectile.position = newPos;
    }

    void OnMouseDown()
    {
        springJoint.spring = 0f;
        clickedOn = true;
        draggedProjectile = Instantiate(projectile, shotPoint.position, Quaternion.identity);
        draggedProjectile.isKinematic = true;
    }

    void OnMouseUp()
    {
        springJoint.spring = 1000f; // Ustaw wartoœæ sprê¿ystoœci na odpowiedni¹ wartoœæ dla Twojej gry
        clickedOn = false;
        draggedProjectile.isKinematic = false;
        Release();
    }

    void Release()
    {
        lineRenderer.enabled = false;
        if (springJoint.connectedBody != null)
        {
            springJoint.connectedBody = null;
        }
    }
}
