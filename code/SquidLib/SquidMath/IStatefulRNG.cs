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
        string getState();

        /**
         * Set the current internal state of this StatefulRandomness with a long.
         *
         * @param state a 64-bit long. You should avoid passing 0, even though some implementations can handle that.
         */
        void setState(string state);
    }
}
