using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;

namespace OrbitalTools { 
public class OrbitalElements
{
    public double sma;
    public double ecc;
    public double inc;
    public double lan;
    public double aop;
    public double tra;
    public void print()
    {
        Debug.Log("sma: " + sma);
        Debug.Log("ecc: " + ecc);
        Debug.Log("inc: " + inc);
        Debug.Log("lan: " + lan);
        Debug.Log("aop: " + aop);
        Debug.Log("tra: " + tra);
    }
}
public class Program
{

    /**
* Computes the delta-time until a particular true-anomaly is reached.
* @param       grav_param      The gravitational parameter of the two-body
system.
* @param       oe      The orbital elements of the body of interest.
* @param       true_anomaly    The true anomaly at the final time.
* @returns     The amount of time until the true anomaly has been reached.
*/
    public static double Atanh(double x)
    {
        return (Math.Log(1 + x) - Math.Log(1 - x)) / 2;
    }
    double timeUntilAnomaly(double grav_param, OrbitalElements oe, double
    true_anomaly)
    {
        if (oe.sma > 0.0)
        {
            double ecc_anomaly_i =
2.0 * Math.Atan(Math.Sqrt((1.0 - oe.ecc) / (1.0 + oe.ecc)) * Math.Tan(oe.tra / 2.0));
            double ecc_anomaly_f =
2.0 * Math.Atan(Math.Sqrt((1.0 - oe.ecc) / (1.0 + oe.ecc)) * Math.Tan(true_anomaly / 2.0));

            if (ecc_anomaly_f < ecc_anomaly_i) ecc_anomaly_f +=
2.0 * Math.PI;

            double mean_anomaly_i =
ecc_anomaly_i - oe.ecc * Math.Sin(ecc_anomaly_i);
            double mean_anomaly_f =
ecc_anomaly_f - oe.ecc * Math.Sin(ecc_anomaly_f);

            return
Math.Sqrt(Math.Pow(oe.sma, 3.0) / grav_param) * (mean_anomaly_f - mean_anomaly_i);
        }
        else
        {
            double hyp_anomaly_i =
2.0 * Atanh(Math.Sqrt((oe.ecc - 1.0) / (1.0 + oe.ecc)) * Math.Tan(oe.tra / 2.0));
            double hyp_anomaly_f =
2.0 * Atanh(Math.Sqrt((oe.ecc - 1.0) / (1.0 + oe.ecc)) * Math.Tan(true_anomaly / 2.0));

            double mean_anomaly_i =
-hyp_anomaly_i + oe.ecc * Math.Sinh(hyp_anomaly_i);
            double mean_anomaly_f =
-hyp_anomaly_f + oe.ecc * Math.Sinh(hyp_anomaly_f);

            return
Math.Sqrt(Math.Pow(-oe.sma, 3.0) / grav_param) * (mean_anomaly_f - mean_anomaly_i);
        }
    }



    /**
* Computes the true anomaly after a delta-time has passed.
* @param       grav_param      The gravitational parameter of the two-body
system.
* @param       oe      The orbital elements of the body of interest.
* @param       delta_time      The elapsed time.
* @returns     The true anomaly at the final time.
*/
    double anomalyAfterTime(double grav_param, OrbitalElements oe, double
    delta_time)
    {
        if (oe.sma > 0.0)
        {
            double ecc_anomaly_i =
2.0 * Math.Atan(Math.Sqrt((1.0 - oe.ecc) / (1.0 + oe.ecc)) * Math.Tan(oe.tra / 2.0));
            double mean_anomaly_f =
ecc_anomaly_i - oe.ecc * Math.Sin(ecc_anomaly_i) + Math.Sqrt(grav_param / Math.Pow(oe.sma, 3.0)) * delta_time;

            // perform Newton-Raphson iteration to determine the final eccentric anomaly
            double ecc_anomaly_f = mean_anomaly_f;
            double error =
mean_anomaly_f - ecc_anomaly_f + oe.ecc * Math.Sin(ecc_anomaly_f);
            while (Math.Abs(error) > 1E-10)
            {
                ecc_anomaly_f =
ecc_anomaly_f - error / (oe.ecc * Math.Cos(ecc_anomaly_f) - 1.0);
                error =
mean_anomaly_f - ecc_anomaly_f + oe.ecc * Math.Sin(ecc_anomaly_f);
            }

            return
2.0 * Math.Atan(Math.Sqrt((1.0 + oe.ecc) / (1.0 - oe.ecc)) * Math.Tan(ecc_anomaly_f / 2.0));
        }
        else
        {
            double hyp_anomaly_i =
2.0 * Atanh(Math.Sqrt((oe.ecc - 1.0) / (1.0 + oe.ecc)) * Math.Tan(oe.tra / 2.0));
            double mean_anomaly_f =
-hyp_anomaly_i + oe.ecc * Math.Sinh(hyp_anomaly_i) + Math.Sqrt(grav_param / Math.Pow(-oe.sma, 3.0)) * delta_time;

            // perform Newton-Raphson iteration to determine the final eccentric anomaly
            double hyp_anomaly_f = mean_anomaly_f;
            double error =
mean_anomaly_f + hyp_anomaly_f - oe.ecc * Math.Sinh(hyp_anomaly_f);
            while (Math.Abs(error) > 1E-10)
            {
                hyp_anomaly_f =
hyp_anomaly_f - error / (oe.ecc * Math.Cosh(hyp_anomaly_f) - 1);
                error =
mean_anomaly_f + hyp_anomaly_f - oe.ecc * Math.Sinh(hyp_anomaly_f);
            }

            return
2.0 * Math.Atan(Math.Sqrt((1.0 + oe.ecc) / (oe.ecc - 1.0)) * Math.Tanh(hyp_anomaly_f / 2.0));
        }
    }


    /**
* Converts orbital elements to position and velocity.
* @param grav_param The gravitational parameter of the two-body system.
* @param oe The orbital elements to be converted from.
* @returns A vector corresponding to the concatenated position and
velocity.
*/
    public static List<double> oe2rv(double grav_param, OrbitalElements oe)
    {
        // rotation matrix
        double R11 =
Math.Acos(oe.aop) * Math.Acos(oe.lan) - Math.Acos(oe.inc) * Math.Sin(oe.aop) * Math.Sin(oe.lan);
        double R12 =
-Math.Acos(oe.lan) * Math.Sin(oe.aop) - Math.Acos(oe.inc) * Math.Acos(oe.aop) * Math.Sin(oe.lan);
        //double R13 = Math.Sin(oe.inc)*Math.Sin(oe.lan);
        double R21 =
Math.Acos(oe.inc) * Math.Acos(oe.lan) * Math.Sin(oe.aop) + Math.Acos(oe.aop) * Math.Sin(oe.lan);
        double R22 =
Math.Acos(oe.inc) * Math.Acos(oe.aop) * Math.Acos(oe.lan) - Math.Sin(oe.aop) * Math.Sin(oe.lan);
        //double R23 = -Math.Acos(oe.lan)*Math.Sin(oe.inc);
        double R31 = Math.Sin(oe.inc) * Math.Sin(oe.aop);
        double R32 = Math.Acos(oe.aop) * Math.Sin(oe.inc);
        //double R33 = Math.Acos(oe.inc);

        // semi-latus rectum
        double p = oe.sma * (1 - oe.ecc * oe.ecc);

        // position in the perifocal frame
        //std::vector<double> r_pf(3);
        double[] r_pf = new double[3];
        double r_norm = p / (1 + oe.ecc * Math.Acos(oe.tra));
        r_pf[0] = r_norm * Math.Acos(oe.tra);
        r_pf[1] = r_norm * Math.Sin(oe.tra);
        r_pf[2] = 0.0;

        // velocity in the perifocal frame
        //std::vector<double> v_pf(3);
        double[] v_pf = new double[3];
        v_pf[0] = Math.Sqrt(grav_param / p) * -Math.Sin(oe.tra);
        v_pf[1] = Math.Sqrt(grav_param / p) * (oe.ecc + Math.Acos(oe.tra));
        v_pf[2] = 0.0;

        // rotate the position and velocity into the body-fixed inertial frame
        //std::vector<double> rv(6);
        double[] rv_ = new double[6];
        List<double> rv = new List<double>(rv_);
        rv[0] = R11 * r_pf[0] + R12 * r_pf[1] /*+R13*r_pf[2]*/;
        rv[1] = R21 * r_pf[0] + R22 * r_pf[1] /*+R23*r_pf[2]*/;
        rv[2] = R31 * r_pf[0] + R32 * r_pf[1] /*+R33*r_pf[2]*/;
        rv[3] = R11 * v_pf[0] + R12 * v_pf[1] /*+R13*v_pf[2]*/;
        rv[4] = R21 * v_pf[0] + R22 * v_pf[1] /*+R23*v_pf[2]*/;
        rv[5] = R31 * v_pf[0] + R32 * v_pf[1] /*+R33*v_pf[2]*/;

        return rv;
    }



    /**
     * Converts position and velocity to orbital elements
     * @param grav_param The gravitational parameter of the two-body system.
     * @param rv The concatenated position and velocity to convert from.
     * @returns The orbital elements.
     */
    public static OrbitalElements rv2oe(double grav_param, List<double> rv)
    {
        OrbitalElements oe = new OrbitalElements();

        // Semi-major Axis : Vis-viva Equation
        oe.sma = 1.0f / (
        2.0f / Math.Sqrt(rv[0] * rv[0] + rv[1] * rv[1] + rv[2] * rv[2])
        - (rv[3] * rv[3] + rv[4] * rv[4] + rv[5] * rv[5]) / grav_param
        );

        // Angular Momentum
        //List<double> h = new List<double>(3);
        double[] h = new double[3];
        h[0] = rv[1] * rv[5] - rv[2] * rv[4];
        h[1] = rv[2] * rv[3] - rv[0] * rv[5];
        h[2] = rv[0] * rv[4] - rv[1] * rv[3];

        // Norm of position
        double r = Math.Sqrt(rv[0] * rv[0] + rv[1] * rv[1] + rv[2] * rv[2]);

        // Eccentricity Vector :  e = v x h / mu - r/|r|
        //std::vector<double> e(3);
        double[] e = new double[3];
        e[0] = (rv[4] * h[2] - rv[5] * h[1]) / grav_param - rv[0] / r;
        e[1] = (rv[5] * h[0] - rv[3] * h[2]) / grav_param - rv[1] / r;
        e[2] = (rv[3] * h[1] - rv[4] * h[0]) / grav_param - rv[2] / r;

        // Eccentricity
        oe.ecc = Math.Sqrt(e[0] * e[0] + e[1] * e[1] + e[2] * e[2]);

        // Inclination
        oe.inc = Math.Acos(h[2] / Math.Sqrt(h[0] * h[0] + h[1] * h[1] + h[2] * h[2]));

        // Ascending Node Direction (In x-y plane)
        //std::vector<double> n(2);
        double[] n = new double[2];
        n[0] = -h[1] / Math.Sqrt(h[0] * h[0] + h[1] * h[1]);
        n[1] = h[0] / Math.Sqrt(h[0] * h[0] + h[1] * h[1]);
        double n_norm = Math.Sqrt(n[0] * n[0] + n[1] * n[1]);

        // Longitude of the Ascending Node
        oe.lan = Math.Acos(n[0]) / n_norm;
        if (n[1] < 0.0) oe.lan = 2 * Math.PI - oe.lan;

        // Argument of Periapsis
        oe.aop = Math.Acos((n[0] * e[0] + n[1] * e[1]) / (n_norm * oe.ecc));
        if (e[2] < 0.0) oe.aop = 2 * Math.PI - oe.aop;

        // True Anomaly
        oe.tra = Math.Acos((rv[0] * e[0] + rv[1] * e[1] + rv[2] * e[2]) / (r * oe.ecc));
        if (rv[0] * rv[3] + rv[1] * rv[4] + rv[2] * rv[5] < 0.0) oe.tra = 2 * Math.PI - oe.tra;

        return oe;
    }
    
}
}