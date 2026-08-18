using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Duell/Card")]
public class Card : ScriptableObject
{
    public string cardName;
    public int cost;           // Стоимость в Выдержке (1-3)
    public CardType type;
    public int value;          // Урон или прочность щита
    public Sprite cardImage;   // Иконка карты
    [TextArea] public string description;
}

public enum CardType
{
    Attack,   // Мгновенный урон
    Charge,   // Отложенный урон
    Shield    // Защита
}