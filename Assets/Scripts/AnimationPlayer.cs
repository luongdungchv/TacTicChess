using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnimationPlayer : MonoBehaviour
{
    public static AnimationPlayer ins;
    public GameObject yourTurnText;
    public TextMeshPro damagePopupText;

    private void Start()
    {
        ins = this;
    }
    IEnumerator YourTurnEnum(float duration)
    {
        yourTurnText.SetActive(true);
        yield return new WaitForSeconds(duration);
        yourTurnText.SetActive(false);
    }
   
    IEnumerator DamagerPopupLerp(Vector2 targetPos, float duration, float dmg, string type)
    {
        float t = 0;
        //damagePopupText.transform.position = position;
        damagePopupText.gameObject.SetActive(true);
        damagePopupText.text = $"{type}{dmg}";
        while (t < duration)
        {
            t += Time.deltaTime;
            damagePopupText.transform.position = Vector3.Lerp(targetPos, targetPos + new Vector2(0, 0.5f), t);
            yield return null;
        }
        if (t >= duration) damagePopupText.gameObject.SetActive(false);
    }
    public static void PlayYourTurnAnim(float duration)
    {
        ins.StartCoroutine(ins.YourTurnEnum(duration));
    }
    public static void DamagePopup(Vector2 position, float duration, float dmg, string type)
    {
        ins.StartCoroutine(ins.DamagerPopupLerp(position, duration, dmg, type));
    }
    public static void LerpPosition(Transform from, Vector2 to, float time)
    {
        ins.StartCoroutine(ins.LerpPosEnum(from, to, time));
    }
    public IEnumerator LerpPosEnum(Transform from, Vector2 to, float time)
    {
        float t = 0;
        Vector2 fromCopy = from.position;
        while (t <= 1)
        {
            t += Time.deltaTime / time;
            from.position = Vector2.Lerp(fromCopy, to, t);
            yield return null;
        }
    }
}
