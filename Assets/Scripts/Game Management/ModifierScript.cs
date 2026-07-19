using UnityEngine;

/// <summary>
/// This class is meant to keep track of optional modifiers applied at the end of each turn randomly, 
/// giving instantenuous or 1-turn-long effects.
/// </summary>
public class ModifierScript : MonoBehaviour
{

    #region Singleton
    public static ModifierScript Instance;
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


    public void ApplyModifier(int index)
    {
        switch (index)
        {
            default:
                break;
        }
    }

    public void ResetModifiers()
    {

    }
}
