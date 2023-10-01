using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform _interactionPoint;
    [SerializeField] private float _interactionPointRadius = 0.5f;
    [SerializeField] private LayerMask _interactableMask;

    private readonly Collider[] _colliders = new Collider[3];
    private Outline LastOutlined;
    [SerializeField] private int _numFound;
    
    private void Update()
    {
        _numFound = Physics.OverlapSphereNonAlloc(_interactionPoint.position, _interactionPointRadius, _colliders, _interactableMask);

        if (_numFound > 0)
        {
            var interactable = _colliders[0].GetComponent<IInteractable>();
            var outline = interactable.GetGameObject().GetComponent<Outline>();

            if (!outline.enabled)
            {
                handleOutline(outline);
                LastOutlined = outline;
            }

            if (interactable != null && Input.GetKey(KeyCode.E))
            {
                interactable.Interact(this);
            }
        }
        else
        {
            if (LastOutlined) {
                handleOutline(LastOutlined);
                LastOutlined = null;
            }
        }
        
    }

    private void handleOutline(Outline outline)
    {
        outline.enabled = !outline.enabled;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionPointRadius);
    }
}
