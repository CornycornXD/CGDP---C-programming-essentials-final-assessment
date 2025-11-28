using System;
using UnityEngine;

public class smoothFollowCam : MonoBehaviour
{
    [SerializeField] private Transform camTarget;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float smoothTime = 0.25f;
    private Vector2 velocity = Vector2.zero;

    private void FixedUpdate()
    {
        Vector2 targetPosition =  (Vector2)camTarget.position + offset;
        Vector2 newPosition = Vector2.SmoothDamp((Vector2)transform.position, targetPosition, ref velocity, smoothTime);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }
}
