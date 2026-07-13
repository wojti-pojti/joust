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
    [SerializeField] float targetYRotation;
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
        if(GameManager.Instance.isFightActive)
        {
            distanceBetweenPlayers = Mathf.Abs(player1.transform.position.x - player2.transform.position.x);
            midpointX = Mathf.Min(player1.transform.position.x, player2.transform.position.x) + 0.5f * distanceBetweenPlayers;
            currentFOV = Mathf.Max(startFOV - (startDistanceBetweenPlayers / distanceBetweenPlayers), 4);

            Vector3 newPos = new Vector3(midpointX,
                Mathf.Max(startPosition.y - 0.25f * (startDistanceBetweenPlayers / distanceBetweenPlayers), -2f),
                startPosition.z);

            this.transform.SetPositionAndRotation(newPos, startRotation);

            cam.orthographicSize = currentFOV;
        }

        if ((transform.rotation.y % 360) != targetYRotation) 
        {
            float nextY = Mathf.Lerp(transform.rotation.y, targetYRotation, 1.5f);
            //float nextY = transform.rotation.y + rotationSpeed * Time.fixedDeltaTime;
            transform.rotation = new Quaternion(0f, nextY, 0f, 0f);
            
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

    /// <summary>
    /// Plays the animation of the camera turning around to show a reaction graphic.
    /// </summary>
    /// <param name="index">The index of the reaction image.</param>
    /// <param name="duration">The duration of how long the image should be shown.</param>
    public void DisplayViewersReaction(int index, float duration)
    {
        // setup the reaction panel based on index

        StartCoroutine(RotateCameraAround(duration));
    }

    /// <summary>
    /// Actually turns around the camera around y-axis for a chosen amount of time.
    /// </summary>
    /// <param name="duration">Duration for which the camera should stay rotated.</param>
    /// <returns></returns>
    IEnumerator RotateCameraAround(float duration)
    {
        targetYRotation = 180f;
        cam.orthographic = false;
        yield return new WaitForSeconds(1.5f);
        cam.orthographic = true;
        yield return new WaitForSeconds(duration);
        cam.orthographic = false;
        yield return new WaitForSeconds(1.5f);
        targetYRotation = 0f;
        cam.orthographic = true;
    }
}
