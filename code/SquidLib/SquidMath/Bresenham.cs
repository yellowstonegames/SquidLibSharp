﻿using System;
using System.Collections.Generic;

namespace SquidLib.SquidMath {
    /**
     * Provides a means to generate Bresenham lines in 2D and 3D.
     *
     * @author Eben Howard - http://squidpony.com - howard@squidpony.com
     * @author Lewis Potter
     * @author Tommy Ettinger
     * @author smelC
     */
    public static class Bresenham {

        /**
         * Generates a 2D Bresenham line between two points.
         *
         * @param a the starting point
         * @param b the ending point
         * @return The path between {@code a} and {@code b}.
         */
        public static Coord[] Line2D(Coord a, Coord b) => Line2D(a.X, a.Y, b.X, b.Y);

        /**
         * Generates a 3D Bresenham line between two points.
         *
         * @param a Coord to start from. This will be the first element of the list
         * @param b Coord to end at. This will be the last element of the list.
         * @return A list of points between a and b.
         */
        // TODO - make this return Coord3D[]
        public static Queue<Coord3D> Line3D(Coord3D a, Coord3D b) => Line3D(a.X, a.Y, a.Z, b.X, b.Y, b.Z);

        /**
         * Generates a 3D Bresenham line between the given coordinates.
         * Uses a Coord3D for each point; keep in mind Coord3D values are not
         * pooled like ordinary 2D Coord values, and this may cause more work
         * for the garbage collector, especially on Android, if many Coord3D
         * values are produced.
         *
         * @param startx the x coordinate of the starting point
         * @param starty the y coordinate of the starting point
         * @param startz the z coordinate of the starting point
         * @param endx the x coordinate of the starting point
         * @param endy the y coordinate of the starting point
         * @param endz the z coordinate of the starting point
         * @return a Queue (internally, an Deque) of Coord3D points along the line
         */
        public static Queue<Coord3D> Line3D(int startx, int starty, int startz, int endx, int endy, int endz) {
            int dx = endx - startx;
            int dy = endy - starty;
            int dz = endz - startz;

            int ax = Math.Abs(dx);
            int ay = Math.Abs(dy);
            int az = Math.Abs(dz);
            Queue<Coord3D> result = new Queue<Coord3D>(Math.Max(Math.Max(ax, ay), az));

            ax <<= 1;
            ay <<= 1;
            az <<= 1;

            int signx = dx == 0 ? 0 : (dx >> 31 | 1); // signum with less converting to/from float
            int signy = dy == 0 ? 0 : (dy >> 31 | 1); // signum with less converting to/from float
            int signz = dz == 0 ? 0 : (dz >> 31 | 1); // signum with less converting to/from float

            int x = startx;
            int y = starty;
            int z = startz;

            int deltax, deltay, deltaz;
            if (ax >= Math.Max(ay, az)) /* x dominant */ {
                deltay = ay - (ax >> 1);
                deltaz = az - (ax >> 1);
                while (true) {
                    result.Enqueue(new Coord3D(x, y, z));
                    if (x == endx) {
                        return result;
                    }

                    if (deltay >= 0) {
                        y += signy;
                        deltay -= ax;
                    }

                    if (deltaz >= 0) {
                        z += signz;
                        deltaz -= ax;
                    }

                    x += signx;
                    deltay += ay;
                    deltaz += az;
                }
            } else if (ay >= Math.Max(ax, az)) /* y dominant */ {
                deltax = ax - (ay >> 1);
                deltaz = az - (ay >> 1);
                while (true) {
                    result.Enqueue(new Coord3D(x, y, z));
                    if (y == endy) {
                        return result;
                    }

                    if (deltax >= 0) {
                        x += signx;
                        deltax -= ay;
                    }

                    if (deltaz >= 0) {
                        z += signz;
                        deltaz -= ay;
                    }

                    y += signy;
                    deltax += ax;
                    deltaz += az;
                }
            } else {
                deltax = ax - (az >> 1);
                deltay = ay - (az >> 1);
                while (true) {
                    result.Enqueue(new Coord3D(x, y, z));
                    if (z == endz) {
                        return result;
                    }

                    if (deltax >= 0) {
                        x += signx;
                        deltax -= az;
                    }

                    if (deltay >= 0) {
                        y += signy;
                        deltay -= az;
                    }

                    z += signz;
                    deltax += ax;
                    deltay += ay;
                }
            }
        }

        /**
         * Generates a 2D Bresenham line between two points. Returns an array
         * of Coord instead of a Queue.
         * <br>
         * Uses ordinary Coord values for points, and these can be pooled
         * if they aren't beyond what the current pool allows (it starts,
         * by default, pooling Coords with x and y between -3 and 255,
         * inclusive). If the Coords are pool-able, it can significantly
         * reduce the work the garbage collector needs to do, especially
         * on Android.
         *
         * @param startx the x coordinate of the starting point
         * @param starty the y coordinate of the starting point
         * @param endx the x coordinate of the starting point
         * @param endy the y coordinate of the starting point
         * @return an array of Coord points along the line
         */
        public static Coord[] Line2D(int startx, int starty, int endx, int endy) =>
            Line2D(startx, starty, endx, endy, 0x7fffffff); // largest positive int for maxLength; it is extremely unlikely that this could be reached


        /**
         * Generates a 2D Bresenham line between two points, stopping early if
         * the number of Coords returned reaches maxLength.. Returns an array
         * of Coord instead of a Queue.
         * <br>
         * Uses ordinary Coord values for points, and these can be pooled
         * if they aren't beyond what the current pool allows (it starts,
         * by default, pooling Coords with x and y between -3 and 255,
         * inclusive). If the Coords are pool-able, it can significantly
         * reduce the work the garbage collector needs to do, especially
         * on Android.
         *
         * @param startx the x coordinate of the starting point
         * @param starty the y coordinate of the starting point
         * @param endx the x coordinate of the starting point
         * @param endy the y coordinate of the starting point
         * @param maxLength the largest count of Coord points this can return; will stop early if reached
         * @return an array of Coord points along the line
         */
        public static Coord[] Line2D(int startx, int starty, int endx, int endy, int maxLength) {
            int dx = endx - startx;
            int dy = endy - starty;

            int signx = dx == 0 ? 0 : (dx >> 31 | 1); // signum with less converting to/from float
            int signy = dy == 0 ? 0 : (dy >> 31 | 1); // signum with less converting to/from float

            int ax = (dx = Math.Abs(dx)) << 1;
            int ay = (dy = Math.Abs(dy)) << 1;

            int x = startx;
            int y = starty;

            int deltax, deltay;
            if (ax >= ay) /* x dominant */ {
                deltay = ay - (ax >> 1);
                Coord[] result = new Coord[Math.Min(maxLength, dx + 1)];
                for (int i = 0; i <= dx && i < maxLength; i++) {
                    result[i] = Coord.Get(x, y);

                    if (deltay >= 0) {
                        y += signy;
                        deltay -= ax;
                    }

                    x += signx;
                    deltay += ay;
                }
                return result;
            } else /* y dominant */ {
                deltax = ax - (ay >> 1);
                Coord[] result = new Coord[Math.Min(maxLength, dy + 1)];
                for (int i = 0; i <= dy && i < maxLength; i++) {
                    result[i] = Coord.Get(x, y);

                    if (deltax >= 0) {
                        x += signx;
                        deltax -= ay;
                    }


                    y += signy;
                    deltax += ax;
                }
                return result;
            }
        }

    }

}
