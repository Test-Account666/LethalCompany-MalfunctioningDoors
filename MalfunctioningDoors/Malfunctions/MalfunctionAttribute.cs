using System;

namespace MalfunctioningDoors.Malfunctions;

[AttributeUsage(AttributeTargets.Class)]
public class MalfunctionAttribute(int weight = 1) : Attribute {
    internal readonly int weight = weight;
}