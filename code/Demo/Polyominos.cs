using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Console = SadConsole.Console;

using SquidLib.SquidMath;

namespace Demo {
    public class Polynomino {
        private static int width = 120,
            height = 40,
            millisecondDelay = 10;
        private RNG rng = new RNG();

        private Dictionary<int, List<string>> polynominoes = default;
        private Color foreground = default;
        private Console console = default;
        private DateTime start = default;

        private int currentRank = 1;
        private int currentPolyIndex = 0;
        private int workingX = 1, workingY = 1, maxY = 1;

        public static void Main() => _ = new Polynomino();

        private Polynomino() {
            SadConsole.Game.Create(width, height);
            SadConsole.Game.OnInitialize = () => {
                PolynominoBuilder polyBuilder = new PolynominoBuilder();
                polynominoes = polyBuilder.Build(10);

                console = new Console(width, height);
                console.IsFocused = true;
                console.Cursor.IsEnabled = false;

                SadConsole.Global.CurrentScreen = console;

                start = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(millisecondDelay));
            };

            SadConsole.Game.OnUpdate = (gameTime) => {
                if (start.AddMilliseconds(millisecondDelay) < DateTime.UtcNow) {
                    currentPolyIndex++;
                    if (currentPolyIndex >= polynominoes[currentRank].Count) {
                        currentRank++;
                        if (currentRank > polynominoes.Count) {
                            currentRank = 1;
                            console.Clear();
                            workingX = 1;
                            workingY = 1;
                        }
                        currentPolyIndex = 0;
                    }

                    foreground = Color.White.GetRandomColor(rng);

                    string polyString = polynominoes[currentRank][currentPolyIndex];
                    string[] outputs = polyString.Split('\n');

                    // figure out if new row start is needed
                    int spaceLeft = width - 2 - workingX;
                    int outputsLength = outputs.Max(s => s.Length);
                    if (spaceLeft < outputsLength) {
                        workingX = 1;
                        workingY = maxY + 1;
                    }

                    // See if a new wipe needs to happen
                    if (workingY + outputs.Length >= height - 1) {
                        console.Clear();
                        workingX = 1;
                        workingY = 1;
                        maxY = 1;
                    }

                    maxY = Math.Max(maxY, workingY + outputs.Length);

                    int y = workingY;
                    foreach (string line in outputs) {
                        console.Print(workingX, y, line, foreground);
                        y++;
                    }
                    workingX += outputsLength + 1;

                    start = DateTime.UtcNow;
                }
            };

            SadConsole.Game.Instance.Run();
            SadConsole.Game.Instance.Dispose();
        }
    }
}
