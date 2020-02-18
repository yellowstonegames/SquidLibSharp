namespace SquidLib.SquidMath {

    /**
     * A simple interface for IRNGs that have the additional property of a state that can be re-set.
     * Created by Tommy Ettinger on 9/15/2015.
     */
    public interface IStatefulRNG : IRNG {
        /**
         * Get the current internal state of the IStatefulRNG as a string, which only has to encode the state so that
         * an IStatefulRNG implementation with the same class can load the state back with setState().
         * @return the current internal state of this object.
         */
        string StateCode { get; set; }
    }
}
