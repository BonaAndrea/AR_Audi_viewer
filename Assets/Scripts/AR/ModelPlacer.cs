using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ModelPlacer : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject modelPrefab;
    public GameObject SpawnedModel => _spawnedModel;
    private GameObject _spawnedModel;
    private bool _isPlacementMode = true;
    private static readonly List<ARRaycastHit> Hits = new();

    public bool IsPlacementMode => _isPlacementMode;
    public bool HasModel => _spawnedModel != null;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {

            if (!_isPlacementMode) return;

            var touches = Touch.activeTouches;


        if (touches.Count == 0) return;

        var touch = touches[0];
        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;

        if (raycastManager.Raycast(touch.screenPosition, Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = Hits[0].pose;

            if (_spawnedModel == null)
            {
                _spawnedModel = Instantiate(modelPrefab, hitPose.position, hitPose.rotation);
                var handler = _spawnedModel.GetComponent<TouchHandler>();
                if (handler != null) handler.enabled = false;
            }
            else
            {
                _spawnedModel.transform.position = hitPose.position;
            }
        }
    }

    public void ToggleMode()
    {
        _isPlacementMode = !_isPlacementMode;

        if (_spawnedModel != null)
        {
            var handler = _spawnedModel.GetComponent<TouchHandler>();
            if (handler != null)
                handler.enabled = !_isPlacementMode;
        }
    }

    public string GetModeLabel()
    {
        return _isPlacementMode ? "Modalità: Posiziona" : "Modalità: Manipola";
    }
}