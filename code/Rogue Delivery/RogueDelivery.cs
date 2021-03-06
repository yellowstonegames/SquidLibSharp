﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Keys = Microsoft.Xna.Framework.Input.Keys;

using Console = SadConsole.Console;
using SadConsole.Components;
using SadConsole.Input;

using SquidLib.SquidGrid;
using SquidLib.SquidMath;
using SquidLib.SquidMisc;

namespace RogueDelivery {
    class RogueDelivery {
        private enum ControlType { OnFoot, DrivingWagon }

        private RNG rng;
        private Console console;

        private int windowWidth = 120,
            windowHeight = 40,
            logHeight = 3,
            statusHeight = 1;
        private int width, height; // for the map area
        private string[] log;

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

        private RogueDelivery() {
            rng = new RNG(DateTime.Today.ToString(CultureInfo.InvariantCulture)); // Different seed every day, but the same seed on that day, for testing.
            width = windowWidth - 2;
            height = windowHeight - logHeight - statusHeight - 4;
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

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    byte blue = BlueNoise.GetSeeded(x, y, 1234567u);
                    if (blue < 7) {
                        littleMobs.Add(new Mob(Prototype.Pebble) { Location = Coord.Get(x, y) });
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
        }

        private void InitDisplay() {
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

            log = new string[logHeight];
            for (int i = 0; i < log.Length; i++) {
                log[i] = "";
            }

            // Setup terminal
            SadConsole.Game.Create(windowWidth, windowHeight);
            SadConsole.Game.OnInitialize = () => {
                console = new Console(windowWidth, windowHeight);
                console.IsFocused = true;
                console.Cursor.IsEnabled = false;
                console.Components.Add(new SadKeyboardController(this));

                SadConsole.Global.CurrentScreen = console;
                DrawMap();

                Message("Welcome to Rogue Delivery!");
                Message("Arrow keys to move, space to jump in and out of wagon.");
                Message("Hold shift when moving to slide instead of turn.");
            };
        }

        private void Start() {
            InitObjects();
            InitDisplay();
            SadConsole.Game.Instance.Run();
            SadConsole.Game.Instance.Dispose();
        }

        private class SadKeyboardController : KeyboardConsoleComponent {
            private RogueDelivery master;
            public SadKeyboardController(RogueDelivery master) => this.master = master;

            // TODO - queue multiple "simultaneous" inputs registered
            public override void ProcessKeyboard(Console console, Keyboard info, out bool handled) {
                handled = false;

                // check for movements
                if (info.IsKeyPressed(Keys.Left)) {
                    master.MovePlayer(Direction.Left, info.IsKeyDown(Keys.LeftShift) || info.IsKeyDown(Keys.RightShift));
                    handled = true;
                } else if (info.IsKeyPressed(Keys.Right)) {
                    master.MovePlayer(Direction.Right, info.IsKeyDown(Keys.LeftShift) || info.IsKeyDown(Keys.RightShift));
                    handled = true;
                } else if (info.IsKeyPressed(Keys.Up)) {
                    master.MovePlayer(Direction.Up, info.IsKeyDown(Keys.LeftShift) || info.IsKeyDown(Keys.RightShift));
                    handled = true;
                } else if (info.IsKeyPressed(Keys.Down)) {
                    master.MovePlayer(Direction.Down, info.IsKeyDown(Keys.LeftShift) || info.IsKeyDown(Keys.RightShift));
                    handled = true;
                } else if (info.IsKeyPressed(Keys.Space)) {
                    switch (master.controlType) {
                        case ControlType.DrivingWagon:
                            master.controlType = ControlType.OnFoot;
                            master.bigMobs.Add(master.wagon);
                            break;
                        case ControlType.OnFoot:
                            master.controlType = ControlType.DrivingWagon;
                            master.bigMobs.Remove(master.wagon);
                            break;
                    }
                    handled = true;
                }

                if (handled) {
                    master.DrawMap();
                }
            }
        }

        private void MovePlayer(Direction direction, bool strafe = false) {
            switch (controlType) {
                case ControlType.DrivingWagon:
                    if (wagon.Facing == direction || strafe) {
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

        private void ClearArea(int x, int y, int clearWidth, int clearHeight) => console.Clear(new Rectangle(x, y, clearWidth, clearHeight));

        private void DrawMap() {
            ClearArea(1, 1, width, height);

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

            console.DefaultForeground = GetRandomColor();
            PaintBorder();

            // Status section
            int startY = windowHeight - statusHeight - 1;
            console.DefaultForeground = player.Rep.Color;
            int startX = Put(2, startY, playerPhysical.Name);
            console.DefaultForeground = Color.White;
            startX = Put(startX + 1, startY, "The");
            console.DefaultForeground = Color.LightCoral;
            startX = Put(startX + 1, startY, playerPhysical.Class);
            console.DefaultForeground = Color.White;
            startX = Put(startX + 1, startY, "--");
            console.DefaultForeground = Color.Silver;
            startX = Put(startX + 1, startY, "Strength: " + playerPhysical.Strength);
            console.DefaultForeground = Color.HotPink;
            startX = Put(startX + 2, startY, "Luck: " + playerPhysical.Luck);
            console.DefaultForeground = Color.MediumPurple;
            startX = Put(startX + 2, startY, "Wiles: " + playerPhysical.Wiles);
            console.DefaultForeground = Color.RosyBrown;
            startX = Put(startX + 2, startY, "XP: " + playerPhysical.XP);
            console.DefaultForeground = Color.SeaGreen;
            startX = Put(startX + 2, startY, "Health: " + playerPhysical.Health);
        }

        private Color GetRandomColor() => Color.White.GetRandomColor(rng);

        private void PaintBorder() {
            foreach (var kvp in borders) {
                Put(kvp.Key, kvp.Value);
            }
        }

        private void Message(string message) {
            for (int i = 1; i < log.Length; i++) {
                log[i - 1] = log[i];
            }
            log[log.Length - 1] = message;
            ClearArea(1, height + 2, width, logHeight);
            for (int i = 0; i < log.Length; i++) {
                Put(1, height + 2 + i, log[i]);
            }
        }

        private void Put(int x, int y, char c) {
            byte[] converting = BitConverter.GetBytes(c);
            byte cp437char = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(437), converting)[0]; // target only has a byte size character
            console.SetGlyph(x, y, cp437char, console.DefaultForeground);
        }

        private void Put(Coord coord, char c) => Put(coord.X, coord.Y, c);

        private int Put(int x, int y, string s) {
            console.Print(x, y, s, console.DefaultForeground);
            return x + s.Length;
        }

        private void PutOnMap(Mob mob) {
            console.DefaultForeground = mob.Rep.Color;
            PutOnMap(mob.Location, mob.Rep.Glyph);
        }

        private void PutOnMap(BigMob bigMob) {
            Coord drawingOffset = bigMob.DrawingOffset();
            foreach ((var coord, var rep) in bigMob.Reps[bigMob.Facing].Tiles) {
                console.DefaultForeground = rep.Color;
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
                Put(x + 1, y + 1, c); // handle window offset
            }
        }

        private bool InMapBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

        private bool InMapBounds(Coord coord) => InMapBounds(coord.X, coord.Y);

    }
}
