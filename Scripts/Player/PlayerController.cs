using UnityEngine;
using Unity.VisualScripting;
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private PlayerEffects playerEffects;
    private PlayerStats playerStats;
    private Rigidbody2D rb;

    [Header("Player Info")]

    string tmp;
    public string PlayerId { get; private set; }
    public string PlayerName { get; private set; }
    public bool IsLocalPlayer { get; private set; }
    public int CurrentLap { get; private set; }
    public int CurrentCheckpoint { get; private set; }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
   
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        playerEffects = GetComponent<PlayerEffects>();
        playerStats = GetComponent<PlayerStats>();
    }

    public void Initialize(string id, string name, bool isLocalPlayer)
    {
        PlayerId = id;
        PlayerName = name;
        IsLocalPlayer = isLocalPlayer;

        this.gameObject.name = PlayerName;

        playerInput.IsLocalPlayer = isLocalPlayer;
        CurrentLap = 0;
        CurrentCheckpoint = -1;
    }

    private void Update()
    {
        if (playerEffects != null)
        {
            playerEffects.UpdateEffects(playerMovement.getIsDrifting());
        }

    }

 
    public void ResetPlayer()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        CurrentLap = 0;
        CurrentCheckpoint = -1;
    }

    public PlayerMovement GetPlayerMoment()
    {
        return playerMovement;
    }

    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }

}