using Arkeum.Production.Gameplay.Run;
using Arkeum.Production.Presentation.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class ShopOfferPopupPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Image itemDescImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Color affordablePriceColor = Color.black;
        [SerializeField] private Color unaffordablePriceColor = new Color(0.72f, 0.12f, 0.12f);

        private bool initialized;

        public void Initialize()
        {
            ResolveReferences();
            initialized = true;
            Hide();
        }

        public void Refresh(
            RunController runController,
            int currentGold,
            bool isInRun,
            WorldPresenter worldPresenter)
        {
            if (!initialized)
            {
                Initialize();
            }

            if (!isInRun ||
                runController == null ||
                worldPresenter == null ||
                !runController.TryGetAdjacentShopOffer(out ShopOfferDefinition offer) ||
                !worldPresenter.TryGetShopPopupAnchor(offer.Position, out Vector3 anchorWorldPosition) ||
                !TrySetPositionAtWorldAnchor(anchorWorldPosition, worldPresenter.WorldCamera))
            {
                Hide();
                return;
            }

            Show(offer, currentGold);
        }

        private bool TrySetPositionAtWorldAnchor(Vector3 worldPosition, Camera worldCamera)
        {
            RectTransform popupRect = transform as RectTransform;
            RectTransform parentRect = popupRect != null ? popupRect.parent as RectTransform : null;
            if (popupRect == null || parentRect == null || worldCamera == null)
            {
                return false;
            }

            Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
            if (screenPosition.z <= 0f)
            {
                return false;
            }

            Canvas canvas = popupRect.GetComponentInParent<Canvas>();
            Camera canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenPosition,
                    canvasCamera,
                    out Vector2 localPosition))
            {
                return false;
            }

            popupRect.anchoredPosition = localPosition;
            return true;
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void Show(ShopOfferDefinition offer, int currentGold)
        {
            if (offer == null)
            {
                Hide();
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (itemNameText != null)
            {
                itemNameText.text = offer.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = offer.Summary;
            }

            int price = Mathf.Max(0, offer.Price);
            if (priceText != null)
            {
                priceText.text = $"{price} Gold";
                priceText.color = currentGold >= price ? affordablePriceColor : unaffordablePriceColor;
            }

            if (itemDescImage != null)
            {
                Sprite descriptionSprite = offer.Weapon != null
                    ? offer.Weapon.ShopDescriptionSprite
                    : null;
                if (descriptionSprite == null && offer.Weapon != null)
                {
                    descriptionSprite = offer.Weapon.Sprite;
                }

                itemDescImage.sprite = descriptionSprite;
                itemDescImage.enabled = descriptionSprite != null;
            }
        }

        private void ResolveReferences()
        {
            Transform[] descendants = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < descendants.Length; i++)
            {
                Transform descendant = descendants[i];
                switch (descendant.name)
                {
                    case "ItemNameText":
                        itemNameText = descendant.GetComponent<TextMeshProUGUI>();
                        break;
                    case "ItemDescImage":
                        itemDescImage = descendant.GetComponent<Image>();
                        break;
                    case "DescriptionText":
                        descriptionText = descendant.GetComponent<TextMeshProUGUI>();
                        break;
                    case "PriceText":
                        priceText = descendant.GetComponent<TextMeshProUGUI>();
                        break;
                }
            }
        }
    }
}
