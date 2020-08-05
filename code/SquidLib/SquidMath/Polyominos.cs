using System;
using System.Collections.Generic;
using System.Linq;

namespace SquidLib.SquidMath {

    // modified from https://rosettacode.org/wiki/Free_polyominoes_enumeration
    public class PolynominoBuilder {
        private int rank, rankSquared;
        private long[] anyRotationCount;
        private long[] nonFlippedCount;
        private long[] freePolynominoesCount;
        private int[] fieldCheck, fieldCheckR;
        private int fieldSize, fieldWidth;
        private int[] directions;
        private int[] rotationNeutral, rotationX, rotationY;
        private Dictionary<int, List<string>> polynominoes;

        /// <summary>
        /// Builds a string representation list of all polynominoes up to and including the given rank. Only ranks up to 24 are supported.
        /// 
        /// Higher ranks can take considerable time to generate.
        /// </summary>
        /// <param name="targetRank"></param>
        /// <returns></returns>
        public Dictionary<int, List<string>> Build(int targetRank) {
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

        /// <summary>
        /// Returns the previously generated set of polyominoes without regenerating them.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<string>> LastBuilt() => polynominoes;

        private void CountEm() {
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

        private void Recurse(int level, int[] field, int[] putlist, int putno, int putlast) {
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

        private void CheckIt(int[] field, int level) {
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

        private string ConvertToString(int[] field) { // converts field into a minimal string
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
