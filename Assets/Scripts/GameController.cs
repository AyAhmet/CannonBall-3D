using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject projectionObject;
    public GameObject finalProjectionObject;
    public GameObject ballObject;

    [Header("UI Objects")]
    public GameObject starObject;
    public GameObject restartButtonObject;

    [Header("Attiributes")]
    public int numberOfTargets;
    public int numberOfProjections;
    public float vel = 0; // Initial velocity applied to ball.

    private Vector3 buttonDownPosition, buttonPosition, buttonUpPosition, ballInstantiatePosition, difference;

    private List<GameObject> projections = new List<GameObject>();
    private List<Vector3> projectionPositions = new List<Vector3>();
    private List<GameObject> ballClones = new List<GameObject>();
    private List<GameObject> stars = new List<GameObject>();

    private GameObject hitObjectFound;
    private Canvas canvas;
    private float some_value = 0f;
    private int score = 0;


    void Start()
    {
        SetUIElements();
        SpawnBall();
    }

    void SetUIElements() {
        canvas = GameObject.FindObjectsOfType<Canvas>()[0];

        AddRestartButton();
        AddStars();
    }

    void AddRestartButton() {
        float width = Screen.width;
        float height = Screen.height;

        float rightPading = width / 25;
        float topPadding = rightPading;

        float buttonWidth = width / 4;
        float buttonHeight = buttonWidth / 3;

        float xPos = (width / 2) - rightPading - (buttonWidth / 2);
        float yPos = (height / 2) - topPadding - (buttonHeight / 2);

        GameObject button = Instantiate(restartButtonObject, canvas.transform);
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        rectTransform.localPosition = new Vector3(xPos, yPos, 0);
        rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight);

        button.GetComponent<Button>().onClick.AddListener(RestartGame);
    }

    void AddStars() {
        float width = Screen.width;
        float height = Screen.height;

        float leftPading = width / 30;
        float topPadding = leftPading;

        float sizeFactor = width / 10;
        float distanceBetweenStars = sizeFactor * 1.2f;

        for (int i = 0; i < numberOfTargets; i++) {

            float xPos = (-width / 2) + leftPading + (sizeFactor / 2) + (distanceBetweenStars * i);
            float yPos = (height / 2) - topPadding - (sizeFactor / 2);

            GameObject star = Instantiate(starObject, canvas.transform);
            RectTransform rectTransform = star.GetComponent<RectTransform>();

            rectTransform.localPosition = new Vector3(xPos, yPos, 0);
            rectTransform.sizeDelta = new Vector2(sizeFactor, sizeFactor);

            stars.Add(star);
        }
    }

    public void RestartGame() {
        Debug.Log("Restarting Game");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TargetHit(bool didScore) {
        if (didScore) {
            Score();
        }
        ActivateSlowMotion(1f, 0.05f);
        Invoke("TurnCameraLeft", 0.5f);
        some_value -= 90;
    }

    void Score() {
        score += 1;

        for (int i = 0; i < score; i++) {
            GameObject star = stars[i];
            Image image = star.GetComponent<Image>();
            image.color = Color.white;
        }
    }

    void ActivateSlowMotion(float duration, float slowDownFactor) {
        Time.timeScale = slowDownFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        Invoke("DeActivateSlowMotion", duration*slowDownFactor);

    }

    void DeActivateSlowMotion() {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void TurnCameraLeft() {
        GameObject ball = GetBall();
        GameObject camera = GameObject.Find("Main Camera");
        camera.GetComponent<CameraController>().TurnAround(ball, -90f);
    }

    void CalculateProjections() {
        GameObject ball = GetBall();
        Vector3 v = ball.transform.forward * vel; // Velocity of the ball
        float time, distance;
        (distance, time) = GetDistanceAndTime();
        if (time == 0) {
            DeletePreviousProjections();
            return;
        }

        projectionPositions.Clear();
        for (int i = 1; i < numberOfProjections + 1; i++) {

            float distanceBetweenProjections = distance / numberOfProjections;

            // To have equal space between projections
            float deltaTime = (time / numberOfProjections) * i;

            float deltaZ = v.z * deltaTime;
            float deltaX = v.x * deltaTime;

            // Velocity in Y direction changes over time due to gravity, so we should calculate.
            float vyAtGivenTime = v.y + (Physics.gravity.y * deltaTime);
            float vyAverage = (vyAtGivenTime + v.y) / 2;
            float deltaY = vyAverage * deltaTime;

            Vector3 deltaPosition = new Vector3(deltaX, deltaY, deltaZ);
            Vector3 projectionPosition = deltaPosition + ball.transform.position; // Adding ball's initial position
            projectionPositions.Add(projectionPosition);
        }
    }

    (float, float) GetDistanceAndTime() {
        GameObject ball = GetBall();

        Vector3 v = ball.transform.forward * vel;
        float distance = 0;

        Vector3 lastPosition = ball.transform.position;

        int maxNumberOfIterations = 100;
        float timeStamp = 0.02f;
        for (int i = 1; i < maxNumberOfIterations; i++) {

            float deltaX = v.x * timeStamp * i;
            float deltaZ = v.z * timeStamp * i;

            // Velocity in Y direction changes over time due to gravity, so we should calculate.
            float vyAtGivenTime = v.y + (Physics.gravity.y * timeStamp * i);
            float vyAverage = (vyAtGivenTime + v.y) / 2;
            float deltaY = vyAverage * timeStamp * i;

            Vector3 deltaPosition = new Vector3(deltaX, deltaY, deltaZ);
            Vector3 nextPosition = deltaPosition + ball.transform.position;

            float distanceTraveled = Vector3.Distance(lastPosition, deltaPosition);

            distance += distanceTraveled;

            RaycastHit hit;
            if (Physics.Linecast(ball.transform.position, nextPosition, out hit)) {
                return (distance - 0.5f, timeStamp * i);
            }
        }
        return (0f, 0f);
    }

    void DeletePreviousProjections() {
        foreach (GameObject projection in projections) {
            Destroy(projection);
        }
    }

    void DrawProjections() {
        for (int i = 0; i < projectionPositions.Count; i++) {
            Vector3 position = projectionPositions[i];
            if (i == projectionPositions.Count - 1) {
                projections.Add(Instantiate(finalProjectionObject, position, Quaternion.identity));
            } else {
                projections.Add(Instantiate(projectionObject, position, Quaternion.identity));
            }

        }
    }

    void CreateProjections() {
        CalculateProjections();
        DeletePreviousProjections();
        DrawProjections();
    }

    void RotateBall() {
        Vector2 positionChange = touchMovedPosition - touchBeganPosition;
        float relativeChangeX = positionChange.x / Screen.width;
        float relativeChangeY = positionChange.y / Screen.height;

        relativeChangeX = Mathf.Clamp(relativeChangeX, -0.33f, 0.33f);
        relativeChangeY = Mathf.Clamp(relativeChangeY, -0.33f, 0.33f);

        GameObject ball = GetBall();
        ball.transform.rotation = Quaternion.Euler(relativeChangeY * 180, some_value + (-relativeChangeX * 90) , 0);
    }

    GameObject GetBall() {
        int indexOfLastAddedBall = ballClones.Count - 1;
        GameObject ball = ballClones[indexOfLastAddedBall];
        return ball;
    }

    void SpawnBall() {
        GameObject ball = Instantiate(ballObject);
        ballClones.Add(ball);
    }

    Vector2 touchBeganPosition, touchMovedPosition, touchEndedPosition;
    void Update() {
        if (Input.touchCount > 0) {

            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchBeganPosition = touch.position;
                    break;

                case TouchPhase.Moved:
                    touchMovedPosition = touch.position;

                    RotateBall();
                    CreateProjections();
                    break;

                case TouchPhase.Ended:

                    touchEndedPosition = touch.position;

                    GameObject ball = GetBall();
                    Rigidbody ballsRB = ball.GetComponent<Rigidbody>();
                    ballsRB.isKinematic = false;
                    ballsRB.AddForce(ball.transform.forward * vel, ForceMode.VelocityChange);
                    Destroy(ball, 3f);
                    Invoke("SpawnBall", 0.5f);
                    DeletePreviousProjections();
                    break;
            }
        }
    }
}
