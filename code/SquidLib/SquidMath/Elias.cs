using System;
using System.Collections.Generic;
using SquidLib.SquidGrid;

namespace SquidLib.SquidMath {

    /**
     * Contains methods to draw anti-aliased lines based on floating-point
     * coordinates.
     * <br>
     * Because of the way this line is calculated, endpoints may be swapped and
     * therefore the list may not be in start-to-end order.
     * <br>
     * Based on work by Hugo Elias at
     * http://freespace.virgin.net/hugo.elias/graphics/x_wuline.htm which is in turn
     * based on work by Wu.
     * @author Eben Howard - http://squidpony.com - howard@squidpony.com
     */
    public class Elias {
        private List<Coord> path;
        private Grid<double> map;
        private int width, height;
        private double threshold = 0.0;

        public Elias() => path = new List<Coord>();

        public Grid<double> LightMap(double startx, double starty, double endx, double endy) {
            Line(startx, starty, endx, endy);
            return map;
        }

        /**
         * Gets the line between the two points.
         *
         * @param startx
         * @param starty
         * @param endx
         * @param endy
         * @return
         */
        public List<Coord> Line(double startx, double starty, double endx, double endy) {
            path.Clear();
            width = (int)(Math.Max(startx, endx) + 1);
            height = (int)(Math.Max(starty, endy) + 1);
            map = new Grid<double>(width, height);
            RunLine(startx, starty, endx, endy);
            return path;
        }
        /**
         * Gets the line between the two points.
         *
         * @param startx
         * @param starty
         * @param endx
         * @param endy
         * @param brightnessThreshold between 0.0 (default) and 1.0; only Points with higher brightness will be included
         * @return
         */
        public List<Coord> Line(double startx, double starty, double endx, double endy, double brightnessThreshold) {
            threshold = brightnessThreshold;
            path.Clear();
            width = (int)(Math.Max(startx, endx) + 1);
            height = (int)(Math.Max(starty, endy) + 1);
            map = new Grid<double>(width, height);
            RunLine(startx, starty, endx, endy);
            return path;
        }

        public List<Coord> Line(Coord start, Coord end) => Line(start.X, start.Y, end.X, end.Y);

        public List<Coord> Line(Coord start, Coord end, double brightnessThreshold) => Line(start.X, start.Y, end.X, end.Y, brightnessThreshold);

        public List<Coord> GetLastPath() => path;

        /**
         * Marks the location as having the visibility given.
         *
         * @param x
         * @param y
         * @param c
         */
        private void Mark(int x, int y, double c) {
            //check bounds overflow from antialiasing
            if (c > threshold && map.TrySet(x, y, c))
                path.Add(Coord.Get(x, y));
        }

        private static double Frac(double x) => x - Math.Truncate(x);

        private static double Invfrac(double x) => 1 - Frac(x);

        private void RunLine(double startx, double starty, double endx, double endy) {
            double x1 = startx, y1 = starty, x2 = endx, y2 = endy;
            double grad, xd, yd, xgap, xend, yend, yf, brightness1, brightness2;
            int x, ix1, ix2, iy1, iy2;
            bool shallow = false;

            xd = x2 - x1;
            yd = y2 - y1;

            if (Math.Abs(xd) > Math.Abs(yd)) {
                shallow = true;
            }

            if (!shallow) {
                double temp = x1;
                x1 = y1;
                y1 = temp;
                temp = x2;
                x2 = y2;
                y2 = temp;
                xd = x2 - x1;
                yd = y2 - y1;
            }
            if (x1 > x2) {
                double temp = x1;
                x1 = x2;
                x2 = temp;
                temp = y1;
                y1 = y2;
                y2 = temp;
                xd = x2 - x1;
                yd = y2 - y1;
            }

            grad = yd / xd;

            //add the first end point
            xend = Math.Truncate(x1 + .5);
            yend = y1 + grad * (xend - x1);

            xgap = Invfrac(x1 + .5);

            ix1 = (int)xend;
            iy1 = (int)yend;

            brightness1 = Invfrac(yend) * xgap;
            brightness2 = Frac(yend) * xgap;

            if (shallow) {
                Mark(ix1, iy1, brightness1);
                Mark(ix1, iy1 + 1, brightness2);
            } else {
                Mark(iy1, ix1, brightness1);
                Mark(iy1 + 1, ix1, brightness2);
            }

            yf = yend + grad;

            //add the second end point
            xend = Math.Truncate(x2 + .5);
            yend = y2 + grad * (xend - x2);

            xgap = Invfrac(x2 - .5);

            ix2 = (int)xend;
            iy2 = (int)yend;

            //add the in-between points
            for (x = ix1 + 1; x < ix2; x++) {
                brightness1 = Invfrac(yf);
                brightness2 = Frac(yf);

                if (shallow) {
                    Mark(x, (int)yf, brightness1);
                    Mark(x, (int)yf + 1, brightness2);
                } else {
                    Mark((int)yf, x, brightness1);
                    Mark((int)yf + 1, x, brightness2);
                }

                yf += grad;
            }

            brightness1 = Invfrac(yend) * xgap;
            brightness2 = Frac(yend) * xgap;

            if (shallow) {
                Mark(ix2, iy2, brightness1);
                Mark(ix2, iy2 + 1, brightness2);
            } else {
                Mark(iy2, ix2, brightness1);
                Mark(iy2 + 1, ix2, brightness2);
            }

        }
    }

}
