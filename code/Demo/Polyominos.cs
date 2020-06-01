using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Keys = Microsoft.Xna.Framework.Input.Keys;

using Console = SadConsole.Console;
using SadConsole.Components;
using SadConsole.Input;

using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace Demo {
    public class Polynomino {
        private static int width = 120,
            height = 40,
            millisecondDelay = 100;
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
                polynominoes = PolynominoBuilder.BuildPolynominoes(10);

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

    // modified from https://rosettacode.org/wiki/Free_polyominoes_enumeration
    class PolynominoBuilder {
        static int rank, rankSquared;
        static long[] anyRotationCount;
        static long[] nonFlippedCount;
        static long[] freePolynominoesCount;
        static int[] fieldCheck, fieldCheckR;
        static int fieldSize, fieldWidth;
        static int[] directions;
        static int[] rotationNeutral, rotationX, rotationY;
        static Dictionary<int, List<string>> polynominoes;

        /// <summary>
        /// Builds a string representation list of all polynominoes of the given rank. Only ranks up to 24 are supported.
        /// </summary>
        /// <param name="targetRank"></param>
        /// <returns></returns>
        public static Dictionary<int, List<string>> BuildPolynominoes(int targetRank) {
            rank = targetRank;
            if (rank < 1 || rank > 24) {
                throw new InvalidOperationException($"Rank input must be between 1 and 24, invalid input of {rank}.");
            }
            polynominoes = new Dictionary<int, List<string>>();
            for (int i = 1; i <= rank; i++) {
                polynominoes[i] = new List<string>();
            }

            CountEm();
            return polynominoes;
        }

        static void CountEm() {
            rankSquared = rank * rank;
            anyRotationCount = new long[rank + 1];
            nonFlippedCount = new long[rank + 1];
            freePolynominoesCount = new long[rank + 1];
            fieldWidth = rank * 2 - 2;
            fieldSize = (rank - 1) * (rank - 1) * 2 + 1;
            int[] pnField = new int[fieldSize];
            int[] pnPutList = new int[fieldSize];
            fieldCheck = new int[rankSquared];
            fieldCheckR = new int[rankSquared];
            directions = new int[] { 1, fieldWidth, -1, -fieldWidth };
            rotationNeutral = new int[] { 0, rank - 1, rankSquared - 1, rankSquared - rank, rank - 1, 0, rankSquared - rank, rankSquared - 1 };
            rotationX = new int[] { 1, rank, -1, -rank, -1, rank, 1, -rank };
            rotationY = new int[] { rank, -1, -rank, 1, rank, 1, -rank, -1 };
            Recurse(0, pnField, pnPutList, 0, 1);
        }

        static void Recurse(int level, int[] field, int[] putlist, int putno, int putlast) {
            CheckIt(field, level);
            if (rank == level) return;
            int pos;
            for (int i = putno; i < putlast; i++) {
                field[pos = putlist[i]] |= 1;
                int k = 0;
                foreach (int dir in directions) {
                    int pos2 = pos + dir;
                    if (0 <= pos2 && pos2 < fieldSize && (field[pos2] == 0)) {
                        field[pos2] = 2;
                        putlist[putlast + k++] = pos2;
                    }
                }
                Recurse(level + 1, field, putlist, i + 1, putlast + k);
                for (int j = 0; j < k; j++) field[putlist[putlast + j]] = 0;
                field[pos] = 2;
            }
            for (int i = putno; i < putlast; i++) field[putlist[i]] &= -2;
        }

        static void CheckIt(int[] field, int level) {
            anyRotationCount[level]++;
            for (int i = 0; i < rankSquared; i++) fieldCheck[i] = 0;
            int x, y;
            for (x = rank; x < fieldWidth; x++)
                for (y = 0; y + x < fieldSize; y += fieldWidth)
                    if ((field[x + y] & 1) == 1) goto bail;
                    bail:
            int x2 = rank - x, t;
            for (int i = 0; i < fieldSize; i++)
                if ((field[i] & 1) == 1) fieldCheck[((t = (i + rank - 2)) % fieldWidth) + x2 + (t / fieldWidth * rank)] = 1;
            int of1; for (of1 = 0; of1 < fieldCheck.Length && (fieldCheck[of1] == 0); of1++) ;
            bool c = true; int r;
            for (r = 1; r < 8 && c; r++) {
                for (x = 0; x < rank; x++) for (y = 0; y < rank; y++)
                        fieldCheckR[rotationNeutral[r] + rotationX[r] * x + rotationY[r] * y] = fieldCheck[x + y * rank];
                int of2; for (of2 = 0; of2 < fieldCheckR.Length && (fieldCheckR[of2] == 0); of2++) ;
                of2 -= of1;
                for (int i = of1; i < rankSquared - ((of2 > 0) ? of2 : 0); i++) {
                    if (fieldCheck[i] > fieldCheckR[i + of2]) break;
                    if (fieldCheck[i] < fieldCheckR[i + of2]) { c = false; break; }
                }
            }
            if (r > 4) nonFlippedCount[level]++;
            if (c) {
                if (level > 0) {
                    polynominoes[level].Add(ConvertToString(field.ToArray()));
                }
                freePolynominoesCount[level]++;
            }
        }

        static string ConvertToString(int[] field) // converts field into a minimal string
        {
            char[] res = new string(' ', rank * (fieldWidth + 1) - 1).ToCharArray();
            for (int i = fieldWidth; i < res.Length; i += fieldWidth + 1) res[i] = '\n';
            for (int i = 0, j = rank - 2; i < field.Length; i++, j++) {
                if ((field[i] & 1) == 1) res[j] = '#';
                if (j % (fieldWidth + 1) == fieldWidth) i--;
            }
            List<string> t = new string(res).Split('\n').ToList();
            int nn = 100, m = 0, v, k = 0; // trim down
            foreach (string s in t) {
                if ((v = s.IndexOf('#')) < nn) if (v >= 0) nn = v;
                if ((v = s.LastIndexOf('#')) > m) if (v < fieldWidth + 1) m = v;
                if (v < 0) break; k++;
            }
            m = m - nn + 1; // convert difference to length
            for (int i = t.Count - 1; i >= 0; i--) {
                if (i >= k) t.RemoveAt(i);
                else t[i] = t[i].Substring(nn, m);
            }
            return String.Join("\n", t.ToArray());
        }
    }
}
