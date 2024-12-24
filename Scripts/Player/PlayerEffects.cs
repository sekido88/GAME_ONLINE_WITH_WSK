using UnityEngine;

public class PlayerEffects : MonoBehaviour 
{
    [SerializeField] private Transform firePoint;

    [Header("Effect Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color boostColor = Color.red;

    [SerializeField] private GameObject prefabSocketEffect;
    [SerializeField] private GameObject prefabTrailEffect;

    private GameObject objSocketEffect;
    private GameObject objTrailEffect;
    private ParticleSystem socketEffect;
    private TrailRenderer trailEffect;

    private string socketEffectName;
    private string trailEffectName;
    private void Awake()
    {
        // InitializeEffects();
        objSocketEffect = transform.GetChild(0).transform.GetChild(0).gameObject;
        objTrailEffect = transform.GetChild(0).transform.GetChild(1).gameObject;
    }

    private void InitializeEffects()
    {
        InitSocketEffect();
        InitTrailEffect();
    }

    private void InitSocketEffect()
    {
        if (prefabSocketEffect == null || firePoint == null) return;

        objSocketEffect = Instantiate(prefabSocketEffect, firePoint);
        socketEffect = objSocketEffect.GetComponent<ParticleSystem>();
        objSocketEffect.transform.localPosition = Vector3.zero;
    }

    private void InitTrailEffect()
    {
        if (prefabTrailEffect == null || firePoint == null) return;

        objTrailEffect = Instantiate(prefabTrailEffect, firePoint);
        trailEffect = objTrailEffect.GetComponent<TrailRenderer>();
        objTrailEffect.transform.localPosition = Vector3.zero;
    }

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
        }
        else if (!isAccelerating && socketEffect.isPlaying)
        {
            socketEffect.Stop();
        }
    }

    private void UpdateTrailEffect(bool isAccelerating)
    {
        if (trailEffect == null) return;

        Color trailColor = isAccelerating ? boostColor : normalColor;
        trailEffect.startColor = trailColor;
        trailEffect.endColor = trailColor;
    }

    public Transform GetFirePoint()
    {
        return firePoint;
    }

    public void SetSocketEffect(GameObject socketEffect)
    {
        if (socketEffect == null) return;
        socketEffectName = socketEffect.name;
        prefabSocketEffect = socketEffect;
        DestroyObjSocketEffect();
        InitSocketEffect();
    }

    public void SetTrailEffect(GameObject trailEffect)
    {
        if (trailEffect == null) return;
        trailEffectName = trailEffect.name;
        prefabTrailEffect = trailEffect;
        DestroyObjTrailEffect();
        InitTrailEffect();
    }

    private void DestroyObjSocketEffect()
    {
        if (objSocketEffect != null)
        {
            Destroy(objSocketEffect);
        }
    }

    private void DestroyObjTrailEffect()
    {
        if (objTrailEffect != null)
        {
            Destroy(objTrailEffect);
        }
    }

    private void OnDestroy()
    {
        DestroyObjSocketEffect();
        DestroyObjTrailEffect();
    }
    public void SetEffect(GameObject socketEffect, GameObject trailEffect) {
        for(int i = 0 ;i < transform.GetChild(0).childCount ;i++) {
            Destroy(transform.GetChild(0).transform.GetChild(i).gameObject);
        }
        prefabSocketEffect = socketEffect;
        prefabTrailEffect = trailEffect;
        InitSocketEffect();
        InitTrailEffect();
    }

    public string GetSocketEffectName() => socketEffectName;
    public string GetTrailEffectName() => trailEffectName;
}