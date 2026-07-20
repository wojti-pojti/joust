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
        // choose and apply upgrade/modifier
        /* ideas
            0 - nothing
            1 - restore shield
            2 - repair lance
            3 - invert controls for 1 round
            4 - horses charge on their own for 1 round
            5 - very wide jousting field for 1 round
            6 - reinforced lance for 1 round
         */
        // some animation
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
