using System;
using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class HiddenArea : MonoBehaviour
{
    [Header("Follow offset")]
    [SerializeField] private CinemachineFollow followCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 5f);
    
    [Header("Camera FOV")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float fieldOfView = 80f;
    
    [Header("Camera Target")]
    [SerializeField] private CameraController cameraTarget;
    [SerializeField] private Transform target;
    
    [Header("Tween")]
    [SerializeField] private float zoomDuration = 0.35f;
    [SerializeField] private Ease zoomEase = Ease.OutQuad;
    
    private Collider2D coll;
    private Vector3 prevFollow;
    private Tween followTween;
    
    private void Awake()
    {
        coll = GetComponent<Collider2D>();
        coll.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        prevFollow = followCamera.FollowOffset;

        if (cameraTarget && target) cameraTarget.player = target;
        if (cinemachineCamera) cinemachineCamera.Lens.FieldOfView = fieldOfView;
        
        Vector3 targetOffset = offset;

        TweenFollowOffset(targetOffset);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        if (cameraTarget) cameraTarget.player = GameController.Instance?.GetPlayer().transform;
        if (cinemachineCamera) cinemachineCamera.Lens.FieldOfView = 60f;
        
        TweenFollowOffset(prevFollow);
    }

    private void TweenFollowOffset(Vector3 targetOffset)
    {
        followTween?.Kill();

        followTween = DOTween.To(
                () => followCamera.FollowOffset,
                value => followCamera.FollowOffset = value,
                targetOffset,
                zoomDuration
            )
            .SetEase(zoomEase)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        followTween?.Kill();

        if (followCamera != null)
        {
            followCamera.FollowOffset = prevFollow;
        }
    }
}