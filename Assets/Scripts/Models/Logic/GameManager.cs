﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> 
{
    [SerializeField] private int totalPlayers;
    [SerializeField] private int numPlayers;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private AudioClip startSound;
    // TODO: Temporary 
    [SerializeField] private float winnerTime;
    //
    [SerializeField] private Transform spawnPositions;
    [SerializeField] private Transform healthPanel;

    private Player[] players;
    private Player winner;
    private int infected;
    private int remain;
    private bool started;
    private RouletteController roulette;
    private Notifier notifier;

    public Player Winner
    {
        get { return winner; }
    }

	void Start () 
    {
        this.remain = this.totalPlayers;
        this.players = new Player[this.totalPlayers];
        for (int i = 0; i < this.totalPlayers; i++) 
        {
            Vector3 position = spawnPositions.GetChild(i).position;
            Quaternion rotation = Quaternion.identity;
            this.players[i] = Instantiate<Player>(playerPrefab, position, rotation);
            this.players[i].Number = i;
            this.players[i].Playable = i < numPlayers;
            this.players[i].UI = healthPanel.GetChild(i).GetComponent<PlayerUIController>();
        }
        this.started = false;
        this.roulette = GetComponent<RouletteController>();

        // Notifier
        notifier = new Notifier();
        notifier.Subscribe(Player.ON_DIE, HandleOnDie);
        notifier.Subscribe(RouletteController.ON_FINISH_SELECTED, HandleOnSelectedInfected);

	}
    private void Update()
    {
        if( StateManager.Instance.State == GameState.Start &&
            Input.GetKeyUp(KeyCode.Return) && !started)
        {
            started = true;
            StartCoroutine(this.SpinRoulette());
            AudioManager.Instance.PlayOneShoot2D(startSound, 0.5f);
        }
        if ( StateManager.Instance.State == GameState.End &&
            Input.GetKeyUp(KeyCode.Return))
        {
            // TODO: Change This!
            SceneManager.LoadScene("Main");
        }
    }

    public void Infect(int player)
    {
        if (this.infected != player &&
            this.players[player].State == PlayerState.Human)
        {
            this.players[player].Mutate(PlayerState.Infected);

            if (this.players[this.infected].State == PlayerState.Infected)
            {
                this.players[this.infected].Mutate(PlayerState.Human);
            }
            else if (this.players[this.infected].State == PlayerState.MadChicken)
            {
                this.players[this.infected].Mutate(PlayerState.Chicken);
            }
            this.infected = player;
        }
    }

    private void HandleOnDie(params object[] args)
    {
        UpdateRemain();
    }

    private void HandleOnSelectedInfected(object[] args)
    {
        this.infected = (int)args[0];
        //Debug.Log("Manager - Infected: " + infected);
        this.players[this.infected].Mutate(PlayerState.Infected);
        StateManager.Instance.State = GameState.Battle;
    }

    private void UpdateRemain()
    {
        this.remain--;
        if (this.remain == 2)
        {
            StateManager.Instance.State = GameState.StressBattle;
        }
        else if (remain <= 1)
        {
            this.CheckWinner();
            this.winner.Win();
            // TODO: Not working
            this.players[this.infected].Mutate(PlayerState.Chicken);
            //
            StateManager.Instance.State = GameState.Winner;
            StartCoroutine(this.WinnerWait(this.winnerTime));
        }
    }
    private void CheckWinner()
    {
        for (int i = 0; i < this.totalPlayers; i++)
        {
            if (this.players[i].State == PlayerState.Human)
            {
                this.winner = this.players[i];
                return;
            }
        }
    }
    private IEnumerator SpinRoulette()
    {
        yield return new WaitForSeconds(2.75f);
        StateManager.Instance.State = GameState.Roulette;
        this.roulette.Initialize(this.totalPlayers);
    }

    private IEnumerator WinnerWait(float time)
    {
        yield return new WaitForSeconds(time);
        StateManager.Instance.State = GameState.End;
    }

    void OnDestroy()
    {
        notifier.UnsubcribeAll();
    }

}
