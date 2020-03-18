using System;
using System.Collections.Generic;
using System.Text;
using SquidLib.SquidMath;

namespace SquidLib.SquidGrid {
    public enum DungeonRoom {
        Cave = 0,
        BoxRoom = 1,
        RoundRoom = 2,
        WalledBoxRoom = 3,
        WalledRoundRoom = 4
    }
    public class WanderingRoomGenerator {
        public Grid<char> Dungeon { get; set; }
        public Grid<CellCategory> Environment { get; set; }
        public RNG Random { get; set; }
        private ProbabilityTable<DungeonRoom> RoomTable;

    }
}
