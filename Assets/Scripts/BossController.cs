using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Настройки босса")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float timeLimit = 30f;
    [SerializeField] private int reward = 100;

    [Header("UI элементы")]
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text timerText;
    [SerializeField] private Button attackButton;

    [Header("Зависимости")]
    [SerializeField] private MessageManager messageManager;
    [SerializeField] private Clicker clicker;

    [Header("Настройки штрафа")]
    [SerializeField] private int penalty = 50;

    private int currentHealth;
    private float timeRemaining;
    private bool isBossFightActive;

    private void Start()
    {
        InitializeBoss();
    }

    private void InitializeBoss()
    {
        if (bossPanel != null) bossPanel.SetActive(false);
        if (attackButton != null) attackButton.onClick.AddListener(OnAttackButtonClick);
    }

    public void StartBossFight()
    {
        if (isBossFightActive) return;

        isBossFightActive = true;
        currentHealth = maxHealth;
        timeRemaining = timeLimit;

        if (bossPanel != null) bossPanel.SetActive(true);
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (messageManager != null) messageManager.ShowNotification("Босс появился! У вас есть 10 секунд!");
        StartCoroutine(BossFightTimer());
    }

    private IEnumerator BossFightTimer()
    {
        while (timeRemaining > 0 && isBossFightActive)
        {
            timeRemaining -= Time.deltaTime;
            if (timerText != null) timerText.text = $"Время: {Mathf.CeilToInt(timeRemaining)} сек";
            if (healthSlider != null) healthSlider.value = currentHealth;
            yield return null;
        }

        if (isBossFightActive) EndBossFight(false);
    }

    private void OnAttackButtonClick()
    {
        if (isBossFightActive) DamageBoss(1); // Изменено: урон 1 клик
    }
    private void DamageBoss(int damage)
    {
        currentHealth -= 1; // Изменено: урон 1 клик
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0) EndBossFight(true);
    }

    private void EndBossFight(bool victory)
    {
        isBossFightActive = false;
        if (bossPanel != null) bossPanel.SetActive(false);

        if (victory)
        {
            Clicker.AddClicks(reward); // Исправлено: вызываем статический метод через имя класса
            if (messageManager != null) messageManager.ShowWinMessage($"Победа! Вы получили {reward} кликов!");
        }
        else
        {
            Clicker.RemoveClicks(penalty); // Исправлено: вызываем статический метод через имя класса
            if (messageManager != null) messageManager.ShowLoseMessage($"Время вышло! Босс сбежал. Вы потеряли {penalty} кликов.");
        }
    }
}