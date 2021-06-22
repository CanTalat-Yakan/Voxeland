using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : Mirror.NetworkBehaviour
{
    [SerializeField] Animator anim;
    public bool run = false;

    void Start()
    {
        run = false;
    }

    Vector3 tmpPos;
    void Update()
    {
        if (!isLocalPlayer) return;
        if (GameManager.Instance.LOCKED) return;

        run = Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0;
        if (transform.position == tmpPos)
            run = false;
        tmpPos = transform.position;

        anim.SetBool("Run", run);
    }
}
