using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIFadeIn : MonoBehaviour {

    public CanvasGroup canvasGroup;

    public float FadeSpeed = 1f;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        if (canvasGroup.alpha > 0)
        {
            float newalpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * FadeSpeed);
            canvasGroup.alpha = newalpha;
        }

	}
}
