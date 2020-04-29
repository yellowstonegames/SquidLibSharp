using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using BearLib;
using ColorHelper;
using SquidLib;
using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace RogueDelivery {
    class RogueDelivery {
        private enum ControlType { OnFoot, DrivingWagon }

        private bool keepRunning = true;
        private RNG rng;

        private int windowWidth = 120,
            windowHeight = 40,
            logHeight = 3,
            statusHeight = 1;
        private int width, height; // for the map area
        private string[] log = new string[] {
            "Welcome to Rogue Delivery!",
            "Arrow keys to move, space to jump in and out of wagon.",
            "Hold shift when moving to slide instead of turn."
        };

        private Dictionary<Coord, char> borders = new Dictionary<Coord, char>();

        private ControlType controlType = ControlType.OnFoot;
        private Mob player;
        private Physical playerPhysical;
        private BigMob wagon;

        private List<BigMob> bigMobs = new List<BigMob>();
        private List<Mob> littleMobs = new List<Mob>();

        static void Main() {
            var rd = new RogueDelivery(); // start up the primary system
            rd.Start();
        }

        private void InitObjects() {
            player = new Mob { Rep = new Representation { Glyph = '@', Color = Color.Aquamarine } };
            playerPhysical = new Physical() {
                Name = "Rogue",
                Class = "Delivery Driver",
                Health = 23,
                Strength = 4,
                Luck = 12,
                Wiles = 2,
                XP = 0
            };

            wagon = new BigMob(Prototype.Wagon);
            bigMobs.Add(wagon);

            for (int i = 0; i < 20; i++) {
                bigMobs.Add(new BigMob(Prototype.Log) {
                    Location = Coord.Get(rng.NextInt(width), rng.NextInt(height)),
                    Facing = rng.RandomElement(DirectionTools.Cardinals)
                });
            }
        }

        private RogueDelivery() {
            rng = new RNG(DateTime.Today.ToString(CultureInfo.InvariantCulture)); // Different seed every day, but the same seed on that day, for testing.

            width = windowWidth - 2;
            height = windowHeight - logHeight - statusHeight - 4;

            InitObjects();

            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    byte blue = BlueNoise.GetSeeded(x, y, 1234567u);
                    if (blue < 7) {
                        littleMobs.Add(new Mob(Prototype.Pebble) { Location = Coord.Get(x,y) });
                    } else if (blue < 30) {
                        littleMobs.Add(new Mob(Prototype.Bush) { Location = Coord.Get(x, y) });
                    } else {
                        littleMobs.Add(new Mob(Prototype.Grass) { Location = Coord.Get(x, y) });
                    }
                }
            }

            // TODO - check that space is clear enough for them to get placed
            wagon.Location = Coord.Get(width / 2, height / 2);
            player.Location = Coord.Get(wagon.Location.X - 4, wagon.Location.Y); ;

            // init display style
            for (int x = 1; x < windowWidth - 1; x++) {
                borders[Coord.Get(x, 0)] = '─';
                borders[Coord.Get(x, windowHeight - 1)] = '─';
                borders[Coord.Get(x, windowHeight - 1 - statusHeight - 1)] = '─';
                borders[Coord.Get(x, windowHeight - 1 - statusHeight - 1 - logHeight - 1)] = '─';
            }
            for (int y = 1; y < windowHeight - 1; y++) {
                borders[Coord.Get(0, y)] = '│';
                borders[Coord.Get(windowWidth - 1, y)] = '│';
            }
            borders[Coord.Get(0, 0)] = '┌';
            borders[Coord.Get(windowWidth - 1, 0)] = '┐';
            borders[Coord.Get(0, windowHeight - 1)] = '└';
            borders[Coord.Get(windowWidth - 1, windowHeight - 1)] = '┘';
            borders[Coord.Get(0, windowHeight - 1 - statusHeight - 1)] = '├';
            borders[Coord.Get(0, windowHeight - 1 - statusHeight - 1 - logHeight - 1)] = '├';
            borders[Coord.Get(windowWidth - 1, windowHeight - 1 - statusHeight - 1)] = '┤';
            borders[Coord.Get(windowWidth - 1, windowHeight - 1 - statusHeight - 1 - logHeight - 1)] = '┤';

            // Setup terminal
            Terminal.Open();
            Terminal.Set(
                $"window: title='Rogue Delivery', size={windowWidth}x{windowHeight};" +
                $"output: vsync=true;" +
                $"font: Iosevka.ttf, size=9x21, hinting=autohint"
                );
            BltColor.LoadAurora();
            Terminal.Color(rng.RandomElement<string>(BltColor.AuroraNames));
            Terminal.Refresh();
        }

        private void Start() {
            DrawMap();
            Terminal.Refresh();

            while (keepRunning) {
                if (Terminal.HasInput()) {
                    int read = Terminal.Read();
                    switch (read) {
                        case Terminal.TK_ESCAPE:
                        case Terminal.TK_CLOSE:
                            keepRunning = false;
                            break;
                        case Terminal.TK_LEFT:
                            MovePlayer(Direction.Left);
                            break;
                        case Terminal.TK_UP:
                            MovePlayer(Direction.Up);
                            break;
                        case Terminal.TK_DOWN:
                            MovePlayer(Direction.Down);
                            break;
                        case Terminal.TK_RIGHT:
                            MovePlayer(Direction.Right);
                            break;
                        case Terminal.TK_SPACE when controlType == ControlType.DrivingWagon:
                            controlType = ControlType.OnFoot;
                            bigMobs.Add(wagon);
                            break;
                        case Terminal.TK_SPACE when controlType == ControlType.OnFoot:
                            controlType = ControlType.DrivingWagon;
                            bigMobs.Remove(wagon);
                            break;
                        default:
                            // ignore unknown commands
                            Terminal.Color(Color.White);
                            Message($"Pressed {(char)Terminal.State(Terminal.TK_WCHAR)}");
                            break;
                    }
                    DrawMap();
                    Terminal.Refresh();
                }
            }
            Terminal.Close();
        }

        private void MovePlayer(Direction direction) {
            switch (controlType) {
                case ControlType.DrivingWagon:
                    if (wagon.Facing == direction || Terminal.Check(Terminal.TK_SHIFT)) {
                        if (IsBlocked(wagon, wagon.Facing, direction)) {
                            //
                        } else {
                            wagon.Location += direction.Coord();
                        }
                    } else {
                        if (IsBlocked(wagon, direction, Direction.None)) {
                            //
                        } else {
                            wagon.Facing = direction;
                        }
                    }
                    break;
                case ControlType.OnFoot:
                    if (IsBlocked(player, player.Location + direction.Coord())) {
                        //
                    } else {
                        player.Location += direction.Coord();
                    }
                    break;
            }
        }

        private bool IsBlocked(BigMob mover, Coord coord) => IsBlocked(mover, Direction.Up, coord);

        private bool IsBlocked(BigMob mover, Direction facing, Direction direction) =>
            IsBlocked(mover, facing, mover.Location + direction.Coord());

        private bool IsBlocked(BigMob mover, Direction facing, Coord coord) =>
            IsBlocked(mover.Reps[facing].Tiles.Keys.Select(c => coord - mover.Location + c));

        private bool IsBlocked(IEnumerable<Coord> targetCoords) =>
            targetCoords.Intersect(
                littleMobs
                .Where(lm => lm.Blocking)
                .Select(lm => lm.Location)
                .Concat(bigMobs
                    .Where(bm => bm.Blocking)
                    .SelectMany(bm => bm.Reps[bm.Facing].Tiles.Keys
                        .Select(c => c + bm.DrawingOffset()))))
                .Any();

        private void DrawMap() {
            Terminal.ClearArea(1, 1, width, height);

            foreach (Mob mob in littleMobs) {
                PutOnMap(mob);
            }

            foreach (BigMob bigMob in bigMobs) {
                PutOnMap(bigMob);
            }

            PutOnMap(wagon); // need to make sure it prints over other things except player
            switch (controlType) {
                case ControlType.OnFoot:
                    PutOnMap(player);
                    break;
                case ControlType.DrivingWagon:
                    player.Location = wagon.Location;
                    PutOnMap(player);
                    break;
            }

            Terminal.Color(rng.RandomElement(BltColor.AuroraNames));
            PaintBorder();

            // Status section
            int startY = windowHeight - statusHeight - 1;
            Terminal.Color(player.Rep.Color);
            int startX = Put(2, startY, playerPhysical.Name);
            Terminal.Color(Color.White);
            startX = Put(startX + 1, startY, "The");
            Terminal.Color(Color.LightCoral);
            startX = Put(startX + 1, startY, playerPhysical.Class);
            Terminal.Color(Color.White);
            startX = Put(startX + 1, startY, "--");
            Terminal.Color(Color.Silver);
            startX = Put(startX + 1, startY, "Strength: " + playerPhysical.Strength);
            Terminal.Color(Color.HotPink);
            startX = Put(startX + 2, startY, "Luck: " + playerPhysical.Luck);
            Terminal.Color(Color.MediumPurple);
            startX = Put(startX + 2, startY, "Wiles: " + playerPhysical.Wiles);
            Terminal.Color(Color.RosyBrown);
            startX = Put(startX + 2, startY, "XP: " + playerPhysical.XP);
            Terminal.Color(Color.SeaGreen);
            startX = Put(startX + 2, startY, "Health: " + playerPhysical.Health);
        }

        private void PaintBorder() {
            foreach (var kvp in borders) {
                Put(kvp.Key, kvp.Value);
            }
        }

        private void Message(string message) {
            for (int i = 1; i < log.Length; i++) {
                log[i - 1] = log[i];
            }
            log[log.Length] = message;
            Terminal.ClearArea(1, height + 3, width, logHeight);
            for (int i = 0; i < log.Length; i++) {
                Terminal.Print(1, height + 3 + i, log[i]);
            }
        }

        private static void Put(Coord coord, char c) => Terminal.Put(coord.X, coord.Y, c);

        private static int Put(int x, int y, string s) {
            Terminal.Print(x, y, s);
            return x + s.Length;
        }

        private void PutOnMap(Mob mob) {
            Terminal.Color(mob.Rep.Color);
            PutOnMap(mob.Location, mob.Rep.Glyph);
        }

        private void PutOnMap(BigMob bigMob) {
            Coord drawingOffset = bigMob.DrawingOffset();
            foreach (var (coord, rep) in bigMob.Reps[bigMob.Facing].Tiles) {
                Terminal.Color(rep.Color);
                PutOnMap(drawingOffset + coord, rep.Glyph);
            }
        }

        /// <summary>
        /// Puts the character on the map if the coordinate is valid, otherwise takes no action.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="c"></param>
        private void PutOnMap(Coord coord, char c) => PutOnMap(coord.X, coord.Y, c);

        /// <summary>
        /// Puts the character on the map if the coordinate is valid, otherwise takes no action.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="c"></param>
        private void PutOnMap(int x, int y, char c) {
            if (InMapBounds(x, y)) {
                Terminal.Put(x + 1, y + 1, c); // handle window offset
            }
        }

        private bool InMapBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

        private bool InMapBounds(Coord coord) => InMapBounds(coord.X, coord.Y);

    }
}
