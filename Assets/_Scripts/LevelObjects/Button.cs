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
    
    [SerializeField]
    private ButtonConfig buttonConfig;

    [SerializeField] 
    private List<GameObject> connectedObjects;
    
    private List<IProvidesWeight> _weightProviders;
    
    /* Methods */
    
    /* Uniques */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            HasEnoughWeight();
            print("Checking weight");
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            RegisterWeight(1);
            print("Weight added");
        }
    }


    /* IActivable */
    public void Activate()
    {
        connectedObjects.ForEach(obj =>
        {
            if (obj.TryGetComponent(out IActivable a))
                a.Activate();
        });
    }

    public void Deactivate()
    {
        connectedObjects.ForEach(obj =>
        {
            if (obj.TryGetComponent(out IActivable a))
                a.Deactivate();
        });
    }

    
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
            print("Enough weight");
            return true;
        }
        
        if  (buttonConfig.requiredWeight > CurrentWeight && IsActive)
        {
            IsActive = false;
            Deactivate();
            print("Not enough weight and already deactivated");
        }
    
        print("Not enough weight");
        return false;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IProvidesWeight weightProvider = other.GetComponent<IProvidesWeight>();
        if ( weightProvider != null)
        {
            _weightProviders.Add(weightProvider);
            RegisterWeight(weightProvider);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IProvidesWeight weightProvider = other.GetComponent<IProvidesWeight>();
        if ( weightProvider != null)
        {
            _weightProviders.Add(weightProvider);
            UnregisterWeight(weightProvider);
        }
    }
}
