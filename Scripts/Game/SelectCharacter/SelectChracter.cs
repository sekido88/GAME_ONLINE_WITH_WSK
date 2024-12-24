using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacter : MonoBehaviour
{
    [SerializeField] private List<Sprite> playerSprites;
    [SerializeField] private List<GameObject> prefabSocketEffects;
    [SerializeField] private List<GameObject> prefabTrailEffects;
    [SerializeField] private GameObject pointSpawn;
    [SerializeField] private GameObject prefabSpawn; 
    [SerializeField] private GameObject pointSpawnPreviewPlayer;
    [SerializeField] private GameObject playerPrefab;

    private GameObject currentGameObjectPart;

    private GameObject currentPlayerPreview;
    private string currentTypeSelect = "sprite";

    
    // Trai Effect Animation
    float radius = 5f;
    float speed = 2f;
    public Dictionary<String, int> currentIndexes = new Dictionary<String, int>()
    {
        {"sprite",0},
        {"socketEffect",0},
        {"trailEffect",0}
    };

    private void Awake() {
        playerPrefab = GameManager.Instance.GetPlayerPrefab();
    }
    private void Start()
    {
       
        GeneratePlayerPreview();
        GeneratePartPlayerPreview();
    }

    private void Update()
    {

        if (currentGameObjectPart != null && currentTypeSelect == "trailEffect")
        {
            Vector3 center = pointSpawn.transform.position;
            float radius = 10f; 
            float speed = 3f;  

            float x = center.x + Mathf.Cos(Time.time * speed) * radius;
            float y = center.y + Mathf.Sin(Time.time * speed) * radius;

            currentGameObjectPart.transform.position = new Vector3(x, y, currentGameObjectPart.transform.position.z);
        }

    }
  
    private void GeneratePlayerPreview() {
        currentPlayerPreview = Instantiate(playerPrefab,pointSpawnPreviewPlayer.transform);
        currentPlayerPreview.GetComponent<SpriteRenderer>().sprite = playerSprites[currentIndexes["sprite"]];
        currentPlayerPreview.layer = LayerMask.NameToLayer("UI");
        currentPlayerPreview.GetComponent<SpriteRenderer>().sortingLayerName = "UI";
        currentPlayerPreview.transform.localScale = new Vector3(currentPlayerPreview.transform.localScale.x * 20, currentPlayerPreview.transform.localScale.y * 20, 0);
        currentPlayerPreview.transform.rotation = Quaternion.Euler(0, 0, 0);
 
    }

    private void HandlePlayerPreview() {
        currentPlayerPreview.GetComponent<SpriteRenderer>().sprite = playerSprites[currentIndexes["sprite"]];
        PlayerEffects playerEffects = currentPlayerPreview.GetComponent<PlayerController>().GetPlayerEffect();
        playerEffects.SetSocketEffect(prefabSocketEffects[currentIndexes["socketEffect"]]);
        playerEffects.SetTrailEffect(prefabTrailEffects[currentIndexes["trailEffect"]]);


    }
    public void Next(string type)
    {
        switch (type)
        {
            case "sprite":
                currentIndexes[type] = (currentIndexes[type] + 1) % playerSprites.Count;
                break;
            case "socketEffect":
                currentIndexes[type] = (currentIndexes[type] + 1) % prefabSocketEffects.Count;
                break;
            case "trailEffect":
                currentIndexes[type] = (currentIndexes[type] + 1) % prefabTrailEffects.Count;
                break;
        }
        GeneratePartPlayerPreview();
    }

    public void Previous(string type)
    {
        switch (type)
        {
            case "sprite":
                currentIndexes[type] = (currentIndexes[type] - 1 + playerSprites.Count) % playerSprites.Count;
                break;
            case "socketEffect":
                currentIndexes[type] = (currentIndexes[type] - 1 + playerSprites.Count) % prefabSocketEffects.Count;
                break;
            case "trailEffect":
                currentIndexes[type] = (currentIndexes[type] - 1 + playerSprites.Count) % prefabTrailEffects.Count;
                break;
        }
        GeneratePartPlayerPreview();
    }


    public object GetCurrent(string type)
    {
        return type switch
        {
            "sprite" => playerSprites[currentIndexes[type]],
            "socketEffect" => prefabSocketEffects[currentIndexes[type]],
            "trailEffect" => prefabTrailEffects[currentIndexes[type]],
            _ => null
        };
    }

    public string GetCurrentType()
    {
        return currentTypeSelect;
    }

    public void SetCurrentType(string type)
    {
        currentTypeSelect = type;
        GeneratePartPlayerPreview();
    }

    public void GeneratePartPlayerPreview()
    {
        DestroyCurrentGameObject();
        HandlePlayerPreview();
        switch (currentTypeSelect)
        {
            case "sprite":
                currentGameObjectPart = Instantiate(prefabSpawn, pointSpawn.transform.position,
                Quaternion.identity, pointSpawn.transform);
                Image image = currentGameObjectPart.AddComponent<Image>();
                image.sprite = playerSprites[currentIndexes["sprite"]];
                break;
            case "socketEffect":
                currentGameObjectPart = Instantiate(prefabSocketEffects[currentIndexes["socketEffect"]],
                pointSpawn.transform.position, Quaternion.Euler(0, 0, 180), pointSpawn.transform);
                currentGameObjectPart.transform.localScale = new Vector3(currentGameObjectPart.transform.localScale.x * 2, currentGameObjectPart.transform.localScale.y * 2, 0);
                break;
            case "trailEffect":
                currentGameObjectPart = Instantiate(prefabTrailEffects[currentIndexes["trailEffect"]],
                pointSpawn.transform.position, Quaternion.identity, pointSpawn.transform);
                TrailRenderer lineRenderer = currentGameObjectPart.GetComponent<TrailRenderer>();
                lineRenderer.startWidth = 1.5f;
                lineRenderer.endWidth = 1.5f;
                                                         
                break;
        }
    }

    public void DestroyCurrentGameObject()
    {
        if (currentGameObjectPart != null)
        {
            Destroy(currentGameObjectPart);
        }
    }

    public List<Sprite> GetPlayerSprites() {
        return playerSprites;
    }

    public List<GameObject> GetPrefabSocketEffects() {
        return prefabSocketEffects;
    }

    public List<GameObject> GetPrefabTtrailEffects() {
        return prefabTrailEffects;
    }
}
