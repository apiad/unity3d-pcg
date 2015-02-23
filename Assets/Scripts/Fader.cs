using UnityEngine;
using System.Collections;

public class Fader : MonoBehaviour {

    public enum Status {
        CheckingFadeIn,
        CheckingFadeOut,
        FadingIn,
        FadingOut,
        FadingOutForDestruction,
    }

    private Shader transparentShader;
    private Shader originalShader;
    private Generator parent;

    private bool destroyed;

    public float FadeTime = 0.5f;
    public int FadeSteps = 10;
    public Status AnimationStatus;

    public float MinVisibleDistance;
    public float CameraDistance;

    private bool InView() {
        if (this.parent == null) {
            return false;
        }

        return this.parent.ScreenSize > MinVisibleDistance;
    }

    // Use this for initialization
    void Start() {
        parent = this.transform.parent.GetComponent<Generator>();
        transparentShader = Shader.Find("Transparent/Diffuse");
        originalShader = this.renderer.material.shader;

        if (!InView()) {
            Hide();
            StartCoroutine(CheckFadeIn());
        }
        else {
            Hide();
            FadeIn();
        }
    }

    private IEnumerator CheckFadeOut() {
        AnimationStatus = Status.CheckingFadeOut;

        while (true) {
            if (destroyed) {
                yield break;
            }
            if (!InView()) {
                FadeOut();
                yield break;
            }

            yield return new WaitForSeconds(0.25f); 
        }
    }

    private void Hide() {
        this.renderer.material.shader = transparentShader;
        SetAlpha(0f);
    }

    private void Show() {
        this.renderer.material.shader = originalShader;
        SetAlpha(1f);
    }

    private IEnumerator CheckFadeIn() {
        AnimationStatus = Status.CheckingFadeIn;

        while (true) {
            if (destroyed) {
                yield break;
            }
            if (InView()) {
                FadeIn();
                yield break;
            }

            yield return new WaitForSeconds(0.25f); 
        }
    }

    private void SetAlpha(float alpha) {
        var color = renderer.material.color;
        color.a = alpha;
        renderer.material.color = color;
    }

    public void FadeOutAndDestroy() {
        AnimationStatus = Status.FadingOutForDestruction;

        this.renderer.material.shader = transparentShader;
        StartCoroutine(FadeOutInternal(true));
    }

    public void FadeOut() {
        AnimationStatus = Status.FadingOut;

        this.renderer.material.shader = transparentShader;
        StartCoroutine(FadeOutInternal(false));
    }

    public void FadeIn() {
        AnimationStatus = Status.FadingIn;

        this.renderer.material.shader = transparentShader;
        StartCoroutine(FadeInInternal());
    }

    private IEnumerator FadeInInternal() {
        float alpha = renderer.material.color.a;

        for (float i = 0; i <= FadeSteps; i++) {
            SetAlpha(alpha + (i / FadeSteps) * (1 - alpha));
            yield return new WaitForSeconds(FadeTime / FadeSteps);
        }

        Show();
        StartCoroutine(CheckFadeOut());
    }

    private IEnumerator FadeOutInternal(bool destroy) {
        float alpha = renderer.material.color.a;

        for (float i = 1; i <= FadeSteps; i++) {
            SetAlpha(alpha * (1 - i / FadeSteps));
            yield return new WaitForSeconds(FadeTime / FadeSteps);
        }

        Hide();

        if (destroy) {
            Destroy(this.gameObject);
            destroyed = true;
        }
        else {
            StartCoroutine(CheckFadeIn());
        }
    }
}
