using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InfoText
{
    public string text;
    public MessageType messageType;
    [SerializeField] private bool show = true;

    public enum MessageType
    {
        None,
        Info,
        Warning,
        Error
    }

    public InfoText(string text, MessageType messageType = MessageType.None, bool show = true)
    {
        this.text = text;
        this.messageType = messageType;
        this.show = show;
    }

    public void Show()
    {
        show = true;
    }

    public void Hide()
    {
        show = false;
    }
    
    public void SetVisibility(bool show)
    {
        this.show = show;
    }

}