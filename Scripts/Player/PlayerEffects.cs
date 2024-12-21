
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private TrailRenderer mainTrail;
    [SerializeField] private Transform firePoint;

    [Header("Effect Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color boostColor = Color.red;

    private void Awake()
    {
        InitializeEffects();
    }

    private void InitializeEffects()
    {
        if (fireEffect != null && firePoint != null)
        {
            var newFireEffect = Instantiate(fireEffect, firePoint);
            fireEffect = newFireEffect;
            fireEffect.transform.localPosition = Vector3.zero;
            // fireEffect.transform.localRotation = Quaternion.identity;
        }

        if (mainTrail != null  && firePoint != null) {

            var newTrail = Instantiate(mainTrail, firePoint);
            mainTrail = newTrail;
            mainTrail.transform.localPosition = Vector3.zero;
        }
    }

   
    public void UpdateEffects(bool isAccelerating)
    {
        if (fireEffect != null)
        {
            if (isAccelerating && !fireEffect.isPlaying)
            {
                fireEffect.Play();
            }
            else if (!isAccelerating && fireEffect.isPlaying)
            {
                fireEffect.Stop();
            }
        }

        Color trailColor = isAccelerating ? boostColor : normalColor;
        
        mainTrail.startColor = trailColor;
        mainTrail.endColor = trailColor;
    }

    public Transform GetFirePoint() {
        return firePoint;
    }
}