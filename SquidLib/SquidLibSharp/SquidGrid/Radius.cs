using System;
using System.Collections.Generic;
using SquidLib.SquidMath;


namespace SquidLib.SquidGrid
{
    /**
     * Basic radius strategy implementations likely to be used for roguelikes.
     */
    public enum Radius
    {

        /**
         * In an unobstructed area the FOV would be a square.
         *
         * This is the shape that would represent movement radius in an 8-way
         * movement scheme with no additional cost for diagonal movement.
         */
        SQUARE,
        /**
         * In an unobstructed area the FOV would be a diamond.
         *
         * This is the shape that would represent movement radius in a 4-way
         * movement scheme.
         */
        DIAMOND,
        /**
         * In an unobstructed area the FOV would be a circle.
         *
         * This is the shape that would represent movement radius in an 8-way
         * movement scheme with all movement cost the same based on distance from
         * the source
         */
        CIRCLE,
        /**
         * In an unobstructed area the FOV would be a cube.
         *
         * This is the shape that would represent movement radius in an 8-way
         * movement scheme with no additional cost for diagonal movement.
         */
        CUBE,
        /**
         * In an unobstructed area the FOV would be a octahedron.
         *
         * This is the shape that would represent movement radius in a 4-way
         * movement scheme.
         */
        OCTAHEDRON,
        /**
         * In an unobstructed area the FOV would be a sphere.
         *
         * This is the shape that would represent movement radius in an 8-way
         * movement scheme with all movement cost the same based on distance from
         * the source
         */
        SPHERE,
        /**
         * Like {@link #CIRCLE}, but always uses a rough approximation of distance instead of a more expensive (but more
         * accurate) Euclidean calculation.
         */
        ROUGH_CIRCLE
    }

    public static class RadiusExtensions
    {

        private const double PI2 = Math.PI * 2;
        public static double radius(this Radius r, int startx, int starty, int startz, int endx, int endy, int endz)
        {
            return radius(r, (double)startx, (double)starty, (double)startz, (double)endx, (double)endy, (double)endz);
        }

        public static double radius(this Radius r, double startx, double starty, double startz, double endx, double endy, double endz)
        {
            double dx = Math.Abs(startx - endx);
            double dy = Math.Abs(starty - endy);
            double dz = Math.Abs(startz - endz);
            return radius(r, dx, dy, dz);
        }

        public static double radius(this Radius r, int dx, int dy, int dz)
        {
            return radius(r, (float)dx, (float)dy, (float)dz);
        }

        public static double radius(this Radius r, double dx, double dy, double dz)
        {
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);
            dz = Math.Abs(dz);
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.CUBE:
                    return Math.Max(dx, Math.Max(dy, dz));//radius is longest axial distance
                case Radius.CIRCLE:
                case Radius.SPHERE:
                    return Math.Sqrt(dx * dx + dy * dy + dz * dz);//standard spherical radius
                case Radius.ROUGH_CIRCLE: // ignores z
                    if (dx == dy) return 1.5 * dx;
                    else if (dx < dy) return 1.5 * dx + (dy - dx);
                    else return 1.5 * dy + (dx - dy);
                default:
                    return dx + dy + dz;//radius is the manhattan distance
            }
        }

        public static double radius(this Radius r, int startx, int starty, int endx, int endy)
        {
            return radius(r, (double)startx, (double)starty, (double)endx, (double)endy);
        }
        public static double radius(this Radius r, Coord start, Coord end)
        {
            return radius(r, (double)start.x, (double)start.y, (double)end.x, (double)end.y);
        }
        public static double radius(this Radius r, Coord end)
        {
            return radius(0.0, 0.0, (double)end.x, (double)end.y);
        }

        public static double radius(this Radius r, double startx, double starty, double endx, double endy)
        {
            double dx = startx - endx;
            double dy = starty - endy;
            return radius(r, dx, dy);
        }

        public static double radius(this Radius r, int dx, int dy)
        {
            return radius(r, (double)dx, (double)dy);
        }

        public static double radius(this Radius r, double dx, double dy)
        {
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.CUBE:
                    return Math.Max(dx, dy);//radius is longest axial distance
                case Radius.ROUGH_CIRCLE: // radius is an approximation, roughly octagonal
                    if (dx == dy) return 1.5 * dx;
                    else if (dx < dy) return 1.5 * dx + (dy - dx);
                    else return 1.5 * dy + (dx - dy);
                case Radius.CIRCLE:
                case Radius.SPHERE:
                    return Math.Sqrt(dx * dx + dy * dy);//standard spherical radius
                default:
                    return dx + dy;//radius is the manhattan distance
            }
        }

        public static Coord onUnitShape(this Radius r, double distance, IRNG rng)
        {
            int x = 0, y = 0;
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.CUBE:
                    x = rng.between((int)-distance, (int)distance + 1);
                    y = rng.between((int)-distance, (int)distance + 1);
                    break;
                case Radius.DIAMOND:
                case Radius.OCTAHEDRON:
                    x = rng.between((int)-distance, (int)distance + 1);
                    y = rng.between((int)-distance, (int)distance + 1);
                    if (radius(r, x, y) > distance)
                    {
                        if (x > 0)
                        {
                            if (y > 0)
                            {
                                x = (int)(distance - x);
                                y = (int)(distance - y);
                            }
                            else
                            {
                                x = (int)(distance - x);
                                y = (int)(-distance - y);
                            }
                        }
                        else
                        {
                            if (y > 0)
                            {
                                x = (int)(-distance - x);
                                y = (int)(distance - y);
                            }
                            else
                            {
                                x = (int)(-distance - x);
                                y = (int)(-distance - y);
                            }
                        }
                    }
                    break;
                default: // includes CIRCLE, SPHERE, and ROUGH_CIRCLE
                    double result = distance * Math.Sqrt(rng.nextDouble());
                    double theta = rng.between(0, PI2);
                    x = Convert.ToInt32(Math.Cos(theta) * result);
                    y = Convert.ToInt32(Math.Sin(theta) * result);
                    break;
            }

            return Coord.get(x, y);
        }

        public static Coord3D onUnitShape3D(this Radius r, double distance, IRNG rng)
        {
            int x = 0, y = 0, z = 0;
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.DIAMOND:
                case Radius.CIRCLE:
                case Radius.ROUGH_CIRCLE:
                    Coord p = onUnitShape(r, distance, rng);
                    return new Coord3D(p.x, p.y, 0);//2D strategies
                case Radius.CUBE:
                    x = rng.between((int)-distance, (int)distance + 1);
                    y = rng.between((int)-distance, (int)distance + 1);
                    z = rng.between((int)-distance, (int)distance + 1);
                    break;
                case Radius.OCTAHEDRON:
                case Radius.SPHERE:
                    do
                    {
                        x = rng.between((int)-distance, (int)distance + 1);
                        y = rng.between((int)-distance, (int)distance + 1);
                        z = rng.between((int)-distance, (int)distance + 1);
                    } while (radius(r, x, y, z) > distance);
                    break;
            }

            return new Coord3D(x, y, z);
        }
        public static double volume2D(this Radius r, double radiusLength)
        {
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.CUBE:
                    return (radiusLength * 2 + 1) * (radiusLength * 2 + 1);
                case Radius.DIAMOND:
                case Radius.OCTAHEDRON:
                    return radiusLength * (radiusLength + 1) * 2 + 1;
                default:
                    return Math.PI * radiusLength * radiusLength + 1;
            }
        }
        public static double volume3D(this Radius r, double radiusLength)
        {
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.CUBE:
                    return (radiusLength * 2 + 1) * (radiusLength * 2 + 1) * (radiusLength * 2 + 1);
                case Radius.DIAMOND:
                case Radius.OCTAHEDRON:
                    double total = radiusLength * (radiusLength + 1) * 2 + 1;
                    for (double i = radiusLength - 1; i >= 0; i--)
                    {
                        total += (i * (i + 1) * 2 + 1) * 2;
                    }
                    return total;
                default:
                    return Math.PI * radiusLength * radiusLength * radiusLength * 4.0 / 3.0 + 1;
            }
        }




        private static int clamp(int n, int min, int max)
        {
            return Math.Min(Math.Max(min, n), max - 1);
        }

        public static OrderedSet<Coord> perimeter(this Radius r, Coord center, int radiusLength, bool surpassEdges, int width, int height)
        {
            OrderedSet<Coord> rim = new OrderedSet<Coord>(4 * radiusLength);
            if (!surpassEdges && (center.x < 0 || center.x >= width || center.y < 0 || center.y > height))
                return rim;
            if (radiusLength < 1)
            {
                rim.Add(center);
                return rim;
            }
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.CUBE:
                {
                    for (int i = center.x - radiusLength; i <= center.x + radiusLength; i++)
                    {
                        int x = i;
                        if (!surpassEdges) x = clamp(i, 0, width);
                        rim.Add(Coord.get(x, clamp(center.y - radiusLength, 0, height)));
                        rim.Add(Coord.get(x, clamp(center.y + radiusLength, 0, height)));
                    }
                    for (int j = center.y - radiusLength; j <= center.y + radiusLength; j++)
                    {
                        int y = j;
                        if (!surpassEdges) y = clamp(j, 0, height);
                        rim.Add(Coord.get(clamp(center.x - radiusLength, 0, height), y));
                        rim.Add(Coord.get(clamp(center.x + radiusLength, 0, height), y));
                    }
                }
                break;
                case Radius.DIAMOND:
                case Radius.OCTAHEDRON:
                {
                    int xUp = center.x + radiusLength, xDown = center.x - radiusLength,
                            yUp = center.y + radiusLength, yDown = center.y - radiusLength;
                    if (!surpassEdges)
                    {
                        xDown = clamp(xDown, 0, width);
                        xUp = clamp(xUp, 0, width);
                        yDown = clamp(yDown, 0, height);
                        yUp = clamp(yUp, 0, height);
                    }

                    rim.Add(Coord.get(xDown, center.y));
                    rim.Add(Coord.get(xUp, center.y));
                    rim.Add(Coord.get(center.x, yDown));
                    rim.Add(Coord.get(center.x, yUp));

                    for (int i = xDown + 1, c = 1; i < center.x; i++, c++)
                    {
                        int x = i;
                        if (!surpassEdges) x = clamp(i, 0, width);
                        rim.Add(Coord.get(x, clamp(center.y - c, 0, height)));
                        rim.Add(Coord.get(x, clamp(center.y + c, 0, height)));
                    }
                    for (int i = center.x + 1, c = 1; i < center.x + radiusLength; i++, c++)
                    {
                        int x = i;
                        if (!surpassEdges) x = clamp(i, 0, width);
                        rim.Add(Coord.get(x, clamp(center.y + radiusLength - c, 0, height)));
                        rim.Add(Coord.get(x, clamp(center.y - radiusLength + c, 0, height)));
                    }
                }
                break;
                default:
                {
                    double theta;
                    int x, y, denom = 1;
                    bool anySuccesses;
                    while (denom <= 256)
                    {
                        anySuccesses = false;
                        for (int i = 1; i <= denom; i += 2)
                        {
                            theta = i * (PI2 / denom);
                            x = (int)(Math.Cos(theta) * (radiusLength + 0.25)) + center.x;
                            y = (int)(Math.Sin(theta) * (radiusLength + 0.25)) + center.y;

                            if (!surpassEdges)
                            {
                                x = clamp(x, 0, width);
                                y = clamp(y, 0, height);
                            }
                            Coord p = Coord.get(x, y);
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
        public static Coord extend(this Radius r, Coord center, Coord middle, int radiusLength, bool surpassEdges, int width, int height)
        {
            if (!surpassEdges && (center.x < 0 || center.x >= width || center.y < 0 || center.y > height ||
                    middle.x < 0 || middle.x >= width || middle.y < 0 || middle.y > height))
                return Coord.get(0, 0);
            if (radiusLength < 1)
            {
                return center;
            }
            double theta = Math.Atan2(middle.y - center.y, middle.x - center.x),
                    cosTheta = Math.Cos(theta), sinTheta = Math.Sin(theta);

            Coord end = Coord.get(middle.x, middle.y);
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.CUBE:
                case Radius.DIAMOND:
                case Radius.OCTAHEDRON:
                {
                    int rad2 = 0;
                    if (surpassEdges)
                    {
                        while (radius(r, center.x, center.y, end.x, end.y) < radiusLength)
                        {
                            rad2++;
                            end = Coord.get(Convert.ToInt32(cosTheta * rad2) + center.x
                                    , Convert.ToInt32(sinTheta * rad2) + center.y);
                        }
                    }
                    else
                    {
                        while (radius(r, center.x, center.y, end.x, end.y) < radiusLength)
                        {
                            rad2++;
                            end = Coord.get(clamp(Convert.ToInt32(cosTheta * rad2) + center.x, 0, width)
                                          , clamp(Convert.ToInt32(sinTheta * rad2) + center.y, 0, height));
                            if (end.x == 0 || end.x == width - 1 || end.y == 0 || end.y == height - 1)
                                return end;
                        }
                    }

                    return end;
                }
                default:
                {
                    end = Coord.get(clamp(Convert.ToInt32(cosTheta * radiusLength) + center.x, 0, width)
                            , clamp(Convert.ToInt32(sinTheta * radiusLength) + center.y, 0, height));
                    if (!surpassEdges)
                    {
                        long edgeLength = 0;
                        //                    if (end.x == 0 || end.x == width - 1 || end.y == 0 || end.y == height - 1)
                        if (end.x < 0)
                        {
                            // wow, we lucked out here. the only situation where cos(angle) is 0 is if the angle aims
                            // straight up or down, and then x cannot be < 0 or >= width.
                            edgeLength = Convert.ToInt32((0 - center.x) / cosTheta);
                            end = end.setY(clamp(Convert.ToInt32(sinTheta * edgeLength) + center.y, 0, height));
                        }
                        else if (end.x >= width)
                        {
                            // wow, we lucked out here. the only situation where cos(angle) is 0 is if the angle aims
                            // straight up or down, and then x cannot be < 0 or >= width.
                            edgeLength = Convert.ToInt32((width - 1 - center.x) / cosTheta);
                            end = end.setY(clamp(Convert.ToInt32(sinTheta * edgeLength) + center.y, 0, height));
                        }

                        if (end.y < 0)
                        {
                            // wow, we lucked out here. the only situation where sin(angle) is 0 is if the angle aims
                            // straight left or right, and then y cannot be < 0 or >= height.
                            edgeLength = Convert.ToInt32((0 - center.y) / sinTheta);
                            end = end.setX(clamp(Convert.ToInt32(cosTheta * edgeLength) + center.x, 0, width));
                        }
                        else if (end.y >= height)
                        {
                            // wow, we lucked out here. the only situation where sin(angle) is 0 is if the angle aims
                            // straight left or right, and then y cannot be < 0 or >= height.
                            edgeLength = Convert.ToInt32((height - 1 - center.y) / sinTheta);
                            end = end.setX(clamp(Convert.ToInt32(cosTheta * edgeLength) + center.x, 0, width));
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
        public static bool equals2D(this Radius r, Radius other)
        {
            switch (r)
            {
                case Radius.ROUGH_CIRCLE:
                    return other == Radius.ROUGH_CIRCLE;
                case Radius.CIRCLE:
                case Radius.SPHERE:
                    return other == Radius.CIRCLE || other == Radius.SPHERE;
                case Radius.SQUARE:
                case Radius.CUBE:
                    return other == Radius.SQUARE || other == Radius.CUBE;
                default:
                    return other == Radius.DIAMOND || other == Radius.OCTAHEDRON;
            }
        }
        public static bool inRange(this Radius r, int startx, int starty, int endx, int endy, int minRange, int maxRange)
        {
            double dist = radius(r, startx, starty, endx, endy);
            return dist >= minRange - 0.001 && dist <= maxRange + 0.001;
        }

        public static int roughDistance(this Radius r, int xPos, int yPos)
        {
            int x = Math.Abs(xPos), y = Math.Abs(yPos);
            switch (r)
            {
                case Radius.CIRCLE:
                case Radius.SPHERE:
                case Radius.ROUGH_CIRCLE:
                {
                    if (x == y) return 3 * x;
                    else if (x < y) return 3 * x + 2 * (y - x);
                    else return 3 * y + 2 * (x - y);
                }
                case Radius.DIAMOND:
                case Radius.OCTAHEDRON:
                    return 2 * (x + y);
                default:
                    return 2 * Math.Max(x, y);
            }
        }

        public static List<Coord> pointsInside(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height)
        {
            return pointsInside(r, centerX, centerY, radiusLength, surpassEdges, width, height, null);
        }
        public static List<Coord> pointsInside(this Radius r, Coord center, int radiusLength, bool surpassEdges, int width, int height)
        {
            if (center == null) return null;
            return pointsInside(r, center.x, center.y, radiusLength, surpassEdges, width, height, null);
        }

        public static List<Coord> pointsInside(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height, List<Coord> buf)
        {
            List<Coord> contents = buf == null ? new List<Coord>((int)Math.Ceiling(volume2D(r, radiusLength))) : buf;
            if (!surpassEdges && (centerX < 0 || centerX >= width || centerY < 0 || centerY >= height))
                return contents;
            if (radiusLength < 1)
            {
                contents.Add(Coord.get(centerX, centerY));
                return contents;
            }
            switch (r)
            {
                case Radius.SQUARE:
                case Radius.CUBE:
                {
                    for (int i = centerX - radiusLength; i <= centerX + radiusLength; i++)
                    {
                        for (int j = centerY - radiusLength; j <= centerY + radiusLength; j++)
                        {
                            if (!surpassEdges && (i < 0 || j < 0 || i >= width || j >= height))
                                continue;
                            contents.Add(Coord.get(i, j));
                        }
                    }
                }
                break;
                case Radius.DIAMOND:
                case Radius.OCTAHEDRON:
                {
                    for (int i = centerX - radiusLength; i <= centerX + radiusLength; i++)
                    {
                        for (int j = centerY - radiusLength; j <= centerY + radiusLength; j++)
                        {
                            if ((Math.Abs(centerX - i) + Math.Abs(centerY - j) > radiusLength) ||
                                    (!surpassEdges && (i < 0 || j < 0 || i >= width || j >= height)))
                                continue;
                            contents.Add(Coord.get(i, j));
                        }
                    }
                }
                break;
                default:
                {
                    float high, changedX;
                    int rndX, rndY;
                    for (int dx = -radiusLength; dx <= radiusLength; ++dx)
                    {
                        changedX = dx - 0.25f * Math.Sign(dx);
                        rndX = Convert.ToInt32(changedX);
                        high = (float)Math.Sqrt(radiusLength * radiusLength - changedX * changedX);
                        if (surpassEdges || !(centerX + rndX < 0 ||
                                centerX + rndX >= width))
                            contents.Add(Coord.get(centerX + rndX, centerY));
                        for (float dy = high; dy >= 0.75f; --dy)
                        {
                            rndY = Convert.ToInt32(dy - 0.25f);
                            if (surpassEdges || !(centerX + rndX < 0 || centerY + rndY < 0 ||
                                    centerX + rndX >= width || centerY + rndY >= height))
                                contents.Add(Coord.get(centerX + rndX, centerY + rndY));
                            if (surpassEdges || !(centerX + rndX < 0 || centerY - rndY < 0 ||
                                    centerX + rndX >= width || centerY - rndY >= height))
                                contents.Add(Coord.get(centerX + rndX, centerY - rndY));
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
        public static List<Coord> inSquare(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height)
        {
            return Radius.SQUARE.pointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, null);
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
        public static List<Coord> inDiamond(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height)
        {
            return Radius.DIAMOND.pointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, null);
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
        public static List<Coord> inCircle(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height)
        {
            return Radius.CIRCLE.pointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, null);
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
        public static List<Coord> inSquare(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height, List<Coord> buf)
        {
            return Radius.SQUARE.pointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, buf);
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
        public static List<Coord> inDiamond(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height, List<Coord> buf)
        {
            return Radius.DIAMOND.pointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, buf);
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
        public static List<Coord> inCircle(this Radius r, int centerX, int centerY, int radiusLength, bool surpassEdges, int width, int height, List<Coord> buf)
        {
            return Radius.CIRCLE.pointsInside(centerX, centerY, radiusLength, surpassEdges, width, height, buf);
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
        public static OrderedSet<Coord> expand(this Radius r, int distance, int width, int height, IEnumerable<Coord> points)
        {
            List<Coord> around = pointsInside(r, Coord.get(distance, distance), distance, false, width, height);
            OrderedSet<Coord> expanded = new OrderedSet<Coord>(around.Capacity * 16, 0.25f);
            int tx, ty;
            foreach (Coord pt in points)
            {
                foreach (Coord ar in around)
                {
                    tx = pt.x + ar.x - distance;
                    ty = pt.y + ar.y - distance;
                    if (tx >= 0 && tx < width && ty >= 0 && ty < height)
                        expanded.Add(Coord.get(tx, ty));
                }
            }
            return expanded;
        }
    }

}
