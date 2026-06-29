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
    
    private GameObject activeWaterfall;
    
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
        DrinkWater(interactor);
    }

    private void DrinkWater(GameObject interactor)
    {
        if (hasBeenConsumed) return;
        
        interactor?.GetComponent<IProvidesWeight>()?.AddWeight(mass);

        Rigidbody2D d = interactor?.GetComponent<Rigidbody2D>();
        if (d) d.mass += mass * 20;

        DrainActiveWaterfall();
    }
    
    private void DrainActiveWaterfall()
    {
        if (activeWaterfall == null)
            return;

        PixelWaterfall waterfall = activeWaterfall.GetComponentInChildren<PixelWaterfall>();

        if (waterfall == null)
        {
            activeWaterfall.SetActive(false);
            return;
        }

        float startHeight = waterfall.Height;
        float endHeight = 0.03125f;

        DOTween.To(
                () => startHeight,
                value =>
                {
                    startHeight = value;
                    waterfall.SetHeight(value);
                },
                endHeight,
                waterfallDrainDuration
            )
            .SetEase(waterfallDrainEase)
            .OnComplete(() =>
            {
                if (deactivateWaterfallAfterDrain && activeWaterfall != null)
                    activeWaterfall.SetActive(false);
            });
        
        hasBeenConsumed = true;
    }
    
    private void OnValidate()
    {
        SetWaterfall();
    }
}
