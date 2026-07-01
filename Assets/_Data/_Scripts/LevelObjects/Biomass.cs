using System.Collections;
using Cainos.InteractivePixelWater;
using DG.Tweening;
using UnityEngine;

public class Biomass : MonoBehaviour, IInteractable
{
    [SerializeField] private float mass = 1f;
    
    [SerializeField] private GameObject waterfallClean;
    [SerializeField] private GameObject waterfallDirty;

    [Header("Waterfall Config")]
    [SerializeField] private float waterfallDrainDuration = 0.6f;
    [SerializeField] private Ease waterfallDrainEase = Ease.InQuad;
    [SerializeField] private bool deactivateWaterfallAfterDrain = true;
 
    private static readonly int IsShowering = Animator.StringToHash("IsShowering");
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int IsTryingToMove = Animator.StringToHash("IsTryingToMove");
    
    private GameObject activeWaterfall;
    private PixelWaterfall waterfallController;
    private Animator interactorAnimator;
    private Coroutine interactCoroutine;
    
    private bool AddsWeight => mass > 0;
    private bool hasBeenConsumed;
    
    private void Awake()
    {
        waterfallClean.SetActive(false);
        waterfallDirty.SetActive(false);
    }

    private void Start()
    {
        SetWaterfall();
    }

    private void SetWaterfall()
    {
        if (!waterfallClean || !waterfallDirty)
            return;
        
        if (AddsWeight)
        {
            waterfallClean.SetActive(false);
            waterfallDirty.SetActive(true);
            activeWaterfall = waterfallDirty;
        }
        else
        {
            waterfallDirty.SetActive(false);
            waterfallClean.SetActive(true);
            activeWaterfall = waterfallClean;
        }
    }
    

    public void Interact(GameObject interactor)
    {
        if (hasBeenConsumed) return;
        if (interactCoroutine != null) return;

        interactCoroutine = StartCoroutine(InteractRoutine(interactor));
    }

    private IEnumerator InteractRoutine(GameObject interactor)
    {
        yield return MovementHelper.MoveTowards(interactor, transform);

        DrinkWater(interactor);

        interactCoroutine = null;
    }

    private void DrinkWater(GameObject interactor)
    {
        interactor?.GetComponent<IProvidesWeight>()?.AddWeight(mass, false);

        waterfallController = activeWaterfall.GetComponentInChildren<PixelWaterfall>();
        interactorAnimator = interactor?.GetComponentInChildren<Animator>();
        
        StartCoroutine(DrainActiveWaterfall(interactor));
    }
    
    private IEnumerator DrainActiveWaterfall(GameObject interactor)
    {
        if (!activeWaterfall)
            yield break;
        
        if (!waterfallController)
        {
            activeWaterfall.SetActive(false);
            yield break;
        }

        float startHeight = waterfallController.Height;
        float endHeight = 0.03125f;
        
        interactorAnimator?.SetTrigger(IsShowering);
        
        DOTween.To(
                () => startHeight,
                value =>
                {
                    startHeight = value;
                    waterfallController.SetHeight(value);
                },
                endHeight,
                waterfallDrainDuration
            )
            .SetEase(waterfallDrainEase)
            .OnComplete(() =>
            {
                if (deactivateWaterfallAfterDrain && activeWaterfall)
                    activeWaterfall.SetActive(false);
            });
        
        yield return new  WaitForSeconds(waterfallDrainDuration);
        
        interactorAnimator?.SetTrigger(IsTryingToMove);
        hasBeenConsumed = true;
    }
    
    private void OnValidate()
    {
        SetWaterfall();
    }
}
