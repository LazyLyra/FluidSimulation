using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
[RequireComponent(typeof(PhysicsSystemPlayerController))]
public class PhysicsSystemPlayerInputs : MonoBehaviour
{
    public PhysicsSystemPlayerController controlScript;


    // Start is called before the first frame update
    void Start()
    {
        controlScript = GetComponent<PhysicsSystemPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
