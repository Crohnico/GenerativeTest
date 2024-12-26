using UnityEngine;
using UnityEngine.EventSystems;

public class MapPainterInput : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public MapPainter painter;

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)painter.mapDisplay.transform,
            eventData.position,
            eventData.pressEventCamera,
            out localCursor))
        {
            Vector2 pixelPos = new Vector2(
                (localCursor.x + painter.mapDisplay.rectTransform.rect.width / 2) / painter.mapDisplay.rectTransform.rect.width * painter.mapSize,
                (localCursor.y + painter.mapDisplay.rectTransform.rect.height / 2) / painter.mapDisplay.rectTransform.rect.height * painter.mapSize
            );

            painter.PaintAtPosition(pixelPos);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnPointerDown(eventData);
    }
}
