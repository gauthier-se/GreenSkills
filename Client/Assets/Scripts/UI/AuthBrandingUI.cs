using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Animates the branding zone on Login/Register screens.
    /// Shared component with logo sway, ring pulses, floating particles,
    /// and sequenced intro matching the GreenSkills brand identity.
    /// </summary>
    public class AuthBrandingUI : MonoBehaviour
    {
        [Header("Logo")]
        [SerializeField] private RectTransform logoIcon;

        [Header("Text")]
        [SerializeField] private CanvasGroup wordmarkGroup;
        [SerializeField] private CanvasGroup taglineGroup;

        [Header("Background Effects")]
        [SerializeField] private CanvasGroup ring1;
        [SerializeField] private CanvasGroup ring2;
        [SerializeField] private RectTransform[] particles;

        [Header("Timing")]
        [SerializeField] private float ringDelay = 0f;
        [SerializeField] private float logoDelay = 0.3f;
        [SerializeField] private float logoDuration = 0.6f;
        [SerializeField] private float wordmarkDelay = 0.9f;
        [SerializeField] private float taglineDelay = 1.3f;
        [SerializeField] private float particleDelay = 1.0f;

        [Header("Sway")]
        [SerializeField] private float swayAngle = 2f;
        [SerializeField] private float swaySpeed = 1.5f;
        [SerializeField] private float bobAmount = 3f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 _logoBasePosition;

        private void OnEnable()
        {
            InitializeElements();
            StartCoroutine(PlayIntro());
        }

        private void InitializeElements()
        {
            if (logoIcon != null)
            {
                logoIcon.localScale = Vector3.zero;
                _logoBasePosition = logoIcon.anchoredPosition;
            }

            if (wordmarkGroup != null) wordmarkGroup.alpha = 0f;
            if (taglineGroup != null) taglineGroup.alpha = 0f;
            if (ring1 != null) ring1.alpha = 0f;
            if (ring2 != null) ring2.alpha = 0f;

            if (particles != null)
            {
                foreach (var p in particles)
                    if (p != null) p.gameObject.SetActive(false);
            }
        }

        private IEnumerator PlayIntro()
        {
            // Rings fade in + pulse
            yield return new WaitForSeconds(ringDelay);
            if (ring1 != null) StartCoroutine(PulseRing(ring1, 0f));
            if (ring2 != null) StartCoroutine(PulseRing(ring2, 0.8f));

            // Logo pop with overshoot
            yield return new WaitForSeconds(logoDelay - ringDelay);
            if (logoIcon != null)
            {
                StartCoroutine(AnimateScale(logoIcon, Vector3.zero, Vector3.one, logoDuration, true));
                StartCoroutine(SwayLogo());
            }

            // Particles start floating
            yield return new WaitForSeconds(particleDelay - logoDelay);
            if (particles != null)
            {
                foreach (var p in particles)
                    if (p != null) StartCoroutine(FloatParticle(p));
            }

            // Wordmark fade in
            yield return new WaitForSeconds(wordmarkDelay - particleDelay);
            if (wordmarkGroup != null)
                StartCoroutine(FadeIn(wordmarkGroup, 0.5f));

            // Tagline fade in
            yield return new WaitForSeconds(taglineDelay - wordmarkDelay);
            if (taglineGroup != null)
                StartCoroutine(FadeIn(taglineGroup, 0.6f));
        }

        private IEnumerator SwayLogo()
        {
            while (true)
            {
                float rotation = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
                float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;

                if (logoIcon != null)
                {
                    logoIcon.localRotation = Quaternion.Euler(0f, 0f, rotation);
                    logoIcon.anchoredPosition = _logoBasePosition + new Vector3(0f, yOffset, 0f);
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
                float alpha = cycle < 0.2f
                    ? cycle / 0.2f * 0.7f
                    : Mathf.Lerp(0.7f, 0f, (cycle - 0.2f) / 0.8f);

                particle.anchoredPosition = startPos + new Vector3(0, y, 0);
                if (img != null)
                    img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

                yield return null;
            }
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

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
