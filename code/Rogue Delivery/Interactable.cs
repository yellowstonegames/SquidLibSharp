using System.Collections.Generic;
using System.Drawing;

using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace RogueDelivery {
    public interface IInteractable {
        Coord Location { get; set; }
        bool Blocking { get; set; }

        /// <summary>
        /// Returns the Coords that make up the body of this object with the current facing as relative
        /// to the current map.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Coord> Coords();

        /// <summary>
        /// Returns teh Corods that make up the body of this object iwth the curreent location and the
        /// provided facing.
        /// </summary>
        /// <param name="facing"></param>
        /// <returns></returns>
        IEnumerable<Coord> Coords(Direction facing);

        /// <summary>
        /// Returns the maximum outer bounds for the current facing.
        /// 
        /// Accounts for the current Location.
        /// </summary>
        /// <returns></returns>
        Rectangle OuterBounds();

        /// <summary>
        /// Returns the maximum outer bounds for the provided direction facing.
        /// 
        /// Accounts for the current Location.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        Rectangle OuterBounds(Direction direction);

        /// <summary>
        /// Returns true if the provided IInteractable instersects with this one at their
        /// current locations and facings.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool Intersects(IInteractable other);

        /// <summary>
        /// Returns true if the provided Coord is within this IInteractable at its current lcoation
        /// and with its current facing.
        /// </summary>
        /// <param name="intersector"></param>
        /// <returns></returns>
        bool Intersects(Coord intersector);

        /// <summary>
        /// Returns true if any of the provided Coords are within this IInteractable at its current
        /// location and with its current facing.
        /// </summary>
        /// <param name="intersectors"></param>
        /// <returns></returns>
        bool Intersects(IEnumerable<Coord> intersectors);

        /// <summary>
        /// Returns true if any part of the provided Rectangle is within the outer bounds of this
        /// IIneteracable at it's current location and with its current facing.
        /// </summary>
        /// <param name="intersector"></param>
        /// <returns></returns>
        bool IntersectsBounds(Rectangle intersector);
    }
}
