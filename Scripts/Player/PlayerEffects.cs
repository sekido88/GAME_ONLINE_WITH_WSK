using UnityEngine;

public class PlayerEffects : MonoBehaviour 
{
    [SerializeField] private Transform firePoint;

    [Header("Effect Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color boostColor = Color.red;

    [SerializeField] private GameObject prefabSocketEffect;
    [SerializeField] private GameObject prefabTrailEffect;

   private GameObject currentSocketEffect;
    private GameObject currentTrailEffect;
    private ParticleSystem socketEffect;
    private TrailRenderer trailEffect;

    private string socketEffectName;
    private string trailEffectName;


    public void UpdateEffects(bool isAccelerating)
    {
        UpdateSocketEffect(isAccelerating);
        UpdateTrailEffect(isAccelerating);
    }

    private void UpdateSocketEffect(bool isAccelerating)
    {
        if (socketEffect == null) return;

      
        if (isAccelerating && !socketEffect.isPlaying)
        {
            socketEffect.Play();
            AudioManager.Instance.PlaySFX("wind");
        }
        else if (!isAccelerating && socketEffect.isPlaying)
        {
            socketEffect.Stop();
            AudioManager.Instance.StopSfx();
        }


    }

    private void UpdateTrailEffect(bool isAccelerating)
    {
        if (trailEffect == null) return;

        Color trailColor = isAccelerating ? boostColor : normalColor;
        trailEffect.startColor = trailColor;
        trailEffect.endColor = trailColor;
    }

    public void SetEffect(GameObject socketEffectPrefab, GameObject trailEffectPrefab) 
        {
            socketEffectName = socketEffect?.name;
            trailEffectName = trailEffect?.name;
            
            if (currentSocketEffect != null) Destroy(currentSocketEffect);
            if (currentTrailEffect != null) Destroy(currentTrailEffect);

            // Store effect names
            socketEffectName = socketEffectPrefab.name;
          
            // Instantiate new effects
            currentSocketEffect = Instantiate(socketEffectPrefab, firePoint);
            currentTrailEffect = Instantiate(trailEffectPrefab, firePoint);
            
            // Setup components
            socketEffect = currentSocketEffect.GetComponent<ParticleSystem>();
            trailEffect = currentTrailEffect.GetComponent<TrailRenderer>();

            // Configure transforms
            currentSocketEffect.transform.localPosition = Vector3.zero;
            currentTrailEffect.transform.localPosition = Vector3.zero;

            // Ensure effects are enabled
            currentSocketEffect.SetActive(true);
            currentTrailEffect.SetActive(true);
        }

    public string GetSocketEffectName() => socketEffectName;
    public string GetTrailEffectName() => trailEffectName;
}