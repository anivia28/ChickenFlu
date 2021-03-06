﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UIState
{
    public GameState state;
    public Canvas canvas;
}

public class UIController : Singleton<UIController> 
{
    [SerializeField] private UIState[] states;

    private Notifier notifier;

    void Awake()
    {
        notifier = new Notifier();
        notifier.Subscribe(StateManager.ON_STATE_ENTER, HandleOnEnter);
        notifier.Subscribe(StateManager.ON_STATE_EXIT, HandleOnExit);
    }

    void Start()
    {
        this.HandleOnEnter(StateManager.Instance.State);
    }

    void HandleOnEnter(params object[] args)
    {
        GameState state = (GameState)args[0];
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state == state)
            {
                this.states[i].canvas.enabled = true;
            }
        }
        Debug.Log("UI - Setting canvas for state: " + state);
    }

    void HandleOnExit(params object[] args)
    {
        GameState state = (GameState)args[0];
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state == state)
            {
                this.states[i].canvas.enabled = false;
            }
        }
    }

    void OnDestroy()
    {
        notifier.UnsubcribeAll();
    }
}