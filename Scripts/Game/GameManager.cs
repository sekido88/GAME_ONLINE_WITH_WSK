using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
   [SerializeField] private GameObject playerPrefab;
  //  private Dictionary<string, PlayerController> players = new Dictionary<string, PlayerController>();
       private bool isRaceStarted = false;
    private float countdownTime = 3f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

  public void SpawnLocalPlayer()
    {
        GameObject playerObj = Instantiate(playerPrefab);
        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        string playerId = System.Guid.NewGuid().ToString();
        playerController.Initialize(playerId, "Player" + playerId.Substring(0, 4), true);
        
        // Thông báo cho server về player mới
        NetworkManager.Instance.SendMessage("player_joined", new {
            playerId = playerId,
            position = playerObj.transform.position,
            rotation = playerObj.transform.rotation
        });
    }

        public void StartRace()
    {
        isRaceStarted = true;
        NetworkManager.Instance.SendMessage("race_started", new {});
    }
} 