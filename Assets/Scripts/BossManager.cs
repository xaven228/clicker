using UnityEngine;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    // Панель с боссом
    public GameObject bossPanel;
    // Кнопка босса (для нанесения урона)
    public Button bossButton;
    // Ползунок здоровья босса
    public Slider bossHealthBar;
    // Текст для отображения таймера
    public Text timerText;
    // Здоровье босса
    public int bossHealth = 100;
    // Время на победу над боссом
    public float bossTimeLimit = 10f;
    // Награда за победу
    public int bossReward = 500;
    // Штраф за проигрыш
    public int bossPenalty = 200;

    // Переменные для состояния босса
    private bool isBossActive = false;
    private int currentBossHealth;
    private int currentTimeLeft; // Оставшееся время в целых секундах

    // Ссылка на Clicker для изменения количества кликов
    public Clicker clicker;

    // Метод, вызываемый при старте игры
    void Start()
    {
        // Скрываем панель босса при старте
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }

        // Отключаем взаимодействие со слайдером
        if (bossHealthBar != null)
        {
            bossHealthBar.interactable = false; // Запрещаем изменять слайдер мышкой
        }
    }

    // Метод для начала битвы с боссом
    public void StartBossFight()
    {
        Debug.Log("Босс появился!");

        // Активируем панель босса
        if (bossPanel != null)
        {
            bossPanel.SetActive(true);
        }

        // Инициализируем здоровье и таймер босса
        currentBossHealth = bossHealth;
        currentTimeLeft = Mathf.FloorToInt(bossTimeLimit); // Устанавливаем начальное время в целых секундах

        // Обновляем полосу здоровья
        if (bossHealthBar != null)
        {
            bossHealthBar.maxValue = bossHealth;
            bossHealthBar.value = currentBossHealth;
        }

        // Назначаем обработчик события для кнопки босса
        if (bossButton != null)
        {
            bossButton.onClick.AddListener(AttackBoss);
        }

        // Обновляем текст таймера
        UpdateTimerText();

        // Запускаем таймер с интервалом в 1 секунду
        InvokeRepeating(nameof(UpdateTimer), 1f, 1f);
        isBossActive = true;
    }

    // Метод для атаки босса
    private void AttackBoss()
    {
        if (isBossActive)
        {
            currentBossHealth -= 1; // Уменьшаем здоровье босса

            // Обновляем полосу здоровья
            if (bossHealthBar != null)
            {
                bossHealthBar.value = currentBossHealth;
            }

            // Проверяем, побеждён ли босс
            if (currentBossHealth <= 0)
            {
                DefeatBoss();
            }
        }
    }

    // Метод для завершения битвы с боссом
    private void EndBossFight(bool isVictory)
    {
        isBossActive = false;

        // Деактивируем панель босса
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }

        // Убираем обработчик события для кнопки босса
        if (bossButton != null)
        {
            bossButton.onClick.RemoveListener(AttackBoss);
        }

        // Останавливаем таймер
        CancelInvoke(nameof(UpdateTimer));

        if (isVictory)
        {
            Debug.Log("Босс побеждён!");
            MessageManager.Instance.ShowMessage("Вы победили босса!", Color.green);
            clicker.AddClicks(bossReward); // Добавляем награду
        }
        else
        {
            Debug.Log("Вы проиграли боссу!");
            MessageManager.Instance.ShowMessage("Вы проиграли боссу!", Color.red);
            clicker.AddClicks(-bossPenalty); // Отнимаем штраф
        }

        clicker.UpdateAllScoreTexts(); // Обновляем интерфейс
    }

    // Метод для победы над боссом
    private void DefeatBoss()
    {
        EndBossFight(true);
    }

    // Метод для обновления таймера
    private void UpdateTimer()
    {
        currentTimeLeft--; // Уменьшаем таймер на 1 секунду

        // Обновляем текст таймера
        UpdateTimerText();

        // Если время истекло, завершаем бой как проигрыш
        if (currentTimeLeft <= 0)
        {
            Debug.Log("Время истекло! Бой завершён.");
            EndBossFight(false);
        }
    }

    // Метод для обновления текста таймера
    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = $"Время: {currentTimeLeft} сек";
            Debug.Log($"Оставшееся время: {currentTimeLeft} сек");
        }
    }
}