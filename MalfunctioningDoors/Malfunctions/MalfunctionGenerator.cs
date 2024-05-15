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