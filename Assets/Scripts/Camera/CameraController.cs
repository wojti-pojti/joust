using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //public bool dynamic;
    [SerializeField] float distanceBetweenPlayers;
    [SerializeField] float midpointX;
    [SerializeField] float currentFOV;
    [Header("")]
    public GameObject player1;
    public GameObject player2;

    Camera cam;
    [Header("Starting values")]
    [SerializeField] float startFOV;
    [SerializeField] float startDistanceBetweenPlayers;
    [SerializeField] Vector3 startPosition;
    [SerializeField] Quaternion startRotation;

    [Header("After match results")]
    [SerializeField] bool facingBack;

    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float perspectiveFOV = 40f;
    public float perspectiveDistanceBoost = 2f;

    //[SerializeField] float targetYRotation;
    [SerializeField] float rotationSpeed;
    [SerializeField] GameObject reactionPanel;

    #region Singleton
    public static CameraController Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
        startFOV = cam.orthographicSize;
        startDistanceBetweenPlayers = Mathf.Abs(player1.transform.position.x - player2.transform.position.x);
        this.transform.GetPositionAndRotation(out startPosition, out startRotation);
    }

    private void FixedUpdate()
    {
        if(GameManager.Instance.gameState == GameState.MATCH || GameManager.Instance.gameState == GameState.ACTIVE_COMBAT)
        {
            distanceBetweenPlayers = Mathf.Abs(player1.transform.position.x - player2.transform.position.x);
            midpointX = Mathf.Min(player1.transform.position.x, player2.transform.position.x) + 0.5f * distanceBetweenPlayers;
            currentFOV = Mathf.Max(startFOV - (startDistanceBetweenPlayers / distanceBetweenPlayers), 4);

            Vector3 newPos = new Vector3(midpointX,
                Mathf.Max(startPosition.y - 0.25f * (startDistanceBetweenPlayers / distanceBetweenPlayers), -2f),
                startPosition.z);

            this.transform.SetPositionAndRotation(newPos, this.transform.rotation);

            cam.orthographicSize = currentFOV;
        }

        reactionPanel.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, reactionPanel.transform.position.z);
    }

    /// <summary>
    /// Resets the camera to its' initial position.
    /// </summary>
    public void ResetCamera()
    {
        this.transform.SetPositionAndRotation(startPosition, startRotation);
        cam.orthographicSize = startFOV;
    }

    #region Endgame shenanigans
    /// <summary>
    /// Plays the animation of the camera turning around to show a reaction graphic.
    /// </summary>
    /// <param name="rotationDuration">The duration of how long the camera should be rotated.</param>
    /// <param name="stayDuration">The duration of how long the image should be shown.</param>
    public void DisplayViewersReaction(float rotationDuration, float stayDuration)
    {
        StopAllCoroutines();
        facingBack = true;
        StartCoroutine(RotateCameraAround(rotationDuration, stayDuration, 180f));
    }

    /// <summary>
    /// Actually turns around the camera around y-axis for a chosen amount of time.
    /// </summary>
    /// <param name="duration">Duration for which the camera should rotate.</param>
    /// <param name="stayDuration">Duration for which the camera should stay rotated.</param>
    /// <param name="targetRotation">The goal angle of the rotation.</param>
    /// <returns></returns>
    IEnumerator RotateCameraAround(float duration, float stayDuration, float targetRotation)
    {
        cam.orthographic = false;
        cam.fieldOfView = perspectiveFOV;

        float startY = transform.eulerAngles.y;
        float delta = Mathf.DeltaAngle(startY, targetRotation);
        float elapsedTime = 0f;

        while (elapsedTime < duration) 
        {
            elapsedTime += Time.deltaTime;
            float turn = easeCurve.Evaluate(elapsedTime / duration);
            float newRotation = startY + turn * delta;
            transform.rotation = Quaternion.Euler(0f, newRotation, 0f);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);

        if(facingBack)
        {
            cam.orthographic = true;
            cam.orthographicSize = 15f;
            yield return new WaitForSeconds(stayDuration);
            facingBack = false;
            StartCoroutine(RotateCameraAround(duration, 0f, 0f));
        }
        else
        {
            cam.orthographic = true;
            cam.orthographicSize = startFOV;
        }
    }
    #endregion
}
