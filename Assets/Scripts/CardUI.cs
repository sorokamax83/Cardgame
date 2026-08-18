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

    private Card cardData;
    private int cardIndex;
    private GameManager gameManager;

    public void Initialize(Card card, int index, GameManager manager)
    {
        cardData = card;
        cardIndex = index;
        gameManager = manager;

        nameText.text = card.cardName;
        costText.text = card.cost.ToString();
        valueText.text = card.value.ToString();
        descriptionText.text = card.description;
        if (card.cardImage != null)
            cardImage.sprite = card.cardImage;

        // Цвет фона по типу
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

    public void OnPointerClick(PointerEventData eventData)
    {
        gameManager.PlayCard(cardIndex);
    }
}