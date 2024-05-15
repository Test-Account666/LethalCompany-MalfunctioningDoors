using System;
using System.Collections.Generic;

namespace MalfunctioningDoors.Malfunctions;

public class MalfunctionDictionary : Dictionary<Type, int> {
    public new void Add(Type key, int value) {
        if (!key.IsSubclassOf(typeof(MalfunctionalDoor)))
            throw new InvalidOperationException("MalfunctionDictionary can only contain sub-types of " +
                                                typeof(MalfunctionalDoor).FullName + "!");

        base.Add(key, value);
    }
}