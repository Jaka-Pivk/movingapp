using API.Entities;
using Stripe;

namespace API.Services
{
    public class GpsService
    {
        public GpsService()
        {

        }

        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
          const double a = 6378137.0; // Major semi-axis of Earth (meters)
            const double b = 6356752.3142; // Minor semi-axis of Earth (meters)
            const double f = 1 / 298.257223563; // Earth's flattening

            var L = (lon2 - lon1) * (Math.PI / 180);
            var U1 = Math.Atan((1 - f) * Math.Tan(lat1 * (Math.PI / 180)));
            var U2 = Math.Atan((1 - f) * Math.Tan(lat2 * (Math.PI / 180)));

            double sinU1 = Math.Sin(U1);
            double cosU1 = Math.Cos(U1);
            double sinU2 = Math.Sin(U2);
            double cosU2 = Math.Cos(U2);

            double lambda = L;
            double lambdaP;
            double cosSqAlpha;
            double cos2SigmaM;
            double sinSigma;
            double cosSigma;
            double sigma;

            int iterLimit = 100;
            do
            {
                double sinLambda = Math.Sin(lambda);
                double cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) + (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                double sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cosSqAlpha = 1 - sinAlpha * sinAlpha;
                cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;
                lambdaP = lambda;
                lambda = L + (1 - f) * f * sinAlpha * (sigma + f / 4 * (cosSqAlpha * (4 - 3 * cosSqAlpha) - (3 * cosSqAlpha) * (4 - 3 * cosSqAlpha) * cos2SigmaM));

            } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

            if (iterLimit == 0)
            {
                return double.NaN; // formula failed to converge
            }

            double uSq = cosSqAlpha * (a * a - b * b) / (b * b);
            double A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
            double B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));
            double deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) - B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
            double s = b * A * (sigma - deltaSigma);
            return s;
        }
    }
}