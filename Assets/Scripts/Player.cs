using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public string playerName;
    public int maxHP = 20;
    public int currentHP;
    public int stamina = 5;        // Максимум Выдержки за ход
    public int currentStamina;
    public int shield = 0;         // Активный щит
    public List<Card> hand = new List<Card>();
    public List<int> charges = new List<int>(); // Значения урона зарядов
    public List<Card> deck;

    // Максимальный размер руки (можно настроить)
    public int maxHandSize = 8;

    // Конструктор с 2 аргументами (для обратной совместимости)
    public Player(string name, List<Card> startingDeck)
    {
        playerName = name;
        currentHP = maxHP;
        stamina = 5;
        currentStamina = stamina;
        deck = new List<Card>(startingDeck);
        ShuffleDeck();
        DrawCards(4);
    }

    // НОВЫЙ КОНСТРУКТОР с 3 аргументами
    public Player(string name, List<Card> startingDeck, int startingStamina)
    {
        playerName = name;
        currentHP = maxHP;
        stamina = startingStamina;
        currentStamina = stamina;
        deck = new List<Card>(startingDeck);
        ShuffleDeck();
        DrawCards(4);
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count > 0 && hand.Count < maxHandSize)
            {
                hand.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Добирает случайные карты из колоды
    /// </summary>
    /// <param name="count">Количество карт для добора</param>
    public void DrawRandomCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count > 0 && hand.Count < maxHandSize)
            {
                // Берём случайную карту из колоды
                int randomIndex = Random.Range(0, deck.Count);
                Card randomCard = deck[randomIndex];
                hand.Add(randomCard);
                deck.RemoveAt(randomIndex);
                Debug.Log($"{playerName} получил карту: {randomCard.cardName}");
            }
            else if (deck.Count == 0)
            {
                Debug.Log($"{playerName}: колода пуста!");
                break;
            }
            else if (hand.Count >= maxHandSize)
            {
                Debug.Log($"{playerName}: рука полна ({maxHandSize} карт)!");
                break;
            }
        }
    }

    /// <summary>
    /// Проверяет, можно ли добрать карту
    /// </summary>
    public bool CanDrawCard()
    {
        return deck.Count > 0 && hand.Count < maxHandSize;
    }

    /// <summary>
    /// Проверяет, есть ли карты в колоде
    /// </summary>
    public bool HasCardsInDeck()
    {
        return deck.Count > 0;
    }

    public void TakeDamage(int damage)
    {
        // Сначала пробиваем щит
        if (shield > 0)
        {
            int blocked = Mathf.Min(shield, damage);
            shield -= blocked;
            damage -= blocked;
            Debug.Log($"{playerName}: Щит поглотил {blocked} урона");
        }

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        Debug.Log($"{playerName} получает {damage} урона! HP: {currentHP}");
    }

    public void ApplyCharges()
    {
        if (charges.Count == 0) return;
        
        int totalDamage = 0;
        foreach (int charge in charges)
        {
            totalDamage += charge;
        }
        charges.Clear();
        TakeDamage(totalDamage);
    }

    public void ResetStamina()
    {
        currentStamina = stamina;
    }

    public void ResetShield()
    {
        shield = 0;
    }

    public bool IsAlive()
    {
        return currentHP > 0;
    }
}