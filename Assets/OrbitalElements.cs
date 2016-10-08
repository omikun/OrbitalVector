using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;

namespace OrbitalTools
{
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
        public double getPeriod()
        {
            return 2 * Math.PI * Math.Sqrt(Math.Pow(sma, 3) / OrbitData.parentGM);
        }
        public OrbitalElements copyOE()
        {
            OrbitalElements ret = new OrbitalElements();
            ret.sma = sma;
            ret.lan = lan;
            ret.inc = inc;
            ret.ecc = ecc;
            ret.tra = tra;
            ret.aop = aop;
            return ret;
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
        public static double timeUntilAnomaly(double grav_param, OrbitalElements oe, double
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
        public static double anomalyAfterTime(double grav_param, OrbitalElements oe, double
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


    }
    public class Util
    {
        /**
    * Converts orbital elements to position and velocity.
    * @param grav_param The gravitational parameter of the two-body system.
    * @param oe The orbital elements to be converted from.
    * @returns A vector corresponding to the concatenated position and
velocity.
*/
        public static Vector3 oe2r(double grav_param, OrbitalElements oe)
        {
            var v = oe2rd(grav_param, oe);
            Vector3 pos = new Vector3((float)v.x, (float)v.y, (float)v.z);
            return pos;
        }
        public static Vector3d oe2rd(double grav_param, OrbitalElements oe)
        {
            // rotation matrix
            double R11 =
    Math.Cos(oe.aop) * Math.Cos(oe.lan) - Math.Cos(oe.inc) * Math.Sin(oe.aop) * Math.Sin(oe.lan);
            double R12 =
    -Math.Cos(oe.lan) * Math.Sin(oe.aop) - Math.Cos(oe.inc) * Math.Cos(oe.aop) * Math.Sin(oe.lan);
            //double R13 = Math.Sin(oe.inc)*Math.Sin(oe.lan);
            double R21 =
    Math.Cos(oe.inc) * Math.Cos(oe.lan) * Math.Sin(oe.aop) + Math.Cos(oe.aop) * Math.Sin(oe.lan);
            double R22 =
    Math.Cos(oe.inc) * Math.Cos(oe.aop) * Math.Cos(oe.lan) - Math.Sin(oe.aop) * Math.Sin(oe.lan);
            //double R23 = -Math.Cos(oe.lan)*Math.Sin(oe.inc);
            double R31 = Math.Sin(oe.inc) * Math.Sin(oe.aop);
            double R32 = Math.Cos(oe.aop) * Math.Sin(oe.inc);
            //double R33 = Math.Cos(oe.inc);

            // semi-latus rectum
            double p = oe.sma * (1 - oe.ecc * oe.ecc);

            // position in the perifocal frame
            //std::vector<double> r_pf(3);
            double[] r_pf = new double[3];
            double r_norm = p / (1 + oe.ecc * Math.Cos(oe.tra));
            r_pf[0] = r_norm * Math.Cos(oe.tra);
            r_pf[1] = r_norm * Math.Sin(oe.tra);
            r_pf[2] = 0.0;

            // velocity in the perifocal frame
            //std::vector<double> v_pf(3);
            double[] v_pf = new double[3];
            v_pf[0] = Math.Sqrt(grav_param / p) * -Math.Sin(oe.tra);
            v_pf[1] = Math.Sqrt(grav_param / p) * (oe.ecc + Math.Cos(oe.tra));
            v_pf[2] = 0.0;

            // rotate the position and velocity into the body-fixed inertial frame
            //std::vector<double> rv(6);
            VectorD r = new VectorD();
            r.Resize(3);
            r[0] = R11 * r_pf[0] + R12 * r_pf[1] /*+R13*r_pf[2]*/;
            r[1] = R21 * r_pf[0] + R22 * r_pf[1] /*+R23*r_pf[2]*/;
            r[2] = R31 * r_pf[0] + R32 * r_pf[1] /*+R33*r_pf[2]*/;

            Vector3d pos = new Vector3d(r[0], r[1], r[2]);
            return pos;
        }
        public static void oe2rv(double grav_param, OrbitalElements oe, out Vector3d r, out Vector3d v)
        {
            var rv = oe2rv(grav_param, oe);
            r = new Vector3d(rv[0], rv[1], rv[2]);
            v = new Vector3d(rv[3], rv[4], rv[5]);
        }
        public static VectorD oe2rv(double grav_param, OrbitalElements oe)
        {
            // rotation matrix
            double R11 =
    Math.Cos(oe.aop) * Math.Cos(oe.lan) - Math.Cos(oe.inc) * Math.Sin(oe.aop) * Math.Sin(oe.lan);
            double R12 =
    -Math.Cos(oe.lan) * Math.Sin(oe.aop) - Math.Cos(oe.inc) * Math.Cos(oe.aop) * Math.Sin(oe.lan);
            //double R13 = Math.Sin(oe.inc)*Math.Sin(oe.lan);
            double R21 =
    Math.Cos(oe.inc) * Math.Cos(oe.lan) * Math.Sin(oe.aop) + Math.Cos(oe.aop) * Math.Sin(oe.lan);
            double R22 =
    Math.Cos(oe.inc) * Math.Cos(oe.aop) * Math.Cos(oe.lan) - Math.Sin(oe.aop) * Math.Sin(oe.lan);
            //double R23 = -Math.Cos(oe.lan)*Math.Sin(oe.inc);
            double R31 = Math.Sin(oe.inc) * Math.Sin(oe.aop);
            double R32 = Math.Cos(oe.aop) * Math.Sin(oe.inc);
            //double R33 = Math.Cos(oe.inc);

            // semi-latus rectum
            double p = oe.sma * (1 - oe.ecc * oe.ecc);

            // position in the perifocal frame
            //std::vector<double> r_pf(3);
            double[] r_pf = new double[3];
            double r_norm = p / (1 + oe.ecc * Math.Cos(oe.tra));
            r_pf[0] = r_norm * Math.Cos(oe.tra);
            r_pf[1] = r_norm * Math.Sin(oe.tra);
            r_pf[2] = 0.0;

            // velocity in the perifocal frame
            //std::vector<double> v_pf(3);
            double[] v_pf = new double[3];
            v_pf[0] = Math.Sqrt(grav_param / p) * -Math.Sin(oe.tra);
            v_pf[1] = Math.Sqrt(grav_param / p) * (oe.ecc + Math.Cos(oe.tra));
            v_pf[2] = 0.0;

            // rotate the position and velocity into the body-fixed inertial frame
            //std::vector<double> rv(6);
            VectorD rv = new VectorD();
            rv.Resize(6);
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
        public static OrbitalElements rv2oe(double grav_param, VectorD rv)
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
        public static VectorD convertToRv(ref double[] pos, ref double[] vel)
        {
            Debug.Assert(vel.Length == 3);
            Debug.Assert(pos.Length == 3);

            VectorD params_ = new VectorD();
            params_.Resize(6);
            Debug.Assert(params_.Count == 6);
            //2013 Orbital Vector was written with z being up
            //unity y is up, so must swap axis
            params_[0] = pos[0];
            params_[1] = pos[1];
            params_[2] = pos[2];
            params_[3] = vel[0];
            params_[4] = vel[1];
            params_[5] = vel[2];
            return params_;
        }
        public static VectorD convertToParams(double[] parentPos, double gm, double[] accel)
        {
            VectorD params_ = new VectorD();
            params_.Resize(7);
            params_[0] = parentPos[0];
            params_[1] = parentPos[1];
            params_[2] = parentPos[2];
            params_[3] = gm;
            params_[4] = accel[0];
            params_[5] = accel[1];
            params_[6] = accel[2];
            return params_;
        }
        public static VectorD forwardEuler(double t0, double dt, VectorD x0, VectorD params_)
        {
            return x0 + dt * dynamics(t0, x0, params_);
        }

        public static VectorD rungeKutta4(double t0, double dt, VectorD x0, VectorD params_)
        {
            VectorD k1 = dynamics(t0, x0, params_);
            VectorD k2 = dynamics(t0 + dt / 2, x0 + k1 * dt / 2, params_);
            VectorD k3 = dynamics(t0 + dt / 2, x0 + k2 * dt / 2, params_);
            VectorD k4 = dynamics(t0 + dt, x0 + k3 * dt, params_);
            return x0 + dt / 6 * (k1 + 2 * k2 + 2 * k3 + k4);
        }

        public static VectorD dynamics(double t, VectorD x, VectorD params_)
        {
            Debug.Assert(x.Count == 6);
            Debug.Assert(params_.Count == 7);

            VectorD dx = new VectorD();
            dx.Resize(6);

            //the derivative of position is velocity
            dx[0] = x[3];
            dx[1] = x[4];
            dx[2] = x[5];

            //distance between the two bodies
            double distance = 0.0;
            distance += (x[0] - params_[0]) * (x[0] - params_[0]);
            distance += (x[1] - params_[1]) * (x[1] - params_[1]);
            distance += (x[2] - params_[2]) * (x[2] - params_[2]);
            distance = Math.Sqrt(distance);

            //acceleration due to gravity
            dx[3] = -params_[3] * (x[0] - params_[0]) / Math.Pow(distance, 3.0);
            dx[4] = -params_[3] * (x[1] - params_[1]) / Math.Pow(distance, 3.0);
            dx[5] = -params_[3] * (x[2] - params_[2]) / Math.Pow(distance, 3.0);

            //perturbing acceleration
            dx[3] += params_[4];
            dx[4] += params_[5];
            dx[5] += params_[6];

            return dx;

        }
    }
}