using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof(BoxCollider2D) )]

public class Button : MonoBehaviour, IActivable, IDetectsWeight
{
    
    /* Attributes */
    public bool IsActive { get; private set; } = false;
    
    public float CurrentWeight { get; set; }
    
    [SerializeField] private ButtonConfig buttonConfig;

    [SerializeField] private List<GameObject> connectedObjects;
    private List<IProvidesWeight> _weightProviders;

    [SerializeField] private bool deactivates = true;

    [Header("Audio")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private float activationSoundVolume;
    [SerializeField] private AudioClip deactivationSound;
    [SerializeField] private float deactivationSoundVolume;

    
    /* IActivable */
    public void Activate()
    {
        connectedObjects.ForEach(obj =>
        {
            if (obj.TryGetComponent(out IActivable a))
                a?.Activate();
        });
        
        if (activationSound) AudioManager.Instance?.PlayEffect(activationSound, transform.position, activationSoundVolume);
    }

    public void Deactivate()
    {
        connectedObjects.ForEach(obj =>
        {
            if (!obj) return;
            if (obj.TryGetComponent(out IActivable a))
                a?.Deactivate();
        });
        
        if (deactivationSound) AudioManager.Instance?.PlayEffect(deactivationSound, this.transform.position, deactivationSoundVolume);
    }

    
    /* ### [FOR DEBUG] ### */
    /*public void Activate()
    {
        for (int i = connectedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = connectedObjects[i];

            Debug.Log($"Connected object [{i}]: {obj}", this);

            if (obj == null)
            {
                Debug.LogWarning($"El objeto conectado [{i}] está null/destruido", this);
                connectedObjects.RemoveAt(i);
                continue;
            }

            if (obj.TryGetComponent(out IActivable a))
                a.Activate();
        }

        if (activationSound && AudioManager.Instance != null)
            AudioManager.Instance.PlayEffect(activationSound);
    }
    public void Deactivate()
    {
        for (int i = connectedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = connectedObjects[i];

            Debug.Log($"Connected object [{i}]: {obj}", this);

            if (obj == null)
            {
                Debug.LogWarning($"El objeto conectado [{i}] está null/destruido", this);
                connectedObjects.RemoveAt(i);
                continue;
            }

            if (obj.TryGetComponent(out IActivable a))
                a.Deactivate();
        }

        if (deactivationSound && AudioManager.Instance != null)
            AudioManager.Instance.PlayEffect(deactivationSound, transform.position);
    }
    */
    
    /* IDetectsWeight */
    public float GetWeight()
    {
        return CurrentWeight;
    }
    
    public void RegisterWeight(IProvidesWeight weightProvider)
    {
        CurrentWeight += weightProvider.Weight;
        HasEnoughWeight();
    }

    public void RegisterWeight(float weight)
    {
        CurrentWeight += weight;
        HasEnoughWeight();
    }

    public void UnregisterWeight(IProvidesWeight weightProvider)
    {
        CurrentWeight -= weightProvider.Weight;
        HasEnoughWeight();
    }

    public void UnregisterWeight(float weight)
    {
        CurrentWeight -= weight;
        HasEnoughWeight();
    }

    public bool HasEnoughWeight()
    {
        if (buttonConfig.requiredWeight <= CurrentWeight && !IsActive)
        {
            IsActive = true;
            Activate();
            return true;
        }
        
        if  (buttonConfig.requiredWeight > CurrentWeight && IsActive)
        {
            IsActive = false;
            Deactivate();
        }
    
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IProvidesWeight weightProvider = other.GetComponent<IProvidesWeight>();
        if (weightProvider == null) return;
        
        //_weightProviders.Add(weightProvider);
        RegisterWeight(weightProvider);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IProvidesWeight weightProvider = other.GetComponent<IProvidesWeight>();
        if (weightProvider == null || !deactivates) return;
        
        //_weightProviders.Add(weightProvider);
        UnregisterWeight(weightProvider);
    }
}
