using System.Collections;
using UnityEngine;

namespace Arkeum.Production.Presentation.World
{
    public sealed class ActorView : MonoBehaviour
    {
        private const float ActorZ = -0.1f;
        private const float PlayerMoveDuration = 0.12f;
        private const float EnemyMoveDuration = 0.16f;
        private const float EnemyJumpHeight = 0.5f;

        private SpriteRenderer spriteRenderer;
        private Sprite fallbackSprite;
        private Coroutine moveRoutine;
        private Coroutine idleRoutine;
        private Sprite[] idleFrames;
        private float idleFrameRate;
        private bool hasIdleAnimation;
        private Vector2Int gridPosition;
        private bool hasGridPosition;
        private bool facingLeft;

        public Vector2Int GridPosition => gridPosition;

        public void Initialize(SpriteRenderer renderer, Sprite fallback)
        {
            spriteRenderer = renderer;
            fallbackSprite = fallback;
        }

        public void SetVisual(Sprite sprite, Color tint, int sortingOrder)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (!hasIdleAnimation)
            {
                spriteRenderer.sprite = sprite != null ? sprite : fallbackSprite;
            }

            spriteRenderer.color = tint;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.flipX = facingLeft;
        }

        public void SetIdleAnimation(Sprite[] frames, float frameRate)
        {
            float normalizedFrameRate = Mathf.Max(1f, frameRate);
            if (ReferenceEquals(idleFrames, frames) &&
                Mathf.Approximately(idleFrameRate, normalizedFrameRate))
            {
                return;
            }

            StopIdleRoutine();
            idleFrames = frames;
            idleFrameRate = normalizedFrameRate;

            Sprite firstFrame = GetFirstValidIdleFrame();
            hasIdleAnimation = firstFrame != null;
            if (!hasIdleAnimation)
            {
                return;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = firstFrame;
            idleRoutine = StartCoroutine(AnimateIdle());
        }

        public void SetFacing(Vector2Int direction)
        {
            ApplyFacing(direction);
        }

        public void SetPositionImmediate(Vector2Int cell)
        {
            StopMoveRoutine();
            gridPosition = cell;
            hasGridPosition = true;
            transform.position = ToWorldPosition(cell);
        }

        public void MoveTo(Vector2Int cell, bool isPlayer)
        {
            if (!hasGridPosition)
            {
                SetPositionImmediate(cell);
                return;
            }

            Vector2Int direction = cell - gridPosition;
            ApplyFacing(direction);

            if (direction == Vector2Int.zero)
            {
                return;
            }

            StopMoveRoutine();
            Vector3 from = transform.position;
            Vector3 to = ToWorldPosition(cell);
            gridPosition = cell;
            moveRoutine = StartCoroutine(AnimateMove(from, to, isPlayer));
        }

        public void PlayMoveCollision(Vector2Int targetCell)
        {
            if (!hasGridPosition || targetCell == gridPosition)
            {
                return;
            }

            Vector2Int direction = targetCell - gridPosition;
            ApplyFacing(direction);
            StopMoveRoutine();

            Vector3 origin = ToWorldPosition(gridPosition);
            Vector3 target = ToWorldPosition(targetCell);
            transform.position = origin;
            moveRoutine = StartCoroutine(AnimateMoveCollision(origin, target));
        }

        private void ApplyFacing(Vector2Int direction)
        {
            if (direction.x < 0)
            {
                facingLeft = true;
            }
            else if (direction.x > 0)
            {
                facingLeft = false;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = facingLeft;
            }
        }

        private IEnumerator AnimateMove(Vector3 from, Vector3 to, bool isPlayer)
        {
            float duration = isPlayer ? PlayerMoveDuration : EnemyMoveDuration;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                Vector3 position = Vector3.Lerp(from, to, eased);
                if (!isPlayer)
                {
                    position.y += Mathf.Sin(t * Mathf.PI) * EnemyJumpHeight;
                }

                transform.position = position;
                yield return null;
            }

            transform.position = to;
            moveRoutine = null;
        }

        private IEnumerator AnimateMoveCollision(Vector3 origin, Vector3 target)
        {
            Vector3 midpoint = Vector3.Lerp(origin, target, 0.5f);
            float elapsed = 0f;
            while (elapsed < EnemyMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / EnemyMoveDuration);
                float horizontalT = 1f - Mathf.Abs((t * 2f) - 1f);
                float easedHorizontalT = Mathf.SmoothStep(0f, 1f, horizontalT);

                Vector3 position = Vector3.Lerp(origin, midpoint, easedHorizontalT);
                position.y += Mathf.Sin(t * Mathf.PI) * EnemyJumpHeight;
                transform.position = position;
                yield return null;
            }

            transform.position = origin;
            moveRoutine = null;
        }

        private IEnumerator AnimateIdle()
        {
            int frameIndex = 0;
            WaitForSeconds frameDelay = new WaitForSeconds(1f / idleFrameRate);
            while (true)
            {
                Sprite frame = idleFrames[frameIndex];
                if (frame != null)
                {
                    spriteRenderer.sprite = frame;
                }

                frameIndex = (frameIndex + 1) % idleFrames.Length;
                yield return frameDelay;
            }
        }

        private void StopMoveRoutine()
        {
            if (moveRoutine == null)
            {
                return;
            }

            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        private void StopIdleRoutine()
        {
            if (idleRoutine != null)
            {
                StopCoroutine(idleRoutine);
                idleRoutine = null;
            }

            hasIdleAnimation = false;
        }

        private Sprite GetFirstValidIdleFrame()
        {
            if (idleFrames == null)
            {
                return null;
            }

            for (int i = 0; i < idleFrames.Length; i++)
            {
                if (idleFrames[i] != null)
                {
                    return idleFrames[i];
                }
            }

            return null;
        }

        private static Vector3 ToWorldPosition(Vector2Int cell)
        {
            return new Vector3(cell.x, cell.y, ActorZ);
        }
    }
}
