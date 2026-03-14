using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Animates the boot splash screen elements matching the GreenSkills brand.
    /// Attach to a root UI GameObject in Boot scene with references to child elements.
    /// </summary>
    public class BootSplashUI : MonoBehaviour
    {
        [Header("Logo")]
        [SerializeField] private RectTransform logoIcon;
        [SerializeField] private RectTransform sparkIcon;

        [Header("Text")]
        [SerializeField] private CanvasGroup wordmarkGroup;
        [SerializeField] private CanvasGroup taglineGroup;

        [Header("Loading Bar")]
        [SerializeField] private Image loadingBarFill;
        [SerializeField] private CanvasGroup loadingDotsGroup;
        [SerializeField] private RectTransform[] loadingDots;

        [Header("Background Effects")]
        [SerializeField] private CanvasGroup ring1;
        [SerializeField] private CanvasGroup ring2;
        [SerializeField] private RectTransform[] particles;

        [Header("Timing")]
        [SerializeField] private float logoDelay = 0.3f;
        [SerializeField] private float logoDuration = 0.7f;
        [SerializeField] private float sparkDelay = 0.85f;
        [SerializeField] private float wordmarkDelay = 1.1f;
        [SerializeField] private float taglineDelay = 1.5f;
        [SerializeField] private float barDelay = 1.8f;
        [SerializeField] private float barDuration = 2.2f;

        private void OnEnable()
        {
            InitializeElements();
            StartCoroutine(PlaySequence());
        }

        private void InitializeElements()
        {
            if (logoIcon != null) logoIcon.localScale = Vector3.zero;
            if (sparkIcon != null) sparkIcon.localScale = Vector3.zero;
            if (wordmarkGroup != null) wordmarkGroup.alpha = 0f;
            if (taglineGroup != null) taglineGroup.alpha = 0f;
            if (loadingBarFill != null) loadingBarFill.fillAmount = 0f;
            if (loadingDotsGroup != null) loadingDotsGroup.alpha = 0f;

            if (ring1 != null) ring1.alpha = 0.5f;
            if (ring2 != null) ring2.alpha = 0.3f;

            if (particles != null)
            {
                foreach (var p in particles)
                    if (p != null) p.gameObject.SetActive(false);
            }
        }

        private IEnumerator PlaySequence()
        {
            // Start ring pulse loops
            if (ring1 != null) StartCoroutine(PulseRing(ring1, 0f));
            if (ring2 != null) StartCoroutine(PulseRing(ring2, 0.8f));

            // Logo grow
            yield return new WaitForSeconds(logoDelay);
            if (logoIcon != null)
                StartCoroutine(AnimateScale(logoIcon, Vector3.zero, Vector3.one, logoDuration, true));

            // Spark pop
            yield return new WaitForSeconds(sparkDelay - logoDelay);
            if (sparkIcon != null)
                StartCoroutine(AnimateScale(sparkIcon, Vector3.zero, Vector3.one, 0.5f, true));

            // Start particles
            if (particles != null)
            {
                foreach (var p in particles)
                    if (p != null) StartCoroutine(FloatParticle(p));
            }

            // Wordmark slide up
            yield return new WaitForSeconds(wordmarkDelay - sparkDelay);
            if (wordmarkGroup != null)
                StartCoroutine(FadeIn(wordmarkGroup, 0.5f));

            // Tagline fade
            yield return new WaitForSeconds(taglineDelay - wordmarkDelay);
            if (taglineGroup != null)
                StartCoroutine(FadeIn(taglineGroup, 0.6f));

            // Loading bar
            yield return new WaitForSeconds(barDelay - taglineDelay);
            if (loadingDotsGroup != null) loadingDotsGroup.alpha = 1f;
            if (loadingDots != null)
                StartCoroutine(AnimateLoadingDots());
            if (loadingBarFill != null)
                StartCoroutine(AnimateBar());
        }

        private IEnumerator AnimateScale(RectTransform target, Vector3 from, Vector3 to, float duration, bool overshoot)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float value = overshoot ? EaseOutBack(t) : Mathf.SmoothStep(0f, 1f, t);
                target.localScale = Vector3.LerpUnclamped(from, to, value);
                yield return null;
            }
            target.localScale = to;
        }

        private IEnumerator FadeIn(CanvasGroup group, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            group.alpha = 1f;
        }

        private IEnumerator AnimateBar()
        {
            float elapsed = 0f;
            while (elapsed < barDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / barDuration);
                // Ease out quad
                float value = 1f - (1f - t) * (1f - t);
                loadingBarFill.fillAmount = value;
                yield return null;
            }
            loadingBarFill.fillAmount = 1f;
        }

        private IEnumerator AnimateLoadingDots()
        {
            while (true)
            {
                for (int i = 0; i < loadingDots.Length; i++)
                {
                    if (loadingDots[i] == null) continue;
                    float scale = 0.8f + 0.4f * Mathf.Sin(Time.time * 4.5f - i * 1.4f);
                    loadingDots[i].localScale = Vector3.one * Mathf.Clamp(scale, 0.6f, 1.2f);

                    var img = loadingDots[i].GetComponent<Image>();
                    if (img != null)
                    {
                        float alpha = 0.3f + 0.7f * Mathf.Max(0, Mathf.Sin(Time.time * 4.5f - i * 1.4f));
                        img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
                    }
                }
                yield return null;
            }
        }

        private IEnumerator PulseRing(CanvasGroup ring, float delay)
        {
            yield return new WaitForSeconds(delay);
            var rt = ring.GetComponent<RectTransform>();
            while (true)
            {
                float t = Mathf.Sin(Time.time * 2.1f) * 0.5f + 0.5f;
                float scale = Mathf.Lerp(0.85f, 1.05f, t);
                if (rt != null) rt.localScale = Vector3.one * scale;
                ring.alpha = Mathf.Lerp(0.5f, 0.15f, t);
                yield return null;
            }
        }

        private IEnumerator FloatParticle(RectTransform particle)
        {
            particle.gameObject.SetActive(true);
            Vector3 startPos = particle.anchoredPosition;
            var img = particle.GetComponent<Image>();

            while (true)
            {
                float cycle = Mathf.Repeat(Time.time * 0.4f + startPos.x * 0.01f, 1f);
                float y = Mathf.Lerp(0f, 40f, cycle);
                float alpha = cycle < 0.2f ? cycle / 0.2f * 0.7f : Mathf.Lerp(0.7f, 0f, (cycle - 0.2f) / 0.8f);

                particle.anchoredPosition = startPos + new Vector3(0, y, 0);
                if (img != null)
                    img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

                yield return null;
            }
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
