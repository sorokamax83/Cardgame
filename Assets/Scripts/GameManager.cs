using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;          // Для TextMeshPro
using UnityEngine.UI;  // Для Image и других UI-компонентов

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    // Полоски здоровья (прогресс-бары)
    public Image playerHPFill;     // Зелёная полоска HP игрока
    public Image enemyHPFill;      // Красная полоска HP врага
    
    // Полоска выносливости
    public Image staminaFill;      // Голубая/жёлтая полоска выносливости
    
    // ПОРТРЕТЫ
    public Image playerPortrait;    // Портрет игрока
    public Image enemyPortrait;     // Портрет врага
    
    // Тексты (оставляем только нужные)
    public TMP_Text infoText;
    public TMP_Text turnText;
    
    // Опционально: тексты с цифрами HP поверх полосок
    public TMP_Text playerHPText;  // Цифры HP игрока (20/20)
    public TMP_Text enemyHPText;   // Цифры HP врага (20/20)
    
    public Transform handPanel;        // Панель для карт игрока
    public Transform enemyHandPanel;   // Панель для карт врага
    public GameObject cardPrefab;
    public GameObject chargePanel;

    [Header("Card Data")]
    public List<Card> allCards;

    [Header("Deck Settings")]
    public int deckCopies = 50;        // Количество копий каждой карты в колоде

    [Header("Portraits")]
    public Sprite playerDefaultPortrait;   // Портрет игрока по умолчанию
    public Sprite enemyDefaultPortrait;    // Портрет врага по умолчанию
    public Sprite playerDamagedPortrait;   // Портрет игрока при низком HP (опционально)
    public Sprite enemyDamagedPortrait;    // Портрет врага при низком HP (опционально)

    [Header("Animation Settings")]
    public float portraitShakeIntensity = 15f;  // Интенсивность тряски портрета
    public float portraitShakeDuration = 0.3f;  // Длительность тряски
    public float portraitFlashDuration = 0.15f; // Длительность вспышки

    [Header("Draw Settings")]
    public int cardsToDrawPerTurn = 1;        // Количество карт, добираемых за ход
    public int handSizeLimit = 4;             // НОВОЕ: минимальное количество карт в руке

    [Header("Game Balance")]
    public int maxStamina = 5;                // НОВОЕ: максимальная выносливость за ход

    private Player player;
    private Player enemy;
    private bool isPlayerTurn = true;
    private bool gameOver = false;

    // Для хранения оригинальных цветов портретов
    private Color originalPlayerColor;
    private Color originalEnemyColor;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        // === НОВОЕ: создаём колоды с копиями карт ===
        List<Card> playerDeck = new List<Card>();
        List<Card> enemyDeck = new List<Card>();
        
        // Создаём копии карт для колоды
        for (int i = 0; i < deckCopies; i++)
        {
            foreach (Card card in allCards)
            {
                playerDeck.Add(card);
                enemyDeck.Add(card);
            }
        }
        
        Debug.Log($"Колода игрока: {playerDeck.Count} карт");
        Debug.Log($"Колода врага: {enemyDeck.Count} карт");
        
        // Передаём максимальную выносливость в конструктор
        player = new Player("Hero", playerDeck, maxStamina);
        enemy = new Player("Mage", enemyDeck, maxStamina);

        // Сохраняем оригинальные цвета портретов
        if (playerPortrait != null)
            originalPlayerColor = playerPortrait.color;
        if (enemyPortrait != null)
            originalEnemyColor = enemyPortrait.color;

        // Устанавливаем портреты по умолчанию
        SetPortrait(playerPortrait, playerDefaultPortrait);
        SetPortrait(enemyPortrait, enemyDefaultPortrait);

        UpdateUI();
        StartCoroutine(EnemyTurnCoroutine());
    }

    void UpdateUI()
    {
        // Обновляем полоски здоровья
        UpdateHealthBar(playerHPFill, player.currentHP, player.maxHP, Color.green);
        UpdateHealthBar(enemyHPFill, enemy.currentHP, enemy.maxHP, Color.red);
        
        // Обновляем полоску выносливости
        UpdateStaminaBar(staminaFill, player.currentStamina, player.stamina);
        
        // Обновляем портреты в зависимости от HP
        UpdatePortraits();
        
        // Обновляем тексты с цифрами HP (если они есть)
        if (playerHPText != null)
            playerHPText.text = $"{player.currentHP}/{player.maxHP}";
        if (enemyHPText != null)
            enemyHPText.text = $"{enemy.currentHP}/{enemy.maxHP}";
        
        // Обновляем остальные тексты
        turnText.text = isPlayerTurn ? "Your Turn" : "Enemy Turn";

        // === ОБНОВЛЯЕМ РУКУ ИГРОКА ===
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < player.hand.Count; i++)
        {
            Card card = player.hand[i];
            GameObject cardObj = Instantiate(cardPrefab, handPanel);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            cardUI.Initialize(card, i, this, false); // false = карта игрока
        }

        // === ОБНОВЛЯЕМ РУКУ ВРАГА (рубашкой вверх) ===
        if (enemyHandPanel != null)
        {
            // Очищаем панель врага
            foreach (Transform child in enemyHandPanel)
            {
                Destroy(child.gameObject);
            }

            // Создаём карты врага рубашкой вверх
            for (int i = 0; i < enemy.hand.Count; i++)
            {
                Card card = enemy.hand[i];
                GameObject cardObj = Instantiate(cardPrefab, enemyHandPanel);
                CardUI cardUI = cardObj.GetComponent<CardUI>();
                cardUI.Initialize(card, i, this, true); // true = вражеская карта
            }
        }

        UpdateChargesUI();
    }

    /// <summary>
    /// Устанавливает портрет
    /// </summary>
    void SetPortrait(Image portraitImage, Sprite sprite)
    {
        if (portraitImage != null && sprite != null)
        {
            portraitImage.sprite = sprite;
        }
    }

    /// <summary>
    /// Обновляет портреты в зависимости от HP
    /// </summary>
    void UpdatePortraits()
    {
        if (playerPortrait == null || enemyPortrait == null) return;

        // Проверяем HP игрока
        float playerHPPercent = (float)player.currentHP / player.maxHP;
        if (playerHPPercent < 0.25f && playerDamagedPortrait != null)
        {
            playerPortrait.sprite = playerDamagedPortrait;
        }
        else if (playerDefaultPortrait != null)
        {
            playerPortrait.sprite = playerDefaultPortrait;
        }

        // Проверяем HP врага
        float enemyHPPercent = (float)enemy.currentHP / enemy.maxHP;
        if (enemyHPPercent < 0.25f && enemyDamagedPortrait != null)
        {
            enemyPortrait.sprite = enemyDamagedPortrait;
        }
        else if (enemyDefaultPortrait != null)
        {
            enemyPortrait.sprite = enemyDefaultPortrait;
        }
    }

    /// <summary>
    /// Анимация портрета при получении урона (тряска + вспышка)
    /// </summary>
    IEnumerator AnimatePortrait(Image portrait, Color flashColor, float intensity, float duration, float flashDuration)
    {
        if (portrait == null) yield break;
        
        // Сохраняем исходное положение
        Vector3 originalPosition = portrait.rectTransform.anchoredPosition;
        Color originalColor = portrait.color;
        
        // --- Фаза 1: Вспышка ---
        portrait.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        portrait.color = originalColor;
        
        // --- Фаза 2: Тряска ---
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Случайное смещение
            float offsetX = Random.Range(-intensity, intensity);
            float offsetY = Random.Range(-intensity, intensity);
            portrait.rectTransform.anchoredPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Возвращаем портрет на место
        portrait.rectTransform.anchoredPosition = originalPosition;
    }

    /// <summary>
    /// Упрощённая версия анимации (только тряска)
    /// </summary>
    IEnumerator ShakePortrait(Image portrait, float intensity, float duration)
    {
        if (portrait == null) yield break;
        
        Vector3 originalPosition = portrait.rectTransform.anchoredPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float offsetX = Random.Range(-intensity, intensity);
            float offsetY = Random.Range(-intensity, intensity);
            portrait.rectTransform.anchoredPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        portrait.rectTransform.anchoredPosition = originalPosition;
    }

    /// <summary>
    /// Обновляет полоску здоровья
    /// </summary>
    void UpdateHealthBar(Image fillImage, int currentHP, int maxHP, Color baseColor)
    {
        if (fillImage == null) return;
        
        float fillAmount = (float)currentHP / maxHP;
        fillAmount = Mathf.Clamp01(fillAmount);
        fillImage.fillAmount = fillAmount;
        
        if (fillAmount < 0.25f)
            fillImage.color = Color.red;
        else if (fillAmount < 0.5f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = baseColor;
    }

    /// <summary>
    /// Обновляет полоску выносливости
    /// </summary>
    void UpdateStaminaBar(Image fillImage, int currentStamina, int maxStamina)
    {
        if (fillImage == null) return;
        
        float fillAmount = (float)currentStamina / maxStamina;
        fillAmount = Mathf.Clamp01(fillAmount);
        fillImage.fillAmount = fillAmount;
        
        if (fillAmount < 0.25f)
            fillImage.color = Color.red;
        else if (fillAmount < 0.5f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = new Color(0.2f, 0.6f, 1f); // Голубой
    }

    void UpdateChargesUI()
    {
        Debug.Log($"Player charges: {string.Join(", ", player.charges)}");
        Debug.Log($"Enemy charges: {string.Join(", ", enemy.charges)}");
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

    /// <summary>
    /// Добирает карты до указанного лимита
    /// </summary>
    void DrawToHandLimit(Player player, int limit)
    {
        if (player == null) return;
        
        int cardsToDraw = limit - player.hand.Count;
        if (cardsToDraw <= 0)
        {
            Debug.Log($"{player.playerName}: уже есть {player.hand.Count} карт (лимит {limit})");
            return;
        }
        
        // Ограничиваем количество добираемых карт, если колода маленькая
        int availableCards = player.deck.Count;
        int actualDraw = Mathf.Min(cardsToDraw, availableCards);
        
        if (actualDraw > 0)
        {
            player.DrawRandomCards(actualDraw);
            Debug.Log($"{player.playerName} добрал {actualDraw} карт до {limit} (было {player.hand.Count - actualDraw})");
            
            // Показываем сообщение игроку
            if (player == this.player)
            {
                ShowInfo($"Вы добрали до {limit} карт!");
            }
        }
        else if (player.hand.Count < limit && !player.HasCardsInDeck())
        {
            Debug.Log($"{player.playerName}: колода пуста, невозможно добрать до {limit} карт");
            if (player == this.player)
            {
                ShowInfo("Ваша колода пуста!");
            }
        }
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
                // Анимация портрета врага при получении урона
                StartCoroutine(AnimatePortrait(
                    enemyPortrait, 
                    Color.red, 
                    portraitShakeIntensity, 
                    portraitShakeDuration, 
                    portraitFlashDuration
                ));
                ShowInfo($"Dealt {card.value} damage!");
                break;

            case CardType.Charge:
                player.charges.Add(card.value);
                ShowInfo($"Charge for {card.value} damage set!");
                break;

            case CardType.Shield:
                player.shield = card.value;
                ShowInfo($"Shield for {card.value} set!");
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
            // Анимация портрета врага при получении урона от зарядов
            StartCoroutine(AnimatePortrait(
                enemyPortrait, 
                Color.red, 
                portraitShakeIntensity, 
                portraitShakeDuration, 
                portraitFlashDuration
            ));
            ShowInfo($"Charges hit enemy for {totalCharge} damage!");
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

        // === ПЕРЕХОД К ИГРОКУ ===
        isPlayerTurn = true;
        player.ResetStamina();

        // === НОВОЕ: добор карт ДО ЛИМИТА ===
        DrawToHandLimit(player, handSizeLimit);
        DrawToHandLimit(enemy, handSizeLimit);

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
                ShowInfo($"Enemy sets shield for {shieldCard.value}!");
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
                    // Анимация портрета игрока при получении урона от врага
                    StartCoroutine(AnimatePortrait(
                        playerPortrait, 
                        Color.red, 
                        portraitShakeIntensity, 
                        portraitShakeDuration, 
                        portraitFlashDuration
                    ));
                    ShowInfo($"Enemy attacks for {card.value} damage!");
                    break;

                case CardType.Charge:
                    enemy.charges.Add(card.value);
                    ShowInfo($"Enemy sets charge for {card.value}!");
                    break;

                case CardType.Shield:
                    enemy.shield = card.value;
                    ShowInfo($"Enemy sets shield for {card.value}!");
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
            ShowInfo("🏆 YOU WIN!");
        }
        else
        {
            ShowInfo("💀 YOU LOSE...");
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}