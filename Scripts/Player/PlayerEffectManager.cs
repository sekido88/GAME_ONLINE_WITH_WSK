using UnityEngine;
using System.Collections.Generic;

public class PlayerEffectsManager : MonoBehaviour
{
    public static PlayerEffectsManager Instance { get; private set; }

    [Header("Default Effects")]
    [SerializeField] private string defaultSocketEffectName = "Socket Fire Effect Black";
    [SerializeField] private string defaultTrailEffectName = "Trail Effect Black";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckAndApplyEffectsToAllPlayers()
    {
        Dictionary<string, GameObject> players = GameManager.Instance.players;

        foreach (var playerPair in players)
        {
            GameObject playerObj = playerPair.Value;
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            PlayerEffects playerEffects = playerController.GetPlayerEffect();
            if (playerObj.transform.GetChild(0).childCount != 2)
            {


                bool haveSocketFire = false;
                bool haveTrailEffect = false;

                for (int i = 0; i < playerObj.transform.GetChild(0).childCount; i++)
                {


                    string name = playerObj.transform.GetChild(0).GetChild(i).gameObject.name;
                    string[] words = name.Split(' ');
                    string ssName = words[0] + " " + words[1];
                    if (ssName == "Socket Fire")
                    {
                        haveSocketFire = true;
                    }
                    if (ssName == "Trail Effect")
                    {
                        haveTrailEffect = true;
                    }
                }

                if (!haveSocketFire)
                {
                   GameObject obj =  Instantiate(GameManager.Instance.socketEffects[defaultSocketEffectName], playerObj.transform.GetChild(0));
                   obj.transform.localPosition = new Vector3(0, 0,0);
                }

                if (!haveTrailEffect)
                {
                   GameObject obj =  Instantiate(GameManager.Instance.trailEffects[defaultTrailEffectName], playerObj.transform.GetChild(0));
                   obj.transform.localPosition = new Vector3(0, 0,0);
                }
            }

        }
    }

    private void ApplyDefaultEffects(PlayerController player, PlayerEffects playerEffects)
    {
        GameObject socketEffect = null;
        GameObject trailEffect = null;

        if (GameManager.Instance.socketEffects.TryGetValue(defaultSocketEffectName, out GameObject socket))
        {
            socketEffect = socket;
        }

        if (GameManager.Instance.trailEffects.TryGetValue(defaultTrailEffectName, out GameObject trail))
        {
            trailEffect = trail;
        }

        if (socketEffect != null && trailEffect != null)
        {
            playerEffects.SetEffect(socketEffect, trailEffect);
            Debug.Log($"Applied effects to player {player.PlayerName}");
        }
        else
        {
            Debug.LogWarning($"Could not find default effects for player {player.PlayerName}");
        }
    }

    public void SetDefaultEffects(string socketEffectName, string trailEffectName)
    {
        defaultSocketEffectName = socketEffectName;
        defaultTrailEffectName = trailEffectName;
    }

    // Thêm các phương thức tiện ích
    public void ApplyEffectsToSinglePlayer(string playerId)
    {
        if (GameManager.Instance.players.TryGetValue(playerId, out GameObject playerObj))
        {
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            PlayerEffects playerEffects = playerController?.GetPlayerEffect();

            if (playerEffects != null)
            {
                ApplyDefaultEffects(playerController, playerEffects);
            }
        }
    }

    public bool HasPlayerEffects(string playerId)
    {
        if (GameManager.Instance.players.TryGetValue(playerId, out GameObject playerObj))
        {
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            PlayerEffects playerEffects = playerController?.GetPlayerEffect();

            if (playerEffects != null)
            {
                return !string.IsNullOrEmpty(playerEffects.GetSocketEffectName()) &&
                       !string.IsNullOrEmpty(playerEffects.GetTrailEffectName());
            }
        }
        return false;
    }
}