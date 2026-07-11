using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float distanceBetweenPlayers;
    float midpointX;
    [Header("")]
    public GameObject player1;
    public GameObject player2;

    Camera cam;
    float startFOV;
    float startDistanceBetweenPlayers;
    Vector3 startPosition;
    Quaternion startRotation;

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
        startFOV = cam.fieldOfView;
        startDistanceBetweenPlayers = Mathf.Abs(player1.transform.position.x - player2.transform.position.x);
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        distanceBetweenPlayers = Mathf.Abs(player1.transform.position.x - player2.transform.position.x);
        midpointX = Mathf.Min(player1.transform.position.x, player2.transform.position.x) + 0.5f * distanceBetweenPlayers;

        transform.position.Set(midpointX, 
            transform.position.y + (distanceBetweenPlayers / 100f),
            transform.position.z);

        cam.fieldOfView = startFOV - (startDistanceBetweenPlayers / distanceBetweenPlayers);
    }

    /// <summary>
    /// Resets the camera to its' initial position.
    /// </summary>
    public void ResetCamera()
    {
        this.transform.SetPositionAndRotation(startPosition, startRotation);
        cam.fieldOfView = startFOV;
    }
}
