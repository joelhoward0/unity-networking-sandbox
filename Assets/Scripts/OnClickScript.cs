using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnClickScript : MonoBehaviour, IPointerClickHandler {

  public UnityEvent onClick;

  public void OnPointerClick(PointerEventData eventData) {
    onClick.Invoke();
  }
}