using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;          // <-- Добавил для TextMeshPro
using UnityEngine.UI;  // <-- Для Image и других UI-компонентов

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text playerHPText;      // Изменил Text на TMP_Text
    public TMP_Text enemyHPText;       // Изменил Text на TMP_Text
    public TMP_Text staminaText;       // Изменил Text на TMP_Text
    public TMP_Text infoText;          // Изменил Text на TMP_Text
    public TMP_Text turnText;          // Изменил Text на TMP_Text
    public Transform handPanel;
    public GameObject cardPrefab;
    public GameObject chargePanel;

    [Header("Card Data")]
    public List<Card> allCards;

    private Player player;
    private Player enemy;
    private bool isPlayerTurn = true;
    private bool gameOver = false;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        List<Card> playerDeck = new List<Card>(allCards);
        List<Card> enemyDeck = new List<Card>(allCards);
        
        player = new Player("Hero", playerDeck);
        enemy = new Player("Mage", enemyDeck);

        UpdateUI();
        StartCoroutine(EnemyTurnCoroutine());
    }

    void UpdateUI()
    {
        playerHPText.text = "HP: " + player.currentHP;
        enemyHPText.text = "HP: " + enemy.currentHP;
        staminaText.text = "Stamina: " + player.currentStamina + "/" + player.stamina;
        turnText.text = isPlayerTurn ? "Your Turn" : "Enemy Turn";

        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < player.hand.Count; i++)
        {
            Card card = player.hand[i];
            GameObject cardObj = Instantiate(cardPrefab, handPanel);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            cardUI.Initialize(card, i, this);
        }

        UpdateChargesUI();
    }

    void UpdateChargesUI()
    {
        // Можно добавить визуализацию зарядов
    }

    void ShowInfo(string message)
    {
        infoText.text = message;
        StartCoroutine(ClearInfoAfterDelay(2f));
    }

    IEnumerator ClearInfoAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        infoText.text = "";
    }

    public void PlayCard(int cardIndex)
    {
        if (!isPlayerTurn || gameOver) return;
        if (cardIndex < 0 || cardIndex >= player.hand.Count) return;

        Card card = player.hand[cardIndex];
        
        if (card.cost > player.currentStamina)
        {
            ShowInfo("Not enough Stamina!");
            return;
        }

        player.hand.RemoveAt(cardIndex);
        player.currentStamina -= card.cost;

        switch (card.type)
        {
            case CardType.Attack:
                enemy.TakeDamage(card.value);
                ShowInfo("Dealt " + card.value + " damage!");
                break;

            case CardType.Charge:
                player.charges.Add(card.value);
                ShowInfo("Charge for " + card.value + " damage set!");
                break;

            case CardType.Shield:
                player.shield = card.value;
                ShowInfo("Shield for " + card.value + " set!");
                break;
        }

        UpdateUI();

        if (!enemy.IsAlive())
        {
            GameOver(true);
            return;
        }

        if (player.currentStamina == 0 || player.hand.Count == 0)
        {
            EndPlayerTurn();
        }
    }

    void EndPlayerTurn()
    {
        isPlayerTurn = false;
        UpdateUI();
        StartCoroutine(EnemyTurnCoroutine());
    }

    IEnumerator EnemyTurnCoroutine()
    {
        yield return new WaitForSeconds(1f);
        ShowInfo("Enemy turn...");

        if (player.charges.Count > 0)
        {
            int totalCharge = 0;
            foreach (int charge in player.charges)
            {
                totalCharge += charge;
            }
            player.charges.Clear();
            enemy.TakeDamage(totalCharge);
            ShowInfo("Charges hit enemy for " + totalCharge + " damage!");
            yield return new WaitForSeconds(1f);

            if (!enemy.IsAlive())
            {
                GameOver(true);
                yield break;
            }
        }

        enemy.ResetShield();
        enemy.ResetStamina();

        yield return StartCoroutine(EnemyAI());

        if (!player.IsAlive())
        {
            GameOver(false);
            yield break;
        }

        player.ResetShield();

        isPlayerTurn = true;
        player.ResetStamina();
        player.DrawCards(1);
        enemy.DrawCards(1);
        UpdateUI();
        ShowInfo("Your turn!");
    }

    IEnumerator EnemyAI()
    {
        int stamina = enemy.currentStamina;

        if (enemy.currentHP < 5 && stamina >= 1)
        {
            Card shieldCard = enemy.hand.Find(c => c.type == CardType.Shield);
            if (shieldCard != null && shieldCard.cost <= stamina)
            {
                enemy.hand.Remove(shieldCard);
                enemy.shield = shieldCard.value;
                stamina -= shieldCard.cost;
                ShowInfo("Enemy sets shield for " + shieldCard.value + "!");
                yield return new WaitForSeconds(1f);
            }
        }

        while (stamina > 0 && enemy.hand.Count > 0)
        {
            List<Card> affordable = enemy.hand.FindAll(c => c.cost <= stamina);
            if (affordable.Count == 0) break;

            Card card = affordable[Random.Range(0, affordable.Count)];
            enemy.hand.Remove(card);
            stamina -= card.cost;

            switch (card.type)
            {
                case CardType.Attack:
                    player.TakeDamage(card.value);
                    ShowInfo("Enemy attacks for " + card.value + " damage!");
                    break;

                case CardType.Charge:
                    enemy.charges.Add(card.value);
                    ShowInfo("Enemy sets charge for " + card.value + "!");
                    break;

                case CardType.Shield:
                    enemy.shield = card.value;
                    ShowInfo("Enemy sets shield for " + card.value + "!");
                    break;
            }

            yield return new WaitForSeconds(0.8f);

            if (!player.IsAlive())
            {
                yield break;
            }
        }
    }

    void GameOver(bool playerWon)
    {
        gameOver = true;
        if (playerWon)
        {
            ShowInfo("YOU WIN!");
        }
        else
        {
            ShowInfo("YOU LOSE...");
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}