using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
   private Animator animator;
   [SerializeField] private players players;

    private void Awake()
    {
        animator= GetComponent<Animator>();
    }
    private void Update()
    {
                 animator.SetBool("Walking", players.IsWalking());

    }

}
