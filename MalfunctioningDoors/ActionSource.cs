using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalfunctioningDoors;

public static class ActionSource {
    public enum Source {
        UNKNOWN = -666,
        MALFUNCTION = -665,
        TOIL_HEAD = -5,
        LANDMINE = -4,
        TURRET = -3,
        SHOTGUN_ACCIDENT = -2,
        SHOTGUN_ENEMY = -1,

        [Tooltip("Player is actually anything above -1, but this is an enum, so...")]
        PLAYER = 0,
    }

    [Flags]
    public enum SelectableSource {
        TOIL_HEAD = -5,
        LANDMINE = -4,
        TURRET = -3,
        SHOTGUN_ACCIDENT = -2,
        SHOTGUN_ENEMY = -1,

        [Tooltip("Player is actually anything above -1, but this is an enum, so...")]
        PLAYER = 0,

        ALL = TOIL_HEAD | LANDMINE | TURRET | SHOTGUN_ACCIDENT | SHOTGUN_ENEMY | PLAYER,
    }

    public static Source? FromInt(this int source) {
        if (source >= 0) return Source.PLAYER;

        return (Source) source;
    }

    public static Source? FromSelectableSource(this SelectableSource selectableSource) {
        var selectableSourceValue = (int) selectableSource;

        return selectableSourceValue.FromInt();
    }

    public static int ToInt(this Source source) => (int) source;
}