using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool dynamic;
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
        if(dynamic)
        {
            distanceBetweenPlayers = Mathf.Abs(player1.transform.position.x - player2.transform.position.x);
            midpointX = Mathf.Min(player1.transform.position.x, player2.transform.position.x) + 0.5f * distanceBetweenPlayers;

            Vector3 newPos = new Vector3(midpointX,
                Mathf.Max(startPosition.y - 0.25f * (startDistanceBetweenPlayers / distanceBetweenPlayers), -2f),
                startPosition.z);

            this.transform.SetPositionAndRotation(newPos, startRotation);

            currentFOV = Mathf.Max(startFOV - (startDistanceBetweenPlayers / distanceBetweenPlayers), 4);
            cam.orthographicSize = currentFOV;
        }
    }

    /// <summary>
    /// Resets the camera to its' initial position.
    /// </summary>
    public void ResetCamera()
    {
        this.transform.SetPositionAndRotation(startPosition, startRotation);
        cam.orthographicSize = startFOV;
    }
}
