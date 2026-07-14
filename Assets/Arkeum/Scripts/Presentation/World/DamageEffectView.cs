using System.Collections;
using UnityEngine;

namespace Arkeum.Production.Presentation.World
{
    public sealed class DamageEffectView : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Sprite[] frames;
        private Sprite fallbackSprite;
        private float secondsPerFrame;

        public void Initialize(SpriteRenderer renderer, Sprite[] animationFrames, Sprite fallback, float frameRate)
        {
            spriteRenderer = renderer;
            frames = animationFrames;
            fallbackSprite = fallback;
            secondsPerFrame = 1f / Mathf.Max(1f, frameRate);
            StartCoroutine(PlayAnimation());
        }

        private IEnumerator PlayAnimation()
        {
            if (spriteRenderer == null)
            {
                Destroy(gameObject);
                yield break;
            }

            if (frames == null || frames.Length == 0)
            {
                spriteRenderer.sprite = fallbackSprite;
                yield return new WaitForSeconds(secondsPerFrame);
                Destroy(gameObject);
                yield break;
            }

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] != null)
                {
                    spriteRenderer.sprite = frames[i];
                }

                yield return new WaitForSeconds(secondsPerFrame);
            }

            Destroy(gameObject);
        }
    }
}
