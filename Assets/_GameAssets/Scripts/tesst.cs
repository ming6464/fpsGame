using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tesst : MonoBehaviour
{
    public Vector3 vtPos;
    private CharacterController _characterController;
    private InputBase _inputBase;
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _inputBase = new InputBase();
        _inputBase.Enable();
        _inputBase.Character.Jump.performed += (context =>
        {
            _characterController.Move(vtPos);
        });
    }

    
}
