using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;  // Для работы с событиями UI

public class AchievementSliderControl : MonoBehaviour, IPointerDownHandler
{
    public Slider achievementSlider;  // Ссылка на слайдер достижения
    public bool isAchievementUnlocked;  // Статус достижения (заблокирован ли слайдер)
    
    // Этот метод срабатывает, когда пользователь пытается взаимодействовать с слайдером
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isAchievementUnlocked)
        {
            // Если достижение выполнено, блокируем перемещение слайдера
            eventData.pointerPress = null; // Отключаем взаимодействие
            Debug.Log("Перемещение слайдера отключено: достижение выполнено.");
        }
    }

    // Метод для обновления состояния слайдера (например, когда достижение выполнено)
    public void UpdateSliderState(bool achievementUnlocked)
    {
        isAchievementUnlocked = achievementUnlocked;

        // Блокируем слайдер для достижения, если оно выполнено
        if (isAchievementUnlocked)
        {
            achievementSlider.interactable = false;  // Блокируем слайдер
        }
        else
        {
            achievementSlider.interactable = true;   // Разрешаем слайдер
        }
    }
}
