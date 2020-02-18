using System;
using System.Collections.Generic;

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
        private double[][] map;
        private int width, height;
        private double threshold = 0.0;

        public Elias() => path = new List<Coord>();

        public double[][] LightMap(double startx, double starty, double endx, double endy) {
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
            map = ArrayTools.Create<double>(width, height);
            runLine(startx, starty, endx, endy);
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
            map = ArrayTools.Create<double>(width, height);
            runLine(startx, starty, endx, endy);
            return path;
        }

        public List<Coord> Line(Coord start, Coord end) => Line(start.x, start.y, end.x, end.y);

        public List<Coord> Line(Coord start, Coord end, double brightnessThreshold) => Line(start.x, start.y, end.x, end.y, brightnessThreshold);

        public List<Coord> GetLastPath() => path;

        /**
         * Marks the location as having the visibility given.
         *
         * @param x
         * @param y
         * @param c
         */
        private void mark(double x, double y, double c) {
            //check bounds overflow from antialiasing
            if (x >= 0 && x < width && y >= 0 && y < height && c > threshold) {
                path.Add(Coord.Get((int)x, (int)y));
                map[(int)x][(int)y] = c;
            }
        }

        private double frac(double x) => x - Math.Truncate(x);

        private double invfrac(double x) => 1 - frac(x);

        private void runLine(double startx, double starty, double endx, double endy) {
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

            xgap = invfrac(x1 + .5);

            ix1 = (int)xend;
            iy1 = (int)yend;

            brightness1 = invfrac(yend) * xgap;
            brightness2 = frac(yend) * xgap;

            if (shallow) {
                mark(ix1, iy1, brightness1);
                mark(ix1, iy1 + 1, brightness2);
            } else {
                mark(iy1, ix1, brightness1);
                mark(iy1 + 1, ix1, brightness2);
            }

            yf = yend + grad;

            //add the second end point
            xend = Math.Truncate(x2 + .5);
            yend = y2 + grad * (xend - x2);

            xgap = invfrac(x2 - .5);

            ix2 = (int)xend;
            iy2 = (int)yend;

            //add the in-between points
            for (x = ix1 + 1; x < ix2; x++) {
                brightness1 = invfrac(yf);
                brightness2 = frac(yf);

                if (shallow) {
                    mark(x, (int)yf, brightness1);
                    mark(x, (int)yf + 1, brightness2);
                } else {
                    mark((int)yf, x, brightness1);
                    mark((int)yf + 1, x, brightness2);
                }

                yf += grad;
            }

            brightness1 = invfrac(yend) * xgap;
            brightness2 = frac(yend) * xgap;

            if (shallow) {
                mark(ix2, iy2, brightness1);
                mark(ix2, iy2 + 1, brightness2);
            } else {
                mark(iy2, ix2, brightness1);
                mark(iy2 + 1, ix2, brightness2);
            }

        }
    }

}
