using UnityEngine;

public class TutorialStep_PressKey : TutorialStep
{
    [SerializeField] private KeyCode keyToPress = KeyCode.Space;

    public override bool IsCompleted()
    {
        return Input.GetKeyDown(keyToPress);
    }
}
