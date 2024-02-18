using System.Collections;
using UnityEngine;

public class BalisticCalculate : MonoBehaviour
{
    [Tooltip("Coefficient Air Resistance")]
    [SerializeField] private float airResistance = 0.1f;  // Wspó³czynnik oporu powietrza
    [Tooltip("kg")]
    [SerializeField] private float objectMass = 1f;       // Masa obiektu
    [Tooltip("m/s")]
    [SerializeField] private float gravity = 9.8f;        // Przyspieszenie ziemskie
    [Tooltip("m/s")]
    [SerializeField] private float appliedForce = 10f;    // Przy³o¿ona si³a

    [Tooltip("Degree")]
    [SerializeField] private float shootAngle = 45f;      // K¹t strza³u w stopniach

    [Tooltip("Degree for Z-axis rotation")]
    [SerializeField] private float shootAngle_Z = 0f;     // Rotate initial shoot angle in Z-axis

    [Tooltip("Direction of the Wind")]
    [SerializeField] private Vector3 windDirection = new Vector3(1f, 0f, 0f); // Default wind direction in the X direction

    [Tooltip("Wind Force")]
    [SerializeField] private float windForce = 0f; // Adjust the wind force as needed

    [Tooltip("Metr. Powy¿ej tej wartoœci kula nie mo¿e lecieæ. GameOver")]
    [SerializeField] private float MaxYOcclude;

    [SerializeField] private float MaxTime;

    private float previousAirResistance;
    private float previousObjectMass;
    private float previousGravity;
    private float previousAppliedForce;
    private float previousShootAngle;
    private float previousShootAngle_Z;


    private enum CoefficientAirResistance
    {
        Sphere,
        HalfSphere,
        Cone,
        Cube,
        AngledCube,
        LongCylinder,
        ShortCylinder,
        StreamlinedBody,
        SteramlinedHalfBody
    }

    private float[] CoefficientAirResistanceValue = {0.47f, 0.42f, 0.5f, 1.05f, 0.8f, 0.82f, 1.15f, 0.04f, 0.09f};
   

    [SerializeField] private CoefficientAirResistance _CoefficientAirResistance;
    


    private float GetAirResistance(CoefficientAirResistance type)
    {
        int index = (int)type;

        if (index >= 0 && index < CoefficientAirResistanceValue.Length)
        {
            return CoefficientAirResistanceValue[index];
        }
        else
        {
            Debug.LogError("Invalid CoefficientAirResistance type!");
            return 0f; // or handle the error in your own way
        }
    }



    [Tooltip("Metr")]
    [SerializeField] private float Distance;  // Zmieniono na float

    [SerializeField] private float MaximumY;

    private Vector3 initialPosition;    // Pocz¹tkowa pozycja obiektu

    private bool isLaunching = false;





    void Start()
    {
        initialPosition = transform.position;
        ResetFunction();

        airResistance = CoefficientAirResistanceValue[((int)_CoefficientAirResistance)];

        previousAirResistance = airResistance;
        previousObjectMass = objectMass;
        previousGravity = gravity;
        previousAppliedForce = appliedForce;
        previousShootAngle = shootAngle;
        previousShootAngle_Z = shootAngle_Z;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isLaunching)
        {
            isLaunching = true;
            StartCoroutine(LaunchObjectCoroutine());
        }

       

        CheckVariableChanges();
    }

  
    private void CheckVariableChanges()
    {
        if (IsVariableChanged())
        {
            ResetFunction();
            UpdatePreviousValues();
        }
    }

    private bool IsVariableChanged()
    {
        return airResistance != previousAirResistance ||
               objectMass != previousObjectMass ||
               gravity != previousGravity ||
               appliedForce != previousAppliedForce ||
               shootAngle != previousShootAngle ||
               shootAngle_Z != previousShootAngle_Z;
    }


    private void UpdatePreviousValues()
    {
        previousAirResistance = airResistance;
        previousObjectMass = objectMass;
        previousGravity = gravity;
        previousAppliedForce = appliedForce;
        previousShootAngle = shootAngle;
        previousShootAngle_Z = shootAngle_Z;
    }


    private void ResetFunction()
    {
        MaximumY = 0f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //Distance = CalculateIntersectionPointX();

        

        Gizmos.DrawSphere(new Vector3(Distance, 0f, 0f), 0.2f);

        Vector3 currentPosition = initialPosition;
        Vector3 currentVelocity = CalculateInitialVelocity();
        float timeStep = 0.1f;

        for (float t = 0; t <= MaxTime; t += timeStep)
        {
            currentPosition += currentVelocity * timeStep;
            currentVelocity.y -= gravity * timeStep;
            currentVelocity -= currentVelocity.normalized * (airResistance / objectMass) * currentVelocity.magnitude * timeStep;


            // Incorporate wind effect
            currentVelocity += windDirection.normalized * windForce / objectMass;

            if (currentPosition.y > 0)
            {
                Gizmos.DrawSphere(currentPosition, 0.1f);
                Distance = Vector3.Distance(initialPosition, currentPosition);

                UpdateMaximumY(currentPosition.y);
            }
        }
    }

    private void UpdateMaximumY(float currentY)
    {
        if (currentY > MaximumY)
        {
            MaximumY = currentY;
        }
    }



    float CalculateIntersectionPointX()
    {
        Vector3 currentPosition = initialPosition;
        Vector3 currentVelocity = CalculateInitialVelocity();

        for (float t = 0; t <= MaxTime; t += 0.1f)
        {
            currentPosition += currentVelocity * 0.1f;
            currentVelocity.y -= gravity * 0.1f;
            currentVelocity -= currentVelocity.normalized * (airResistance / objectMass) * currentVelocity.magnitude * 0.1f;

            // Incorporate wind effect
            currentVelocity += windDirection.normalized * windForce / objectMass;

            if (currentPosition.y <= 0)
            {
                return currentPosition.x;  // Return only the X value
            }
        }

        // If the trajectory does not intersect the Y = 0 axis in 20s, return the last position
        return currentPosition.x;  // Return only the X value
    }

    IEnumerator LaunchObjectCoroutine()
    {
        float timeStep = 0.1f;
        float currentTime = 0f;

        while (currentTime <= MaxTime)
        {
            if (transform.position.y <= 0 || transform.position.y > MaxYOcclude)
            {
                break;
            }

            Vector3 currentPosition = initialPosition;
            Vector3 currentVelocity = CalculateInitialVelocity();

            for (float t = 0; t <= currentTime; t += timeStep)
            {
                currentPosition += currentVelocity * timeStep;
                currentVelocity.y -= gravity * timeStep;
                currentVelocity -= currentVelocity.normalized * (airResistance / objectMass) * currentVelocity.magnitude * timeStep;

                // Incorporate wind effect
                currentVelocity += windDirection.normalized * windForce / objectMass;
            }

            // Smoothly move the object along the trajectory
            StartCoroutine(SmoothMove(currentPosition));

            yield return new WaitForSeconds(timeStep);
            currentTime += timeStep;
        }

        isLaunching = false;
    }


    IEnumerator SmoothMove(Vector3 targetPosition)
    {
        float duration = 0.1f; // Adjust the duration based on your preference
        float elapsedTime = 0f;

        Vector3 startPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position is exact
        transform.position = targetPosition;
    }



    Vector3 CalculateInitialVelocity()
    {
        float angleRad = Mathf.Deg2Rad * shootAngle;
        float angleRadZ = Mathf.Deg2Rad * shootAngle_Z; // K¹t obrotu w osi Z

        float initialVelocityX = appliedForce * Mathf.Cos(angleRad) / objectMass;
        float initialVelocityY = appliedForce * Mathf.Sin(angleRad) / objectMass;
        float initialVelocityZ = appliedForce * angleRadZ / objectMass; // Sk³adowa obracaj¹ca trajektoriê w osi Z

        Vector3 initialVelocity = new Vector3(initialVelocityX, initialVelocityY, initialVelocityZ);

        // Obrót trajektorii w osi Z za pomoc¹ kwaternionu
        Quaternion rotation = Quaternion.Euler(0, 0, angleRadZ);
        Vector3 rotatedVelocity = rotation * initialVelocity;

        return rotatedVelocity;
    }
}
