using System;
using System.Runtime.CompilerServices;

namespace SquidLib.SquidMath {
    /// <summary>
    /// A way to get 64-bit hash codes of strings and arrays that vary based on a 64-bit seed.
    /// This is meant to serve as a countermeasure for the potential non-deterministic behavior of the CLR
    /// when randomized hashing is enabled. It also helps when many hashes are needed but they need to differ
    /// based on their seeds, even if they hash the same strings. A common way of using this is by either
    /// constructing a new SeededHash when a new seed is needeed, by using one of the 192 SeededHash items in
    /// the <code>Predefined</code> field, or just using <code>Instance</code>.
    /// </summary>
    public class SeededHash {
        private readonly ulong seed;
        internal const ulong b0 = 0xA0761D6478BD642FUL;
        internal const ulong b1 = 0xE7037ED1A0B428DBUL;
        internal const ulong b2 = 0x8EBC6AF09C88C6E3UL;
        internal const ulong b3 = 0x589965CC75374CC3UL;
        internal const ulong b4 = 0x1D8E4E27C47D124FUL;
        internal const ulong b5 = 0xEB44ACCAB455D165UL;

        /**
         * Takes two arguments that are technically ulongs, and should be very different, and uses them to get a result
         * that is technically a ulong and mixes the bits of the inputs. The arguments and result are only technically
         * ulongs because their lower 32 bits matter much more than their upper 32, and giving just any ulong won't work.
         * <br>
         * This is very similar to wyhash's wymum function, but doesn't use 128-bit math because it expects that its
         * arguments are only relevant in their lower 32 bits (allowing their product to fit in 64 bits).
         * @param a a ulong that should probably only hold an int's worth of data
         * @param b a ulong that should probably only hold an int's worth of data
         * @return a sort-of randomized output dependent on both inputs
         */
        public static ulong mum(ulong a, ulong b) {
            ulong n = a * b;
            return n - (n >> 32);
        }

        /**
         * A slower but higher-quality variant on {@link #mum(ulong, ulong)} that can take two arbitrary ulongs (with any
         * of their 64 bits containing relevant data) instead of mum's 32-bit sections of its inputs, and outputs a
         * 64-bit result that can have any of its bits used.
         * @param a any ulong
         * @param b any ulong
         * @return a sort-of randomized output dependent on both inputs
         */
        public static ulong wow(ulong a, ulong b) {
            ulong n = (a ^ (b << 39 | b >> 25)) * (b ^ (a << 39 | a >> 25));
            return n ^ (n >> 32);
        }
        public SeededHash() => seed = 9069147967908697017UL;
        public SeededHash(ulong seed) {
            seed += b1;
            this.seed = seed ^ seed >> 23 ^ seed >> 48 ^ seed << 7 ^ seed << 53;
        }

        public SeededHash(string seed) => this.seed = OverkillHash(seed);

        public static readonly SeededHash
Alpha = new SeededHash("alpha"),
Beta = new SeededHash("beta"),
Gamma = new SeededHash("gamma"),
Delta = new SeededHash("delta"),
Epsilon = new SeededHash("epsilon"),
Zeta = new SeededHash("zeta"),
Eta = new SeededHash("eta"),
Theta = new SeededHash("theta"),
Iota = new SeededHash("iota"),
Kappa = new SeededHash("kappa"),
Lambda = new SeededHash("lambda"),
Mu = new SeededHash("mu"),
Nu = new SeededHash("nu"),
Xi = new SeededHash("xi"),
Omicron = new SeededHash("omicron"),
Pi = new SeededHash("pi"),
Rho = new SeededHash("rho"),
Sigma = new SeededHash("sigma"),
Tau = new SeededHash("tau"),
Upsilon = new SeededHash("upsilon"),
Phi = new SeededHash("phi"),
Chi = new SeededHash("chi"),
Psi = new SeededHash("psi"),
Omega = new SeededHash("omega"),
AlphaAlpha = new SeededHash("ALPHA"),
BetaBeta = new SeededHash("BETA"),
GammaGamma = new SeededHash("GAMMA"),
DeltaDelta = new SeededHash("DELTA"),
EpsilonEpsilon = new SeededHash("EPSILON"),
ZetaZeta = new SeededHash("ZETA"),
EtaEta = new SeededHash("ETA"),
ThetaTheta = new SeededHash("THETA"),
IotaIota = new SeededHash("IOTA"),
KappaKappa = new SeededHash("KAPPA"),
LambdaLambda = new SeededHash("LAMBDA"),
MuMu = new SeededHash("MU"),
NuNu = new SeededHash("NU"),
XiXi = new SeededHash("XI"),
OmicronOmicron = new SeededHash("OMICRON"),
PiPi = new SeededHash("PI"),
RhoRho = new SeededHash("RHO"),
SigmaSigma = new SeededHash("SIGMA"),
TauTau = new SeededHash("TAU"),
UpsilonUpsilon = new SeededHash("UPSILON"),
PhiPhi = new SeededHash("PHI"),
ChiChi = new SeededHash("CHI"),
PsiPsi = new SeededHash("PSI"),
OmegaOmega = new SeededHash("OMEGA"),
Baal = new SeededHash("baal"),
Agares = new SeededHash("agares"),
Vassago = new SeededHash("vassago"),
Samigina = new SeededHash("samigina"),
Marbas = new SeededHash("marbas"),
Valefor = new SeededHash("valefor"),
Amon = new SeededHash("amon"),
Barbatos = new SeededHash("barbatos"),
Paimon = new SeededHash("paimon"),
Buer = new SeededHash("buer"),
Gusion = new SeededHash("gusion"),
Sitri = new SeededHash("sitri"),
Beleth = new SeededHash("beleth"),
Leraje = new SeededHash("leraje"),
Eligos = new SeededHash("eligos"),
Zepar = new SeededHash("zepar"),
Botis = new SeededHash("botis"),
Bathin = new SeededHash("bathin"),
Sallos = new SeededHash("sallos"),
Purson = new SeededHash("purson"),
Marax = new SeededHash("marax"),
Ipos = new SeededHash("ipos"),
Aim = new SeededHash("aim"),
Naberius = new SeededHash("naberius"),
GlasyaLabolas = new SeededHash("glasya_labolas"),
Bune = new SeededHash("bune"),
Ronove = new SeededHash("ronove"),
Berith = new SeededHash("berith"),
Astaroth = new SeededHash("astaroth"),
Forneus = new SeededHash("forneus"),
Foras = new SeededHash("foras"),
Asmoday = new SeededHash("asmoday"),
Gaap = new SeededHash("gaap"),
Furfur = new SeededHash("furfur"),
Marchosias = new SeededHash("marchosias"),
Stolas = new SeededHash("stolas"),
Phenex = new SeededHash("phenex"),
Halphas = new SeededHash("halphas"),
Malphas = new SeededHash("malphas"),
Raum = new SeededHash("raum"),
Focalor = new SeededHash("focalor"),
Vepar = new SeededHash("vepar"),
Sabnock = new SeededHash("sabnock"),
Shax = new SeededHash("shax"),
Vine = new SeededHash("vine"),
Bifrons = new SeededHash("bifrons"),
Vual = new SeededHash("vual"),
Haagenti = new SeededHash("haagenti"),
Crocell = new SeededHash("crocell"),
Furcas = new SeededHash("furcas"),
Balam = new SeededHash("balam"),
Alloces = new SeededHash("alloces"),
Caim = new SeededHash("caim"),
Murmur = new SeededHash("murmur"),
Orobas = new SeededHash("orobas"),
Gremory = new SeededHash("gremory"),
Ose = new SeededHash("ose"),
Amy = new SeededHash("amy"),
Orias = new SeededHash("orias"),
Vapula = new SeededHash("vapula"),
Zagan = new SeededHash("zagan"),
Valac = new SeededHash("valac"),
Andras = new SeededHash("andras"),
Flauros = new SeededHash("flauros"),
Andrealphus = new SeededHash("andrealphus"),
Kimaris = new SeededHash("kimaris"),
Amdusias = new SeededHash("amdusias"),
Belial = new SeededHash("belial"),
Decarabia = new SeededHash("decarabia"),
Seere = new SeededHash("seere"),
Dantalion = new SeededHash("dantalion"),
Andromalius = new SeededHash("andromalius"),
BaalBaal = new SeededHash("BAAL"),
AgaresAgares = new SeededHash("AGARES"),
VassagoVassago = new SeededHash("VASSAGO"),
SamiginaSamigina = new SeededHash("SAMIGINA"),
MarbasMarbas = new SeededHash("MARBAS"),
ValeforValefor = new SeededHash("VALEFOR"),
AmonAmon = new SeededHash("AMON"),
BarbatosBarbatos = new SeededHash("BARBATOS"),
PaimonPaimon = new SeededHash("PAIMON"),
BuerBuer = new SeededHash("BUER"),
GusionGusion = new SeededHash("GUSION"),
SitriSitri = new SeededHash("SITRI"),
BelethBeleth = new SeededHash("BELETH"),
LerajeLeraje = new SeededHash("LERAJE"),
EligosEligos = new SeededHash("ELIGOS"),
ZeparZepar = new SeededHash("ZEPAR"),
BotisBotis = new SeededHash("BOTIS"),
BathinBathin = new SeededHash("BATHIN"),
SallosSallos = new SeededHash("SALLOS"),
PursonPurson = new SeededHash("PURSON"),
MaraxMarax = new SeededHash("MARAX"),
IposIpos = new SeededHash("IPOS"),
AimAim = new SeededHash("AIM"),
NaberiusNaberius = new SeededHash("NABERIUS"),
GlasyaLabolasGlasyaLabolas = new SeededHash("GLASYA_LABOLAS"),
BuneBune = new SeededHash("BUNE"),
RonoveRonove = new SeededHash("RONOVE"),
BerithBerith = new SeededHash("BERITH"),
AstarothAstaroth = new SeededHash("ASTAROTH"),
ForneusForneus = new SeededHash("FORNEUS"),
ForasForas = new SeededHash("FORAS"),
AsmodayAsmoday = new SeededHash("ASMODAY"),
GaapGaap = new SeededHash("GAAP"),
FurfurFurfur = new SeededHash("FURFUR"),
MarchosiasMarchosias = new SeededHash("MARCHOSIAS"),
StolasStolas = new SeededHash("STOLAS"),
PhenexPhenex = new SeededHash("PHENEX"),
HalphasHalphas = new SeededHash("HALPHAS"),
MalphasMalphas = new SeededHash("MALPHAS"),
RaumRaum = new SeededHash("RAUM"),
FocalorFocalor = new SeededHash("FOCALOR"),
VeparVepar = new SeededHash("VEPAR"),
SabnockSabnock = new SeededHash("SABNOCK"),
ShaxShax = new SeededHash("SHAX"),
VineVine = new SeededHash("VINE"),
BifronsBifrons = new SeededHash("BIFRONS"),
VualVual = new SeededHash("VUAL"),
HaagentiHaagenti = new SeededHash("HAAGENTI"),
CrocellCrocell = new SeededHash("CROCELL"),
FurcasFurcas = new SeededHash("FURCAS"),
BalamBalam = new SeededHash("BALAM"),
AllocesAlloces = new SeededHash("ALLOCES"),
CaimCaim = new SeededHash("CAIM"),
MurmurMurmur = new SeededHash("MURMUR"),
OrobasOrobas = new SeededHash("OROBAS"),
GremoryGremory = new SeededHash("GREMORY"),
OseOse = new SeededHash("OSE"),
AmyAmy = new SeededHash("AMY"),
OriasOrias = new SeededHash("ORIAS"),
VapulaVapula = new SeededHash("VAPULA"),
ZaganZagan = new SeededHash("ZAGAN"),
ValacValac = new SeededHash("VALAC"),
AndrasAndras = new SeededHash("ANDRAS"),
FlaurosFlauros = new SeededHash("FLAUROS"),
AndrealphusAndrealphus = new SeededHash("ANDREALPHUS"),
KimarisKimaris = new SeededHash("KIMARIS"),
AmdusiasAmdusias = new SeededHash("AMDUSIAS"),
BelialBelial = new SeededHash("BELIAL"),
DecarabiaDecarabia = new SeededHash("DECARABIA"),
SeereSeere = new SeededHash("SEERE"),
DantalionDantalion = new SeededHash("DANTALION"),
AndromaliusAndromalius = new SeededHash("ANDROMALIUS")
        ;

        public static readonly SeededHash Instance = Baal; // the big boss of every Disgaea game
        /**
         * Has a length of 192, which may be relevant if automatically choosing a predefined hash functor.
         */
        public static readonly SeededHash[] Predefined = new SeededHash[]{
            Alpha, Beta, Gamma, Delta, Epsilon, Zeta, Eta, Theta, Iota, Kappa, Lambda, Mu, Nu, Xi, Omicron, Pi, Rho, Sigma, Tau,
Upsilon, Phi, Chi, Psi, Omega, AlphaAlpha, BetaBeta, GammaGamma, DeltaDelta, EpsilonEpsilon, ZetaZeta, EtaEta,
ThetaTheta, IotaIota, KappaKappa, LambdaLambda, MuMu, NuNu, XiXi, OmicronOmicron, PiPi, RhoRho, SigmaSigma, TauTau,
UpsilonUpsilon, PhiPhi, ChiChi, PsiPsi, OmegaOmega, Baal, Agares, Vassago, Samigina, Marbas, Valefor, Amon, Barbatos,
Paimon, Buer, Gusion, Sitri, Beleth, Leraje, Eligos, Zepar, Botis, Bathin, Sallos, Purson, Marax, Ipos, Aim, Naberius,
GlasyaLabolas, Bune, Ronove, Berith, Astaroth, Forneus, Foras, Asmoday, Gaap, Furfur, Marchosias, Stolas, Phenex,
Halphas, Malphas, Raum, Focalor, Vepar, Sabnock, Shax, Vine, Bifrons, Vual, Haagenti, Crocell, Furcas, Balam, Alloces,
Caim, Murmur, Orobas, Gremory, Ose, Amy, Orias, Vapula, Zagan, Valac, Andras, Flauros, Andrealphus, Kimaris, Amdusias,
Belial, Decarabia, Seere, Dantalion, Andromalius, BaalBaal, AgaresAgares, VassagoVassago, SamiginaSamigina,
MarbasMarbas, ValeforValefor, AmonAmon, BarbatosBarbatos, PaimonPaimon, BuerBuer, GusionGusion, SitriSitri,
BelethBeleth, LerajeLeraje, EligosEligos, ZeparZepar, BotisBotis, BathinBathin, SallosSallos, PursonPurson, MaraxMarax,
IposIpos, AimAim, NaberiusNaberius, GlasyaLabolasGlasyaLabolas, BuneBune, RonoveRonove, BerithBerith, AstarothAstaroth,
ForneusForneus, ForasForas, AsmodayAsmoday, GaapGaap, FurfurFurfur, MarchosiasMarchosias, StolasStolas, PhenexPhenex,
HalphasHalphas, MalphasMalphas, RaumRaum, FocalorFocalor, VeparVepar, SabnockSabnock, ShaxShax, VineVine,
BifronsBifrons, VualVual, HaagentiHaagenti, CrocellCrocell, FurcasFurcas, BalamBalam, AllocesAlloces, CaimCaim,
MurmurMurmur, OrobasOrobas, GremoryGremory, OseOse, AmyAmy, OriasOrias, VapulaVapula, ZaganZagan, ValacValac,
AndrasAndras, FlaurosFlauros, AndrealphusAndrealphus, KimarisKimaris, AmdusiasAmdusias, BelialBelial,
DecarabiaDecarabia, SeereSeere, DantalionDantalion, AndromaliusAndromalius
    };


        /// <summary>
        /// A slower non-cryptographic hash that doesn't take a seed; meant to get usable 64-bit seeds from strings.
        /// </summary>
        /// <param name="data">Any string; may be null or any length.</param>
        /// <returns>A 64-bit hash of data.</returns>
        public static ulong OverkillHash(string data) {
            if (data is null)
                return b0;
            int size = data.Length;
            ulong result = b2 + RNG.Randomize((ulong)size);
            for (int i = 0; i < size; i++) {
                result = RNG.Determine(data[i] + result);
            }
            return result;
        }

        public ulong Hash64(byte[] data) {
            ulong seed = this.seed;
            if (data is null) return seed;
            int len = data.Length;
            for (int i = 3; i < len; i += 4) {
                seed = mum(
                        mum(data[i - 3] ^ b1, data[i - 2] ^ b2) + seed,
                        mum(data[i - 1] ^ b3, data[i] ^ b4));
            }
            switch (len & 3) {
                case 0: seed = mum(b1 ^ seed, b4 + seed); break;
                case 1: seed = mum(seed ^ b3, b4 ^ data[len - 1]); break;
                case 2: seed = mum(seed ^ data[len - 2], b3 ^ data[len - 1]); break;
                case 3: seed = mum(seed ^ data[len - 3] ^ (ulong)data[len - 2] << 16, b1 ^ data[len - 1]); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0);
            return seed - (seed >> 31) + (seed << 33);
        }
        public ulong Hash64(sbyte[] data) {
            if (data is null) return seed;
            return Hash64((byte[])(object)data);
        }

        public ulong Hash64(char[] data) {
            ulong seed = this.seed;
            if (data is null) return seed;
            int len = data.Length;
            for (int i = 3; i < len; i += 4) {
                seed = mum(
                        mum(data[i - 3] ^ b1, data[i - 2] ^ b2) + seed,
                        mum(data[i - 1] ^ b3, data[i] ^ b4));
            }
            switch (len & 3) {
                case 0: seed = mum(b1 ^ seed, b4 + seed); break;
                case 1: seed = mum(seed ^ b3, b4 ^ data[len - 1]); break;
                case 2: seed = mum(seed ^ data[len - 2], b3 ^ data[len - 1]); break;
                case 3: seed = mum(seed ^ data[len - 3] ^ (ulong)data[len - 2] << 16, b1 ^ data[len - 1]); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0);
            return seed - (seed >> 31) + (seed << 33);
        }

        public ulong Hash64(string data) {
            ulong seed = this.seed;
            if (data is null) return seed;
            int len = data.Length;
            for (int i = 3; i < len; i += 4) {
                seed = mum(
                        mum(data[i - 3] ^ b1, data[i - 2] ^ b2) + seed,
                        mum(data[i - 1] ^ b3, data[i] ^ b4));
            }
            switch (len & 3) {
                case 0: seed = mum(b1 ^ seed, b4 + seed); break;
                case 1: seed = mum(seed ^ b3, b4 ^ data[len - 1]); break;
                case 2: seed = mum(seed ^ data[len - 2], b3 ^ data[len - 1]); break;
                case 3: seed = mum(seed ^ data[len - 3] ^ (ulong)data[len - 2] << 16, b1 ^ data[len - 1]); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0);
            return seed - (seed >> 31) + (seed << 33);
        }

        public ulong Hash64(ushort[] data) {
            ulong seed = this.seed;
            if (data is null) return seed;
            int len = data.Length;
            for (int i = 3; i < len; i += 4) {
                seed = mum(
                        mum(data[i - 3] ^ b1, data[i - 2] ^ b2) + seed,
                        mum(data[i - 1] ^ b3, data[i] ^ b4));
            }
            switch (len & 3) {
                case 0: seed = mum(b1 ^ seed, b4 + seed); break;
                case 1: seed = mum(seed ^ b3, b4 ^ data[len - 1]); break;
                case 2: seed = mum(seed ^ data[len - 2], b3 ^ data[len - 1]); break;
                case 3: seed = mum(seed ^ data[len - 3] ^ (ulong)data[len - 2] << 16, b1 ^ data[len - 1]); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0);
            return seed - (seed >> 31) + (seed << 33);
        }
        public ulong Hash64(short[] data) {
            if (data is null) return seed;
            return Hash64((ushort[])(object)data);
        }

        public ulong Hash64(uint[] data) {
            ulong seed = this.seed;
            if (data is null) return seed;
            int len = data.Length;
            for (int i = 3; i < len; i += 4) {
                seed = mum(
                        mum(data[i - 3] ^ b1, data[i - 2] ^ b2) + seed,
                        mum(data[i - 1] ^ b3, data[i] ^ b4));
            }
            switch (len & 3) {
                case 0: seed = mum(b1 ^ seed, b4 + seed); break;
                case 1: seed = mum(seed ^ b3, b4 ^ data[len - 1]); break;
                case 2: seed = mum(seed ^ data[len - 2], b3 ^ data[len - 1]); break;
                case 3: seed = mum(seed ^ data[len - 3] ^ (ulong)data[len - 2] << 16, b1 ^ data[len - 1]); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0);
            return seed - (seed >> 31) + (seed << 33);
        }
        public ulong Hash64(int[] data) {
            if (data is null) return seed;
            return Hash64((uint[])(object)data);
        }
        public ulong Hash64(ulong[] data) {
            if (data is null) return this.seed;
            ulong seed = this.seed, a = this.seed + b4, b = this.seed + b3, c = this.seed + b2, d = this.seed + b1;
            int len = data.Length;
            for (int i = 3; i < len; i += 4) {
                a ^= data[i - 3] * b1; a = (a << 23 | a >> 41) * b3;
                b ^= data[i - 2] * b2; b = (b << 25 | b >> 39) * b4;
                c ^= data[i - 1] * b3; c = (c << 29 | c >> 35) * b5;
                d ^= data[i] * b4; d = (d << 31 | d >> 33) * b1;
                seed += a + b + c + d;
            }
            seed += b5;
            switch (len & 3) {
                case 1: seed = wow(seed, b1 ^ data[len - 1]); break;
                case 2: seed = wow(seed + data[len - 2], b2 + data[len - 1]); break;
                case 3: seed = wow(seed + data[len - 3], b2 + data[len - 2]) ^ wow(seed + data[len - 1], seed ^ b3); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0 ^ seed >> 32);
            return seed - (seed >> 31) + (seed << 33);
        }
        public ulong Hash64(long[] data) {
            if (data is null) return seed;
            return Hash64((ulong[])(object)data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong FloatToULong(float value) => *((uint*)&value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong DoubleToULong(double value) => *((ulong*)&value);
        public ulong Hash64(float[] data) {
            ulong seed = this.seed;
            if (data is null) return seed;
            int len = data.Length;
            for (int i = 3; i < len; i += 4) {
                seed = mum(
                        mum(FloatToULong(data[i - 3]) ^ b1, FloatToULong(data[i - 2]) ^ b2) + seed,
                        mum(FloatToULong(data[i - 1]) ^ b3, FloatToULong(data[i]) ^ b4));
            }
            switch (len & 3) {
                case 0: seed = mum(b1 ^ seed, b4 + seed); break;
                case 1: seed = mum(seed ^ b3, b4 ^ FloatToULong(data[len - 1])); break;
                case 2: seed = mum(seed ^ FloatToULong(data[len - 2]), b3 ^ FloatToULong(data[len - 1])); break;
                case 3: seed = mum(seed ^ FloatToULong(data[len - 3]) ^ FloatToULong(data[len - 2]) << 16, b1 ^ FloatToULong(data[len - 1])); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0);
            return seed - (seed >> 31) + (seed << 33);
        }

        public ulong Hash64(double[] data) {
            if (data is null) return this.seed;
            ulong seed = this.seed, a = this.seed + b4, b = this.seed + b3, c = this.seed + b2, d = this.seed + b1;
            int len = data.Length;
            for (int i = 3; i < len; i += 4) {
                a ^= DoubleToULong(data[i - 3]) * b1; a = (a << 23 | a >> 41) * b3;
                b ^= DoubleToULong(data[i - 2]) * b2; b = (b << 25 | b >> 39) * b4;
                c ^= DoubleToULong(data[i - 1]) * b3; c = (c << 29 | c >> 35) * b5;
                d ^= DoubleToULong(data[i    ]) * b4; d = (d << 31 | d >> 33) * b1;
                seed += a + b + c + d;
            }
            seed += b5;
            switch (len & 3) {
                case 1: seed = wow(seed, b1 ^ DoubleToULong(data[len - 1])); break;
                case 2: seed = wow(seed + DoubleToULong(data[len - 2]), b2 + DoubleToULong(data[len - 1])); break;
                case 3: seed = wow(seed + DoubleToULong(data[len - 3]), b2 + DoubleToULong(data[len - 2])) ^ wow(seed + DoubleToULong(data[len - 1]), seed ^ b3); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0 ^ seed >> 32);
            return seed - (seed >> 31) + (seed << 33);
        }

        public ulong Hash64Alt(string data) {
            ulong seed = this.seed;
            if (data is null) return seed;
            int len = data.Length;
            seed += (ulong)len;
            for (int i = 3; i < len; i += 4) {
                seed = mum(
                        mum(data[i - 3] ^ b1, data[i - 2] ^ b2) + seed,
                        mum(data[i - 1] ^ b3, data[i] ^ b4));
            }
            switch (len & 3) {
                case 0: seed = mum(b1 ^ seed, b4 + seed); break;
                case 1: seed = mum(seed ^ b3, b4 ^ data[len - 1]); break;
                case 2: seed = mum(seed ^ data[len - 2], b3 ^ data[len - 1]); break;
                case 3: seed = mum(seed ^ data[len - 3] ^ (ulong)data[len - 2] << 16, b1 ^ data[len - 1]); break;
            }
            seed = (seed ^ seed << 16) * ((ulong)len ^ b0);
            return seed - (seed >> 31) + (seed << 33);
        }
        /// <summary>
        /// The hash Java's String.hashCode() uses, for comparison.
        /// </summary>
        /// <param name="data">Any string.</param>
        /// <returns>A 32-bit int hash code of data.</returns>
        public static int JavaHash(string data) {
            if (data == null) return 0;
            int h = 1;
            unchecked {
                foreach (char c in data) {
                    h = h * 31 + c;
                }
            }
            return h;
        }
        /// <summary>
        /// Both the upper half and lower half of the 64-bit hash this produces are good at avoiding collisions on English words.
        /// Much simpler than the seeded hashes here, though this is effectively the same as running <code>PhiHash32</code> and a slight variant
        /// on that method on the same data and using one for the bottom half, the other for the top.
        /// </summary>
        /// <param name="data">Any string.</param>
        /// <returns>A 64-bit ulong hash code of data.</returns>
        public static ulong PhiHash(string data) {
            if (data == null) return 0;
            unchecked {
                //ulong h = 0xC6BC279692B5C323UL;
                ulong h1 = 0x4BFD899B8EB6C433UL;
                uint h2 = 0x92B5C323U;
                char c;
                for (int i = 0; i < data.Length; i++) {
                    c = data[i];
                    h1 = (h1 ^ c) * 0x9E3779B9B68B5351UL;
                    //h1 = (h1 ^ h1 >> 32 ^ c) * 0x9E3779B9B68B5351UL;
                    h2 = (h2 ^ c) * 0x7F4A7C15U;
                }
                return h1 << 32 ^ h2;
            }
        }
        /// <summary>
        /// Very good at avoiding collisions on 32-bit int outputs; not seeded.
        /// Much simpler than the seeded hashes here.
        /// </summary>
        /// <param name="data">Any string.</param>
        /// <returns>A 32-bit int hash code of data.</returns>
        public static int PhiHash32(string data) {
            if (data == null) return 0;
            unchecked {
                int h = 0x52B5C323;
                for (int i = 0; i < data.Length; i++) {
                    h = (h ^ data[i]) * 0x7F4A7C15;
                }
                return h;
            }
        }
    }
}
