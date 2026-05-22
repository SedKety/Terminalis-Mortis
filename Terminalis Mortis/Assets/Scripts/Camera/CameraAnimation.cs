using UnityEngine;

public class CameraAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private bool lookingUp;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            lookingUp = !lookingUp;

            animator.SetBool("LookUp", lookingUp);
        }
    }
}