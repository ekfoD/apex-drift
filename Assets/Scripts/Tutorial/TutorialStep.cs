using UnityEngine;

[System.Serializable]
public abstract class TutorialStep : MonoBehaviour
{
    [TextArea(2, 4)]
    public string instructionText = "Complete this step";

    [SerializeField] protected GameObject highlightObject; // Optional visual guide

    protected bool completed = false;

    public virtual void Activate()
    {
        completed = false;
        if (highlightObject != null)
            highlightObject.SetActive(true);
    }

    public virtual void Deactivate()
    {
        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    public abstract bool IsCompleted();
}
