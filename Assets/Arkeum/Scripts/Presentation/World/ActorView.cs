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

            spriteRenderer.sprite = sprite != null ? sprite : fallbackSprite;
            spriteRenderer.color = tint;
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.flipX = facingLeft;
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

        private void StopMoveRoutine()
        {
            if (moveRoutine == null)
            {
                return;
            }

            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        private static Vector3 ToWorldPosition(Vector2Int cell)
        {
            return new Vector3(cell.x, cell.y, ActorZ);
        }
    }
}
