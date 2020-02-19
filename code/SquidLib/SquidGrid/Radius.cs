using System;
using System.Collections.Generic;
using SquidLib.SquidMath;


namespace SquidLib.SquidGrid {
    /**
     * Basic radius strategy implementations likely to be used for roguelikes.
     */
    public enum Radius {

        /**
         * In an unobstructed area the FOV would be a square.
         *
         * This is the shape that would represent movement radius in an 8-way
         * movement scheme with no additional cost for diagonal movement.
         */
        Square,
        /**
         * In an unobstructed area the FOV would be a diamond.
         *
         * This is the shape that would represent movement radius in a 4-way
         * movement scheme.
         */
        Diamond,
        /**
         * In an unobstructed area the FOV would be a circle.
         *
         * This is the shape that would represent movement radius in an 8-way
         * movement scheme with all movement cost the same based on distance from
         * the source
         */
        Circle,
        /**
         * In an unobstructed area the FOV would be a cube.
         *
         * This is the shape that would represent movement radius in an 8-way
         * movement scheme with no additional cost for diagonal movement.
         */
        Cube,
        /**
         * In an unobstructed area the FOV would be a octahedron.
         *
         * This is the shape that would represent movement radius in a 4-way
         * movement scheme.
         */
        Octahedron,
        /**
         * In an unobstructed area the FOV would be a sphere.
         *
         * This is the shape that would represent movement radius in an 8-way
         * movement scheme with all movement cost the same based on distance from
         * the source
         */
        Sphere,
        /**
         * Like {@link #CIRCLE}, but always uses a rough approximation of distance instead of a more expensive (but more
         * accurate) Euclidean calculation.
         */
        RoughCircle
    }

    public static class RadiusExtensions {

        private const double PI2 = Math.PI * 2;
        public static double Radius(this Radius r, int startx, int starty, int startz, int endx, int endy, int endz) {
            return Radius(r, startx, starty, startz, endx, endy, (double)endz);
        }

        public static double Radius(this Radius r, double startx, double starty, double startz, double endx, double endy, double endz) {
            double dx = Math.Abs(startx - endx);
            double dy = Math.Abs(starty - endy);
            double dz = Math.Abs(startz - endz);
            return Radius(r, dx, dy, dz);
        }

        public static double Radius(this Radius r, int dx, int dy, int dz) {
            return Radius(r, (double)dx, dy, dz);
        }

        public static double Radius(this Radius r, double dx, double dy, double dz) {
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);
            dz = Math.Abs(dz);
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube:
                    return Math.Max(dx, Math.Max(dy, dz));//radius is longest axial distance
                case SquidGrid.Radius.Circle:
                case SquidGrid.Radius.Sphere:
                    return Math.Sqrt(dx * dx + dy * dy + dz * dz);//standard spherical radius
                case SquidGrid.Radius.RoughCircle: // ignores z
                    if (dx == dy) return 1.5 * dx;
                    else if (dx < dy) return 1.5 * dx + (dy - dx);
                    else return 1.5 * dy + (dx - dy);
                default:
                    return dx + dy + dz;//radius is the manhattan distance
            }
        }

        public static double Radius(this Radius r, int startx, int starty, int endx, int endy) {
            return Radius(r, (double)startx, starty, endx, endy);
        }
        public static double Radius(this Radius r, Coord start, Coord end) {
            return Radius(r, start.X, start.Y, end.X, end.Y);
        }
        public static double Radius(this Radius r, Coord end) {
            return Radius(0.0, 0.0, end.X, end.Y);
        }

        public static double Radius(this Radius r, double startx, double starty, double endx, double endy) {
            double dx = startx - endx;
            double dy = starty - endy;
            return Radius(r, dx, dy);
        }

        public static double Radius(this Radius r, int dx, int dy) {
            return Radius(r, dx, (double)dy);
        }

        public static double Radius(this Radius r, double dx, double dy) {
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube:
                    return Math.Max(dx, dy);//radius is longest axial distance
                case SquidGrid.Radius.RoughCircle: // radius is an approximation, roughly octagonal
                    if (dx == dy) return 1.5 * dx;
                    else if (dx < dy) return 1.5 * dx + (dy - dx);
                    else return 1.5 * dy + (dx - dy);
                case SquidGrid.Radius.Circle:
                case SquidGrid.Radius.Sphere:
                    return Math.Sqrt(dx * dx + dy * dy);//standard spherical radius
                default:
                    return dx + dy;//radius is the manhattan distance
            }
        }

        public static Coord OnUnitShape(this Radius r, double distance, IRNG rng) {
            if (rng is null) {
                throw new ArgumentNullException($"rng");
            }
            int x = 0, y = 0;
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube:
                    x = rng.NextInt((int)-distance, (int)distance + 1);
                    y = rng.NextInt((int)-distance, (int)distance + 1);
                    break;
                case SquidGrid.Radius.Diamond:
                case SquidGrid.Radius.Octahedron:
                    x = rng.NextInt((int)-distance, (int)distance + 1);
                    y = rng.NextInt((int)-distance, (int)distance + 1);
                    if (Radius(r, x, y) > distance) {
                        if (x > 0) {
                            if (y > 0) {
                                x = (int)(distance - x);
                                y = (int)(distance - y);
                            } else {
                                x = (int)(distance - x);
                                y = (int)(-distance - y);
                            }
                        } else {
                            if (y > 0) {
                                x = (int)(-distance - x);
                                y = (int)(distance - y);
                            } else {
                                x = (int)(-distance - x);
                                y = (int)(-distance - y);
                            }
                        }
                    }
                    break;
                default: // includes CIRCLE, SPHERE, and ROUGH_CIRCLE
                    double result = distance * Math.Sqrt(rng.NextDouble());
                    double theta = rng.NextDouble(0, PI2);
                    x = Convert.ToInt32(Math.Cos(theta) * result);
                    y = Convert.ToInt32(Math.Sin(theta) * result);
                    break;
            }

            return Coord.Get(x, y);
        }

        public static Coord3D OnUnitShape3D(this Radius r, double distance, IRNG rng) {
            if (rng is null) {
                throw new ArgumentNullException($"rng");
            }
            int x = 0, y = 0, z = 0;
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Diamond:
                case SquidGrid.Radius.Circle:
                case SquidGrid.Radius.RoughCircle:
                    Coord p = OnUnitShape(r, distance, rng);
                    return new Coord3D(p.X, p.Y, 0);//2D strategies
                case SquidGrid.Radius.Cube:
                    x = rng.NextInt((int)-distance, (int)distance + 1);
                    y = rng.NextInt((int)-distance, (int)distance + 1);
                    z = rng.NextInt((int)-distance, (int)distance + 1);
                    break;
                case SquidGrid.Radius.Octahedron:
                case SquidGrid.Radius.Sphere:
                    do {
                        x = rng.NextInt((int)-distance, (int)distance + 1);
                        y = rng.NextInt((int)-distance, (int)distance + 1);
                        z = rng.NextInt((int)-distance, (int)distance + 1);
                    } while (Radius(r, x, y, z) > distance);
                    break;
            }

            return new Coord3D(x, y, z);
        }
        public static double Volume2D(this Radius r, double radiusLength) {
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube:
                    return (radiusLength * 2 + 1) * (radiusLength * 2 + 1);
                case SquidGrid.Radius.Diamond:
                case SquidGrid.Radius.Octahedron:
                    return radiusLength * (radiusLength + 1) * 2 + 1;
                default:
                    return Math.PI * radiusLength * radiusLength + 1;
            }
        }
        public static double Volume3D(this Radius r, double radiusLength) {
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube:
                    return (radiusLength * 2 + 1) * (radiusLength * 2 + 1) * (radiusLength * 2 + 1);
                case SquidGrid.Radius.Diamond:
                case SquidGrid.Radius.Octahedron:
                    double total = radiusLength * (radiusLength + 1) * 2 + 1;
                    for (double i = radiusLength - 1; i >= 0; i--) {
                        total += (i * (i + 1) * 2 + 1) * 2;
                    }
                    return total;
                default:
                    return Math.PI * radiusLength * radiusLength * radiusLength * 4.0 / 3.0 + 1;
            }
        }

        private static int Clamp(int n, int min, int max) => Math.Min(Math.Max(min, n), max - 1);

        public static IndexedSet<Coord> Perimeter(this Radius r, Coord center, int radiusLength, bool surpassEdges, int width, int height) {
            IndexedSet<Coord> rim = new IndexedSet<Coord>();
            if (!surpassEdges && (center.X < 0 || center.X >= width || center.Y < 0 || center.Y > height))
                return rim;
            if (radiusLength < 1) {
                rim.Add(center);
                return rim;
            }
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube: {
                    for (int i = center.X - radiusLength; i <= center.X + radiusLength; i++) {
                        int x = i;
                        if (!surpassEdges) x = Clamp(i, 0, width);
                        rim.Add(Coord.Get(x, Clamp(center.Y - radiusLength, 0, height)));
                        rim.Add(Coord.Get(x, Clamp(center.Y + radiusLength, 0, height)));
                    }
                    for (int j = center.Y - radiusLength; j <= center.Y + radiusLength; j++) {
                        int y = j;
                        if (!surpassEdges) y = Clamp(j, 0, height);
                        rim.Add(Coord.Get(Clamp(center.X - radiusLength, 0, height), y));
                        rim.Add(Coord.Get(Clamp(center.X + radiusLength, 0, height), y));
                    }
                }
                break;
                case SquidGrid.Radius.Diamond:
                case SquidGrid.Radius.Octahedron: {
                    int xUp = center.X + radiusLength, xDown = center.X - radiusLength,
                            yUp = center.Y + radiusLength, yDown = center.Y - radiusLength;
                    if (!surpassEdges) {
                        xDown = Clamp(xDown, 0, width);
                        xUp = Clamp(xUp, 0, width);
                        yDown = Clamp(yDown, 0, height);
                        yUp = Clamp(yUp, 0, height);
                    }

                    rim.Add(Coord.Get(xDown, center.Y));
                    rim.Add(Coord.Get(xUp, center.Y));
                    rim.Add(Coord.Get(center.X, yDown));
                    rim.Add(Coord.Get(center.X, yUp));

                    for (int i = xDown + 1, c = 1; i < center.X; i++, c++) {
                        int x = i;
                        if (!surpassEdges) x = Clamp(i, 0, width);
                        rim.Add(Coord.Get(x, Clamp(center.Y - c, 0, height)));
                        rim.Add(Coord.Get(x, Clamp(center.Y + c, 0, height)));
                    }
                    for (int i = center.X + 1, c = 1; i < center.X + radiusLength; i++, c++) {
                        int x = i;
                        if (!surpassEdges) x = Clamp(i, 0, width);
                        rim.Add(Coord.Get(x, Clamp(center.Y + radiusLength - c, 0, height)));
                        rim.Add(Coord.Get(x, Clamp(center.Y - radiusLength + c, 0, height)));
                    }
                }
                break;
                default: {
                    double theta;
                    int x, y, denom = 1;
                    bool anySuccesses;
                    while (denom <= 256) {
                        anySuccesses = false;
                        for (int i = 1; i <= denom; i += 2) {
                            theta = i * (PI2 / denom);
                            x = (int)(Math.Cos(theta) * (radiusLength + 0.25)) + center.X;
                            y = (int)(Math.Sin(theta) * (radiusLength + 0.25)) + center.Y;

                            if (!surpassEdges) {
                                x = Clamp(x, 0, width);
                                y = Clamp(y, 0, height);
                            }
                            Coord p = Coord.Get(x, y);
                            bool test = !rim.Contains(p);

                            rim.Add(p);
                            anySuccesses = test || anySuccesses;
                        }
                        if (!anySuccesses)
                            break;
                        denom *= 2;
                    }
                    break;

                }
            }
            return rim;
        }
        public static Coord Extend(this Radius r, Coord center, Coord middle, int radiusLength, bool surpassEdges, int width, int height) {
            if (!surpassEdges && (center.X < 0 || center.X >= width || center.Y < 0 || center.Y > height ||
                    middle.X < 0 || middle.X >= width || middle.Y < 0 || middle.Y > height))
                return Coord.Get(0, 0);
            if (radiusLength < 1) {
                return center;
            }
            double theta = Math.Atan2(middle.Y - center.Y, middle.X - center.X),
                    cosTheta = Math.Cos(theta), sinTheta = Math.Sin(theta);

            Coord end = Coord.Get(middle.X, middle.Y);
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube:
                case SquidGrid.Radius.Diamond:
                case SquidGrid.Radius.Octahedron: {
                    int rad2 = 0;
                    if (surpassEdges) {
                        while (Radius(r, center.X, center.Y, end.X, end.Y) < radiusLength) {
                            rad2++;
                            end = Coord.Get(Convert.ToInt32(cosTheta * rad2) + center.X
                                    , Convert.ToInt32(sinTheta * rad2) + center.Y);
                        }
                    } else {
                        while (Radius(r, center.X, center.Y, end.X, end.Y) < radiusLength) {
                            rad2++;
                            end = Coord.Get(Clamp(Convert.ToInt32(cosTheta * rad2) + center.X, 0, width)
                                          , Clamp(Convert.ToInt32(sinTheta * rad2) + center.Y, 0, height));
                            if (end.X == 0 || end.X == width - 1 || end.Y == 0 || end.Y == height - 1)
                                return end;
                        }
                    }

                    return end;
                }
                default: {
                    end = Coord.Get(Clamp(Convert.ToInt32(cosTheta * radiusLength) + center.X, 0, width)
                            , Clamp(Convert.ToInt32(sinTheta * radiusLength) + center.Y, 0, height));
                    if (!surpassEdges) {
                        long edgeLength = 0;
                        //                    if (end.x == 0 || end.x == width - 1 || end.y == 0 || end.y == height - 1)
                        if (end.X < 0) {
                            // wow, we lucked out here. the only situation where cos(angle) is 0 is if the angle aims
                            // straight up or down, and then x cannot be < 0 or >= width.
                            edgeLength = Convert.ToInt32((0 - center.X) / cosTheta);
                            end = end.ChangeY(Clamp(Convert.ToInt32(sinTheta * edgeLength) + center.Y, 0, height));
                        } else if (end.X >= width) {
                            // wow, we lucked out here. the only situation where cos(angle) is 0 is if the angle aims
                            // straight up or down, and then x cannot be < 0 or >= width.
                            edgeLength = Convert.ToInt32((width - 1 - center.X) / cosTheta);
                            end = end.ChangeY(Clamp(Convert.ToInt32(sinTheta * edgeLength) + center.Y, 0, height));
                        }

                        if (end.Y < 0) {
                            // wow, we lucked out here. the only situation where sin(angle) is 0 is if the angle aims
                            // straight left or right, and then y cannot be < 0 or >= height.
                            edgeLength = Convert.ToInt32((0 - center.Y) / sinTheta);
                            end = end.ChangeX(Clamp(Convert.ToInt32(cosTheta * edgeLength) + center.X, 0, width));
                        } else if (end.Y >= height) {
                            // wow, we lucked out here. the only situation where sin(angle) is 0 is if the angle aims
                            // straight left or right, and then y cannot be < 0 or >= height.
                            edgeLength = Convert.ToInt32((height - 1 - center.Y) / sinTheta);
                            end = end.ChangeX(Clamp(Convert.ToInt32(cosTheta * edgeLength) + center.X, 0, width));
                        }
                    }
                    return end;
                }
            }
        }

        /**
         * Compares two Radius enums as if they are both in a 2D plane; that is, Radius.SPHERE is treated as equal to
         * Radius.CIRCLE, Radius.CUBE is equal to Radius.SQUARE, and Radius.OCTAHEDRON is equal to Radius.DIAMOND.
         * @param other the Radius to compare this to
         * @return true if the 2D versions of both Radius enums are the same shape.
         */
        public static bool Equals2D(this Radius r, Radius other) {
            switch (r) {
                case SquidGrid.Radius.RoughCircle:
                    return other == SquidGrid.Radius.RoughCircle;
                case SquidGrid.Radius.Circle:
                case SquidGrid.Radius.Sphere:
                    return other == SquidGrid.Radius.Circle || other == SquidGrid.Radius.Sphere;
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube:
                    return other == SquidGrid.Radius.Square || other == SquidGrid.Radius.Cube;
                default:
                    return other == SquidGrid.Radius.Diamond || other == SquidGrid.Radius.Octahedron;
            }
        }
        public static bool InRange(this Radius r, int startx, int starty, int endx, int endy, int minRange, int maxRange) {
            double dist = Radius(r, startx, starty, endx, endy);
            return dist >= minRange - 0.001 && dist <= maxRange + 0.001;
        }

        public static int RoughDistance(this Radius r, int xPos, int yPos) {
            int x = Math.Abs(xPos), y = Math.Abs(yPos);
            switch (r) {
                case SquidGrid.Radius.Circle:
                case SquidGrid.Radius.Sphere:
                case SquidGrid.Radius.RoughCircle: {
                    if (x == y) return 3 * x;
                    else if (x < y) return 3 * x + 2 * (y - x);
                    else return 3 * y + 2 * (x - y);
                }
                case SquidGrid.Radius.Diamond:
                case SquidGrid.Radius.Octahedron:
                    return 2 * (x + y);
                default:
                    return 2 * Math.Max(x, y);
            }
        }

        public static List<Coord> PointsInside(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height) {
            return PointsInside(r, centerX, centerY, radiusLength, surpassEdges, width, height, null);
        }
        public static List<Coord> PointsInside(this Radius r, Coord center, int radiusLength, bool surpassEdges, int width, int height) {
            return PointsInside(r, center.X, center.Y, radiusLength, surpassEdges, width, height, null);
        }

        public static List<Coord> PointsInside(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height, List<Coord> buf) {
            List<Coord> contents = buf ?? new List<Coord>((int)Math.Ceiling(Volume2D(r, radiusLength)));
            if (!surpassEdges && (centerX < 0 || centerX >= width || centerY < 0 || centerY >= height))
                return contents;
            if (radiusLength < 1) {
                contents.Add(Coord.Get(centerX, centerY));
                return contents;
            }
            switch (r) {
                case SquidGrid.Radius.Square:
                case SquidGrid.Radius.Cube: {
                    for (int i = centerX - radiusLength; i <= centerX + radiusLength; i++) {
                        for (int j = centerY - radiusLength; j <= centerY + radiusLength; j++) {
                            if (!surpassEdges && (i < 0 || j < 0 || i >= width || j >= height))
                                continue;
                            contents.Add(Coord.Get(i, j));
                        }
                    }
                }
                break;
                case SquidGrid.Radius.Diamond:
                case SquidGrid.Radius.Octahedron: {
                    for (int i = centerX - radiusLength; i <= centerX + radiusLength; i++) {
                        for (int j = centerY - radiusLength; j <= centerY + radiusLength; j++) {
                            if ((Math.Abs(centerX - i) + Math.Abs(centerY - j) > radiusLength) ||
                                    (!surpassEdges && (i < 0 || j < 0 || i >= width || j >= height)))
                                continue;
                            contents.Add(Coord.Get(i, j));
                        }
                    }
                }
                break;
                default: {
                    float high, changedX;
                    int rndX, rndY;
                    for (int dx = -radiusLength; dx <= radiusLength; ++dx) {
                        changedX = dx - 0.25f * Math.Sign(dx);
                        rndX = Convert.ToInt32(changedX);
                        high = (float)Math.Sqrt(radiusLength * radiusLength - changedX * changedX);
                        if (surpassEdges || !(centerX + rndX < 0 ||
                                centerX + rndX >= width))
                            contents.Add(Coord.Get(centerX + rndX, centerY));
                        for (float dy = high; dy >= 0.75f; --dy) {
                            rndY = Convert.ToInt32(dy - 0.25f);
                            if (surpassEdges || !(centerX + rndX < 0 || centerY + rndY < 0 ||
                                    centerX + rndX >= width || centerY + rndY >= height))
                                contents.Add(Coord.Get(centerX + rndX, centerY + rndY));
                            if (surpassEdges || !(centerX + rndX < 0 || centerY - rndY < 0 ||
                                    centerX + rndX >= width || centerY - rndY >= height))
                                contents.Add(Coord.Get(centerX + rndX, centerY - rndY));
                        }
                    }
                }
                break;
            }
            return contents;
        }

        /**
         * Gets a List of all Coord points within {@code radiusLength} of {@code center} using Chebyshev measurement (making
         * a square). Appends Coords to {@code buf} if it is non-null, and returns either buf or a freshly-allocated List of
         * Coord. If {@code surpassEdges} is false, which is the normal usage, this will not produce Coords with x or y less
         * than 0 or greater than {@code width} or {@code height}; if surpassEdges is true, then it can produce any Coords
         * in the actual radius.
         * @param centerX the center Coord x
         * @param centerY the center Coord x
         * @param radiusLength the inclusive distance from (centerX,centerY) for Coords to use in the List
         * @param surpassEdges usually should be false; if true, can produce Coords with negative x/y or past width/height
         * @param width the width of the area this can place Coords (exclusive, not relative to center, usually map width)
         * @param height the height of the area this can place Coords (exclusive, not relative to center, usually map height)
         * @return a new List containing the points within radiusLength of the center

         */
        public static List<Coord> InSquare(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height) {
            return SquidGrid.Radius.Square.PointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, null);
        }
        /**
         * Gets a List of all Coord points within {@code radiusLength} of {@code center} using Manhattan measurement (making
         * a diamond). Appends Coords to {@code buf} if it is non-null, and returns either buf or a freshly-allocated List
         * of Coord. If {@code surpassEdges} is false, which is the normal usage, this will not produce Coords with x or y
         * less than 0 or greater than {@code width} or {@code height}; if surpassEdges is true, then it can produce any
         * Coords in the actual radius.
         * @param centerX the center Coord x
         * @param centerY the center Coord x
         * @param radiusLength the inclusive distance from (centerX,centerY) for Coords to use in the List
         * @param surpassEdges usually should be false; if true, can produce Coords with negative x/y or past width/height
         * @param width the width of the area this can place Coords (exclusive, not relative to center, usually map width)
         * @param height the height of the area this can place Coords (exclusive, not relative to center, usually map height)
         * @return a new List containing the points within radiusLength of the center
         */
        public static List<Coord> InDiamond(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height) {
            return SquidGrid.Radius.Diamond.PointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, null);
        }
        /**
         * Gets a List of all Coord points within {@code radiusLength} of {@code center} using Euclidean measurement (making
         * a circle). Appends Coords to {@code buf} if it is non-null, and returns either buf or a freshly-allocated List of
         * Coord. If {@code surpassEdges} is false, which is the normal usage, this will not produce Coords with x or y less
         * than 0 or greater than {@code width} or {@code height}; if surpassEdges is true, then it can produce any Coords
         * in the actual radius.
         * @param centerX the center Coord x
         * @param centerY the center Coord x
         * @param radiusLength the inclusive distance from (centerX,centerY) for Coords to use in the List
         * @param surpassEdges usually should be false; if true, can produce Coords with negative x/y or past width/height
         * @param width the width of the area this can place Coords (exclusive, not relative to center, usually map width)
         * @param height the height of the area this can place Coords (exclusive, not relative to center, usually map height)
         * @return a new List containing the points within radiusLength of the center
         */
        public static List<Coord> InCircle(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height) {
            return SquidGrid.Radius.Circle.PointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, null);
        }

        /**
         * Gets a List of all Coord points within {@code radiusLength} of {@code center} using Chebyshev measurement (making
         * a square). Appends Coords to {@code buf} if it is non-null, and returns either buf or a freshly-allocated List of
         * Coord. If {@code surpassEdges} is false, which is the normal usage, this will not produce Coords with x or y less
         * than 0 or greater than {@code width} or {@code height}; if surpassEdges is true, then it can produce any Coords
         * in the actual radius.
         * @param centerX the center Coord x
         * @param centerY the center Coord x
         * @param radiusLength the inclusive distance from (centerX,centerY) for Coords to use in the List
         * @param surpassEdges usually should be false; if true, can produce Coords with negative x/y or past width/height
         * @param width the width of the area this can place Coords (exclusive, not relative to center, usually map width)
         * @param height the height of the area this can place Coords (exclusive, not relative to center, usually map height)
         * @param buf the List of Coord to append points to; may be null to create a new List
         * @return buf, after appending Coords to it, or a new List if buf was null
         */
        public static List<Coord> InSquare(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height, List<Coord> buf) {
            return SquidGrid.Radius.Square.PointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, buf);
        }
        /**
         * Gets a List of all Coord points within {@code radiusLength} of {@code center} using Manhattan measurement (making
         * a diamond). Appends Coords to {@code buf} if it is non-null, and returns either buf or a freshly-allocated List
         * of Coord. If {@code surpassEdges} is false, which is the normal usage, this will not produce Coords with x or y
         * less than 0 or greater than {@code width} or {@code height}; if surpassEdges is true, then it can produce any
         * Coords in the actual radius.
         * @param centerX the center Coord x
         * @param centerY the center Coord x
         * @param radiusLength the inclusive distance from (centerX,centerY) for Coords to use in the List
         * @param surpassEdges usually should be false; if true, can produce Coords with negative x/y or past width/height
         * @param width the width of the area this can place Coords (exclusive, not relative to center, usually map width)
         * @param height the height of the area this can place Coords (exclusive, not relative to center, usually map height)
         * @param buf the List of Coord to append points to; may be null to create a new List
         * @return buf, after appending Coords to it, or a new List if buf was null
         */
        public static List<Coord> InDiamond(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height, List<Coord> buf) {
            return SquidGrid.Radius.Diamond.PointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, buf);
        }
        /**
         * Gets a List of all Coord points within {@code radiusLength} of {@code center} using Euclidean measurement (making
         * a circle). Appends Coords to {@code buf} if it is non-null, and returns either buf or a freshly-allocated List of
         * Coord. If {@code surpassEdges} is false, which is the normal usage, this will not produce Coords with x or y less
         * than 0 or greater than {@code width} or {@code height}; if surpassEdges is true, then it can produce any Coords
         * in the actual radius.
         * @param centerX the center Coord x
         * @param centerY the center Coord x
         * @param radiusLength the inclusive distance from (centerX,centerY) for Coords to use in the List
         * @param surpassEdges usually should be false; if true, can produce Coords with negative x/y or past width/height
         * @param width the width of the area this can place Coords (exclusive, not relative to center, usually map width)
         * @param height the height of the area this can place Coords (exclusive, not relative to center, usually map height)
         * @param buf the List of Coord to append points to; may be null to create a new List
         * @return buf, after appending Coords to it, or a new List if buf was null
         */
        public static List<Coord> InCircle(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height, List<Coord> buf) {
            return SquidGrid.Radius.Circle.PointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, buf);
        }

        /**
         * Given an Iterable of Coord (such as a List or Set), a distance to expand outward by (using this Radius), and the
         * bounding height and width of the map, gets a "thickened" group of Coord as a Set where each Coord in points has
         * been expanded out by an amount no greater than distance. As an example, you could call this on a line generated
         * by Bresenham, OrthoLine, or an LOS object's getLastPath() method, and expand the line into a thick "brush stroke"
         * where this Radius affects the shape of the ends. This will never produce a Coord with negative x or y, a Coord
         * with x greater than or equal to width, or a Coord with y greater than or equal to height.
         * @param distance the distance, as measured by this Radius, to expand each Coord on points up to
         * @param width the bounding width of the map (exclusive)
         * @param height the bounding height of the map (exclusive)
         * @param points an Iterable (such as a List or Set) of Coord that this will make a "thickened" version of
         * @return a Set of Coord that covers a wider area than what points covers; each Coord will be unique (it's a Set)
         */
        public static IndexedSet<Coord> Expand(this Radius r, int distance, int width, int height, IEnumerable<Coord> points) {
            if (points is null) {
                return null;
            }
            List<Coord> around = PointsInside(r, Coord.Get(distance, distance), distance, false, width, height);
            IndexedSet<Coord> expanded = new IndexedSet<Coord>();
            int tx, ty;
            foreach (Coord pt in points) {
                foreach (Coord ar in around) {
                    tx = pt.X + ar.X - distance;
                    ty = pt.Y + ar.Y - distance;
                    if (tx >= 0 && tx < width && ty >= 0 && ty < height)
                        expanded.Add(Coord.Get(tx, ty));
                }
            }
            return expanded;
        }
    }

}
