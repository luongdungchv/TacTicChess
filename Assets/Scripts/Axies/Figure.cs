using AxieMixer.Unity;
using Spine;
using Spine.Unity;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class Figure : MonoBehaviour
{
    private SkeletonAnimation skeletonAnimation;
    public GameObject bullet;
    public static bool isPlayingAnimation = false;

    [SerializeField] private bool _flipX = false;
    public bool flipX
    {
        get
        {
            return _flipX;
        }
        set
        {
            _flipX = value;
            if (skeletonAnimation != null)
            {
                skeletonAnimation.skeleton.ScaleX = (_flipX ? -1 : 1) * Mathf.Abs(skeletonAnimation.skeleton.ScaleX);
            }
        }
    }

    private void Awake()
    {
        skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
    }

    public void SetGenes(string id, string genes)
    {
        if (string.IsNullOrEmpty(genes)) return;

        if (skeletonAnimation != null && skeletonAnimation.state != null)
        {
            skeletonAnimation.state.End -= SpineEndHandler;
        }
        Mixer.SpawnSkeletonAnimation(skeletonAnimation, "10000", genes);

        GetComponent<MeshRenderer>().sortingOrder = 2;
        skeletonAnimation.transform.localPosition = new Vector3(-0.05f, -1.42f, 0f);
        skeletonAnimation.transform.SetParent(transform, false);
        skeletonAnimation.transform.localScale = new Vector3(3, 3, 1);
        skeletonAnimation.skeleton.ScaleX = (_flipX ? -1 : 1) * Mathf.Abs(skeletonAnimation.skeleton.ScaleX);
        skeletonAnimation.timeScale = 0.5f;
        skeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
        skeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
        skeletonAnimation.state.Complete += SpineEndHandler;
    }

    private void OnDisable()
    {
        if (skeletonAnimation != null)
        {
            skeletonAnimation.state.End -= SpineEndHandler;
        }
    }

    public void DoJumpAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "action/move-forward", false);
    }
    public void DoAtkAnim(string atkAnim, Transform target, Action callback, float delay)
    {
        isPlayingAnimation = true;
        if (target.position.x < transform.position.x)
            transform.localScale = new Vector3(3, 3, 1);
        else transform.localScale = new Vector3(-3, 3, 1);
        void fullCallback(TrackEntry entry)
        {
            callback();
            skeletonAnimation.state.Complete -= fullCallback;
        }
        skeletonAnimation.state.SetAnimation(0, atkAnim, false);
        //skeletonAnimation.state.Complete += fullCallback;
        StartCoroutine(StartShootCountdown(delay, callback, target.position));
    }
    public void DoHitOrDieAnim(string getHitAnim)
    {

        skeletonAnimation.state.SetAnimation(0, getHitAnim, false);
    }

    IEnumerator StartShootCountdown(float delay, Action callback, Vector2 target)
    {
        yield return new WaitForSeconds(delay);
        bullet.transform.position = transform.position;
        StartCoroutine(ShootEnum(bullet.transform, target, callback, 0.5f));
    }
    IEnumerator ShootEnum(Transform from, Vector2 to, Action callback, float duration)
    {
        float t = 0;
        Vector2 fromCopy = from.position;
        from.gameObject.SetActive(true);
        while (t <= 1)
        {
            from.position = Vector2.Lerp(fromCopy, to, t);
            t += Time.deltaTime / duration;
            yield return null;
        }
        if (t > 1)
        {
            isPlayingAnimation = false;
            from.gameObject.SetActive(false);
            callback();
        }
    }

    public void DoHitOrDieAnim(string getHitAnim, Action callback)
    {
        Debug.Log("test");
        void fullCallback(TrackEntry entry)
        {
            callback();
            skeletonAnimation.state.End -= fullCallback;
        }
        skeletonAnimation.state.SetAnimation(0, getHitAnim, false);
        skeletonAnimation.state.Complete += fullCallback;
    }

    private void SpineEndHandler(TrackEntry trackEntry)
    {
        string animation = trackEntry.Animation.Name;

        if (animation != "action/idle/normal")
        {
            skeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
            skeletonAnimation.timeScale = 0.5f;
        }
    }
}

