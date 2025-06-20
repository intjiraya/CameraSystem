﻿using System.ComponentModel;
using UnityEngine;

namespace CameraSystem.Models;
public class WorkstationConfig
{
    [Description("The position where the workstation should spawn")]
    public Vector3 Position { get; set; }

    [Description("The rotation of the workstation")]
    public Vector3 Rotation { get; set; }

    [Description("The scale of the workstation")]
    public Vector3 Scale { get; set; }

    public WorkstationConfig()
    {
    }

    public WorkstationConfig(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}
