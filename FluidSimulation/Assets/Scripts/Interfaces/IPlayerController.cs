using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerController
{
    event Action<bool, float> GroundedChanged;
    event Action Jumped;
    Vector2 FrameInput { get; }
}