namespace SquidLib.SquidMath
{
    public interface IRNG
    {
        int between(int v1, int v2);
        double between(int v, double pI2);
        double nextDouble();
    }
}
