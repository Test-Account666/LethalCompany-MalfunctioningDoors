
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
using System.Linq;
using MalfunctioningDoors.Malfunctions.Impl;

namespace MalfunctioningDoors.Malfunctions;

public static class MalfunctionGenerator {
    public static readonly MalfunctionDictionary MalfunctionDictionary = [
    ];

    public static Type GenerateMalfunctionalDoor(Random random) {
        var randomNumber = random.Next(1000, 9999);

        var shuffledMalfunctions = FisherYatesShuffle(MalfunctionDictionary, random).ToHashSet();

        while (randomNumber > 0)
            foreach (var shuffledMalfunction in shuffledMalfunctions) {
                randomNumber -= shuffledMalfunction.Value;
                if (randomNumber > 0)
                    continue;

                return shuffledMalfunction.Key;
            }

        MalfunctioningDoors.Logger.LogError("Couldn't find any malfunctional door type, falling back to CloseMalfunction as default!");

        return typeof(CloseMalfunction);
    }

    private static IEnumerable<KeyValuePair<TKey, TValue>> FisherYatesShuffle<TKey, TValue>(
        Dictionary<TKey, TValue> dictionary, Random random) {
        var list = dictionary.ToList();
        var size = list.Count;
        while (size > 1) {
            size--;
            var randomIndex = random.Next(size + 1);
            (list[randomIndex], list[size]) = (list[size], list[randomIndex]);
        }

        return list;
    }
}