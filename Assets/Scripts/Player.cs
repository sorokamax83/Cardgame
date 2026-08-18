using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public string playerName;
    public int maxHP = 20;
    public int currentHP;
    public int stamina = 3;        // Максимум Выдержки за ход
    public int currentStamina;
    public int shield = 0;         // Активный щит
    public List<Card> hand = new List<Card>();
    public List<int> charges = new List<int>(); // Значения урона зарядов
    public List<Card> deck;

    public Player(string name, List<Card> startingDeck)
    {
        playerName = name;
        currentHP = maxHP;
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
            if (deck.Count > 0 && hand.Count < 8) // Лимит руки
            {
                hand.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }
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