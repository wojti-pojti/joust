using UnityEngine;

/// <summary>
/// This script is meant to control counter-movement of the various (let's say 3) background layers, to create depth.
/// </summary>
public class BackgroundDepthManager : MonoBehaviour
{
    [SerializeField] GameObject camera;
    [Header("Depth & Layers")]
    [SerializeField] GameObject[] layers = new GameObject[3];
    [SerializeField] float[] layerCoefficients = new float[3];
    [Header("Clouds")]
    [SerializeField] float cloudMovementSpeed;
    [SerializeField] GameObject[] cloudGroups = new GameObject[2];

    float lastPosX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastPosX = camera.transform.position.x;
    }

    private void FixedUpdate()
    {
        // depth
        float difference = camera.transform.position.x - lastPosX;
        if (difference != 0) 
        {
            for (int i = 0; i < layers.Length; i++) 
            {
                AdjustLayerPosition(difference, layers[i], layerCoefficients[i]);
            }
        }

        lastPosX = camera.transform.position.x;

        // clouds
        foreach (GameObject cloudGroup in cloudGroups)
        {
            cloudGroup.transform.position = new Vector2(cloudGroup.transform.position.x + cloudMovementSpeed * Time.fixedDeltaTime, 0f);

            if(cloudGroup.transform.localPosition.x > 1.1f)
            {
                cloudGroup.transform.localPosition = new Vector2(-1.05f, 0f);
            }
        }
    }

    /// <summary>
    /// Moves the layer and all children objects in the direction of camera movement by the same distance, multiplied by the layer's coefficient.
    /// </summary>
    /// <param name="difference">The distance travelled by the camera.</param>
    /// <param name="layer">The group of objects constructing the layer.</param>
    /// <param name="coefficient">The number associated with that layer, should be between 0 and 1.</param>
    void AdjustLayerPosition(float difference, GameObject layer, float coefficient)
    {
        if (layer != null)
        {
            layer.transform.position =
                new Vector3(layer.transform.position.x + difference * coefficient, 0, 0);
        }
    }
}
