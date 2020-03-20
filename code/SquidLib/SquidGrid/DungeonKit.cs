using System;
using System.Collections.Generic;
using System.Text;

namespace SquidLib.SquidGrid {
    public enum CellCategory : int {
        Untouched = 0,
        RoomFloor = 1,
        RoomWall = 2,
        CaveFloor = 3,
        CaveWall = 4,
        CorridorFloor = 5,
        CorridorWall = 6,
    }
}
