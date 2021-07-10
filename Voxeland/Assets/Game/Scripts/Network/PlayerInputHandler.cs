using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerInputHandler : Mirror.NetworkBehaviour
{
    [SerializeField] ECM.Controllers.BaseFirstPersonController controller;
    [SerializeField] Rigidbody rb;
    [SerializeField] NetworkAnimator anim;
    public bool run = false;
    public bool walk = false;
    AudioSource runSound;

    void Start() { run = false; runSound = gameObject.AddComponent<AudioSource>(); runSound.spatialBlend = 0; }

    Vector3 tmpPos;
    bool tmpGrounded;
    bool tmpFalling;
    float tmpVelocity;
    void Update()
    {
        // movement for local player
        if (!isLocalPlayer) return;
        if (GameManager.Instance.LOCKED) return;

        run = controller.run;
        walk = !run && Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("Vertical") != 0;
        if (transform.position == tmpPos) run = false;
        tmpPos = transform.position;

        anim.animator.SetBool("Run", run);
        anim.animator.SetBool("Walk", walk);
        anim.animator.SetBool("Crouch", controller.isCrouching);

        if ((tmpGrounded != controller.isGrounded) && tmpVelocity < -10)
            AudioManager.Instance.Play(AudioManager.Instance.m_AudioInfo.Fall[tmpVelocity < -20 ? 1 : 0]).outputAudioMixerGroup = AudioManager.Instance.m_AudioMixer;

        if ((!controller.isJumping && (run || walk) && !runSound.isPlaying)
             || tmpGrounded != controller.isGrounded)
            AudioManager.Play(runSound, AudioManager.PlayRandomFromList(ref AudioManager.Instance.m_AudioInfo.FootSteps[0].clips), false, 0.25f, controller.isCrouching ? 0.5f : controller.run ? 1.3f : 1).outputAudioMixerGroup = AudioManager.Instance.m_AudioMixer;

        tmpGrounded = controller.isGrounded;
        tmpFalling = controller.isFalling;
        tmpVelocity = rb.velocity.y;

        CmdAnim();
    }

    // this is called on the server
    [Command]
    void CmdAnim() { RpcOnAnim(); }

    // this is called for all observers
    [ClientRpc]
    void RpcOnAnim() { anim.animator.SetBool("Run", run); }
}
