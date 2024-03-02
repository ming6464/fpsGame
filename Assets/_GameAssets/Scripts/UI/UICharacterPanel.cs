using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterPanel : MonoBehaviour
{
    public void Home_button_on_click()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnHomePanel);
    }
}