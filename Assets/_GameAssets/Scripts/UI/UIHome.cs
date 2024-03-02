using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHome : MonoBehaviour
{
    public void Play_button_on_click()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnPlayGame);
    }

    public void Bag_button_on_click()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnBagPanel);
    }

    public void Character_button_on_click()
    {
        EventDispatcher.Instance.PostEvent(EventID.OnCharacterPanel);
    }
}