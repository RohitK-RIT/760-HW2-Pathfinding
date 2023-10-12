using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    public static event Action<Vector2> OnMouseClick; 

    private Camera _mainCam;

    private void Start()
    {
        _mainCam = Camera.main;
        if ((Instance && Instance != this) || !_mainCam)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
    }


    private void Update()
    {
        if (!Input.GetMouseButtonUp(0))
            return;
        
        OnMouseClick?.Invoke(_mainCam.ScreenToWorldPoint(Input.mousePosition));
    }
}