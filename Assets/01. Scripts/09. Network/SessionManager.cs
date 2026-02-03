using System;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private readonly Dictionary<byte, GameObject> players = new Dictionary<byte, GameObject>();

    public IReadOnlyDictionary<byte, GameObject> Players => players;

    void Awake()
    {
        byte id = 1;
        players.Add(id, GameObject.Find("Player_Model"));
        Debug.Log($"player {id} : {players[id]}");
    }

    public void HandleDamage(string id, float damage)
    {
        throw new NotImplementedException();
    }
}