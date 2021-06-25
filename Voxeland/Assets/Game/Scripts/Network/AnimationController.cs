using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class AnimationController : Mirror.NetworkBehaviour
{
    [SerializeField] NetworkAnimator anim;
    public bool run = false;

    void Start()
    {
        run = false;
    }

    Vector3 tmpPos;
    void Update()
    {
        // if (!isLocalPlayer) return;
        if (GameManager.Instance.LOCKED) return;

        run = Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0;
        if (transform.position == tmpPos)
            run = false;
        tmpPos = transform.position;

        anim.animator.SetBool("Run", run);
        // RpcOnAnim();
        // CmdAnim();
    }

    // this is called on the server
    [Command]
    void CmdAnim()
    {
        RpcOnAnim();
    }

    // this is called for all observers
    [ClientRpc]
    void RpcOnAnim()
    {
        anim.animator.SetBool("Run", run);
    }
}
