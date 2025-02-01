using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RouletteManager : MonoBehaviour
{
    [Header("Настройки рулетки")]
    public List<CaseItem> caseItems;  // Список предметов с шансом появления
    public List<Sprite> backgrounds;  // Список фонов для выпадения
    public int totalItemsCount = 1000; // Количество экземпляров в рулетке
    public int clickCost = 10; // Стоимость запуска рулетки в кликах

    private List<CaseItem> rouletteItems = new List<CaseItem>(); // Список предметов на рулетке
    private Clicker clicker; // Ссылка на скрипт Clicker

void Start()
{
    clicker = FindObjectOfType<Clicker>();
    if (clicker == null)
    {
        Debug.LogError("Clicker не найден на сцене!");
    }
}

    // Инициализация рулетки с учетом шансов
    private void InitializeRouletteItems()
    {
        rouletteItems.Clear();

        // Заполняем рулетку элементами с учетом шансов
        foreach (var item in caseItems)
        {
            int itemCount = Mathf.RoundToInt(item.spawnChance * totalItemsCount);

            // Добавляем элемент в рулетку столько раз, сколько раз он должен появиться
            for (int i = 0; i < itemCount; i++)
            {
                rouletteItems.Add(item);
            }
        }

        // Добавляем фоны в рулетку
        foreach (var bg in backgrounds)
        {
            // Можете задать шанс появления фона
            int bgCount = Mathf.RoundToInt(0.1f * totalItemsCount); // Пример: 10% для фонов
            for (int i = 0; i < bgCount; i++)
            {
                CaseItem backgroundItem = new CaseItem { itemSprite = bg, name = "Background" };
                rouletteItems.Add(backgroundItem);
            }
        }

        // Перемешиваем список предметов для случайного порядка
        Shuffle(rouletteItems);
    }

    // Метод для перемешивания списка (Фишер-Йейтс)
    private void Shuffle(List<CaseItem> list)
    {
        System.Random rand = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            CaseItem value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

// Метод для начала прокрутки рулетки с оплатой кликами
public void StartRoulette()
{
    if (Clicker.GetClickCount() >= clickCost)  // Используем статический метод через имя класса
    {
        Clicker.RemoveClicks(clickCost);  // Используем статический метод через имя класса
        StartCoroutine(SpinRoulette());  // Запускаем прокрутку
    }
    else
    {
        Debug.Log("Недостаточно кликов для запуска рулетки!");
    }
}

    private IEnumerator SpinRoulette()
    {
        // Определяем случайный индекс для выигрыша
        int totalItems = rouletteItems.Count;
        int targetIndex = Random.Range(0, totalItems);

        // Случайное время прокрутки
        float spinDuration = Random.Range(1f, 3f);
        float elapsedTime = 0f;

        // Прокрутка рулетки по оси X
        float startPosition = 0;
        float targetPosition = startPosition - (totalItems * 100);  // Параметры прокрутки могут изменяться в зависимости от вашего дизайна

        while (elapsedTime < spinDuration)
        {
            float t = elapsedTime / spinDuration;
            t = Mathf.SmoothStep(0, 1, t); // Плавное замедление
            float currentPosition = Mathf.Lerp(startPosition, targetPosition, t);
            // Нужно будет обновить позицию вашего UI панель для рулетки, чтобы она двигалась

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // После завершения прокрутки, обновляем позицию
        // roulettePanel.localPosition = new Vector3(targetPosition, 0, 0);

        // Обрабатываем результат
        HandleSpinResult(targetIndex);
    }

    private void HandleSpinResult(int targetIndex)
    {
        // Получаем предмет по индексу
        CaseItem resultItem = rouletteItems[targetIndex];

        // Выводим сообщение о выигрыше (можно вывести в UI)
        Debug.Log($"Поздравляем! Вы выиграли: {resultItem.name}");

        // Вы можете добавить логику для добавления предмета в инвентарь или другие действия
    }
}

// Структура для предмета рулетки
[System.Serializable]
public class CaseItem
{
    public string name; // Имя предмета
    public Sprite itemSprite; // Изображение предмета
    [Range(0, 1)] public float spawnChance; // Шанс появления предмета (от 0 до 1)
}
