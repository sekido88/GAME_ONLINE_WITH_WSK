using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;        
    [SerializeField] private float smoothSpeed = 5f;  
    [SerializeField] private Vector3 offset = Vector3.zero;        

    private void Start() {
       
    }
    private void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    public void setTarget(Transform target) {
        this.target = target;
    }
}