
/*
    A Lethal Company Mod
    Copyright (C) 2024  TestAccount666 (Entity303 / Test-Account666)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


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