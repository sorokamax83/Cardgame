using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI descriptionText;
    public Image cardImage;
    public Image backgroundImage;

    [Header("Card Back")]
    public Sprite cardBackSprite;  // Рубашка карты (перетащить в инспекторе)

    private Card cardData;
    private int cardIndex;
    private GameManager gameManager;
    private bool isEnemyCard = false; // Флаг: вражеская ли карта

    public void Initialize(Card card, int index, GameManager manager, bool isEnemy = false)
    {
        cardData = card;
        cardIndex = index;
        gameManager = manager;
        isEnemyCard = isEnemy;

        if (isEnemyCard)
        {
            // ===== ВРАЖЕСКАЯ КАРТА: показываем рубашку =====
            // Скрываем всю информацию
            if (nameText != null) nameText.text = "";
            if (costText != null) costText.text = "";
            if (valueText != null) valueText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            
            // Показываем рубашку
            if (cardImage != null && cardBackSprite != null)
                cardImage.sprite = cardBackSprite;
            
            // Фон — нейтральный (тёмно-серый)
            if (backgroundImage != null)
                backgroundImage.color = new Color(0.3f, 0.3f, 0.3f);
        }
        else
        {
            // ===== КАРТА ИГРОКА: показываем полную информацию =====
            if (nameText != null) nameText.text = card.cardName;
            if (costText != null) costText.text = card.cost.ToString();
            if (valueText != null) valueText.text = card.value.ToString();
            if (descriptionText != null) descriptionText.text = card.description;
            
            if (cardImage != null && card.cardImage != null)
                cardImage.sprite = card.cardImage;

            // Цвет фона по типу карты
            if (backgroundImage != null)
            {
                switch (card.type)
                {
                    case CardType.Attack:
                        backgroundImage.color = new Color(0.8f, 0.2f, 0.2f); // Красный
                        break;
                    case CardType.Charge:
                        backgroundImage.color = new Color(0.2f, 0.4f, 0.8f); // Синий
                        break;
                    case CardType.Shield:
                        backgroundImage.color = new Color(0.2f, 0.8f, 0.2f); // Зелёный
                        break;
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Вражеские карты нельзя нажимать
        if (isEnemyCard) return;
        gameManager.PlayCard(cardIndex);
    }
}