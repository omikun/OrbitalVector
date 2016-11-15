/*****************************************************************************
 *   Copyright (C) 2004-2015 The PaGMO development team,                     *
 *   Advanced Concepts Team (ACT), European Space Agency (ESA)               *
 *                                                                           *
 *   https://github.com/esa/pagmo                                            *
 *                                                                           *
 *   act@esa.int                                                             *
 *                                                                           *
 *   This program is free software; you can redistribute it and/or modify    *
 *   it under the terms of the GNU General Public License as published by    *
 *   the Free Software Foundation; either version 2 of the License, or       *
 *   (at your option) any later version.                                     *
 *                                                                           *
 *   This program is distributed in the hope that it will be useful,         *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of          *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the           *
 *   GNU General Public License for more details.                            *
 *                                                                           *
 *   You should have received a copy of the GNU General Public License       *
 *   along with this program; if not, write to the                           *
 *   Free Software Foundation, Inc.,                                         *
 *   59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.               *
 *****************************************************************************/
using System;
using System.Diagnostics;
#if false
class CZF : public ZeroFinder::Function1D
{
	public:
		CZF( double &a,  double &b):Function1D(a,b) {}
		double operator()( double &x) 
		{
			return (p1*Math.Tan(x) - log(Math.Tan(0.5d*x + Math.PI/4)) - p2);
		}
};
#endif
    public class Astro_Functions
{ 
double Mean2Eccentric (ref double M,  ref double e)
{

	double tolerance = 1e-13;
	int n_of_it = 0; // Number of iterations
	double Eccentric,Ecc_New;
	double err = 1.0;



	if (e < 1.0) {
		Eccentric = M + e* Math.Cos(M); // Initial guess
		while ( (err > tolerance) && (n_of_it < 100))
		{
			Ecc_New = Eccentric - (Eccentric - e * Math.Sin(Eccentric) -M )/(1.0 - e * Math.Cos(Eccentric));
			err = Math.Abs(Eccentric - Ecc_New);
			Eccentric = Ecc_New;
			n_of_it++;
		}
	} else {
/*TODO fix this
            CZF FF(e,M);  // function to find its zero point
		ZeroFinder::FZero fz(-M_PI_2 + 1e-8, M_PI_2 - 1e-8);
		Ecc_New = fz.FindZero(FF);
		Eccentric = Ecc_New;
        */
	}
        //TODO swap this
        return tolerance;
	//return Eccentric;
}



void Conversion (ref double[] E,
		ref double[] pos,
		ref double[] vel,
		 ref double mu)
{
	double a,e,i,omg,omp,theta;
	double b,n;
    double[] X_per = new double[3];
    double[] X_dotper = new double[3];
    double[,] R = new double[3,3];

	a = E[0];
	e = E[1];
	i = E[2];
	omg = E[3];
	omp = E[4];
	theta = E[5];

	b = a * Math.Sqrt (1 - e*e);
	n = Math.Sqrt (mu/Math.Pow(a,3));

	 double sin_theta = Math.Sin(theta);
	 double cos_theta = Math.Cos(theta);

	X_per[0] = a * (cos_theta - e);
	X_per[1] = b * sin_theta;

	X_dotper[0] = -(a * n * sin_theta)/(1 - e * cos_theta);
	X_dotper[1] = (b * n * cos_theta)/(1 - e * cos_theta);

	 double cosomg = Math.Cos(omg);
	 double cosomp = Math.Cos(omp);
	 double sinomg = Math.Sin(omg);
	 double sinomp = Math.Sin(omp);
	 double cosi = Math.Cos(i);
	 double sini = Math.Sin(i);

	R[0,0] =  cosomg*cosomp-sinomg*sinomp*cosi;
	R[0,1] =  -cosomg*sinomp-sinomg*cosomp*cosi;

	R[1,0] =  sinomg*cosomp+cosomg*sinomp*cosi;
	R[1,1] =  -sinomg*sinomp+cosomg*cosomp*cosi;

	R[2,0] =  sinomp*sini;
	R[2,1] =  cosomp*sini;

	// evaluate position and velocity
	for (int ii = 0;ii < 3;ii++)
	{
		pos[ii] = 0;
		vel[ii] = 0;
		for (int j = 0;j < 2;j++)
		{
			pos[ii] += R[ii,j] * X_per[j];
			vel[ii] += R[ii,j] * X_dotper[j];
		}
	}
	return;
}


double norm(ref double[] vet1, ref double[] vet2)
{
	double Vin = 0;
	for (int i = 0; i < 3; i++)
	{
		Vin += (vet1[i] - vet2[i])*(vet1[i] - vet2[i]);
	}
	return Math.Sqrt(Vin);
}


double norm2(ref double[] vet1)
{
	double temp = 0.0;
	for (int i = 0; i < 3; i++) {
		temp += vet1[i] * vet1[i];
	}
	return Math.Sqrt(temp);
}


//subfunction that evaluates vector product
void vett(ref double[] vet1, ref double[] vet2,ref double[] prod )
{
	prod[0]=(vet1[1]*vet2[2]-vet1[2]*vet2[1]);
	prod[1]=(vet1[2]*vet2[0]-vet1[0]*vet2[2]);
	prod[2]=(vet1[0]*vet2[1]-vet1[1]*vet2[0]);
}

double Acosh(double x)
    {
        return Math.Log(x + Math.Sqrt(x * x - 1));
    }
double Asinh(double x)
    {
        return Math.Log(x + Math.Sqrt(x * x + 1));
    }
double x2tof(ref double x, ref double s, ref double c, ref bool lw)
{
	double am,a,alfa,beta;

	am = s/2;
	a = am/(1-x*x);
	if (x < 1)//ellpise
	{
		beta = 2 * Math.Asin (Math.Sqrt((s - c)/(2*a)));
		if (lw) beta = -beta;
		alfa = 2 * Math.Acos(x);
	}
	else
	{
		alfa = 2 * Acosh(x);
		beta = 2 * Asinh(Math.Sqrt ((s - c)/(-2 * a)));
		if (lw) beta = -beta;
	}

	if (a > 0)
	{
		return (a * Math.Sqrt (a)* ( (alfa - Math.Sin(alfa)) - (beta - Math.Sin(beta)) ));
	}
	else
	{
		return (-a * Math.Sqrt(-a)*( (Math.Sinh(alfa) - alfa) - ( Math.Sinh(beta) - beta)) );
	}

}


// Subfunction that evaluates the time of flight as a function of x
double tofabn(ref double sigma,ref double alfa,ref double beta)
{
	if (sigma > 0)
	{
		return (sigma * Math.Sqrt (sigma)* ( (alfa - Math.Sin(alfa)) - (beta - Math.Sin(beta)) ));
	}
	else
	{
		return (-sigma * Math.Sqrt(-sigma)*( (Math.Sinh(alfa) - alfa) - ( Math.Sinh(beta) - beta)) );
	}
}

// subfunction that evaluates unit vectors
void vers( ref double[] V_in, ref double[] Ver_out)
{
	double v_mod = 0;
	int i;

	for (i = 0;i < 3;i++)
	{
		v_mod += V_in[i]*V_in[i];
	}

	double sqrtv_mod = Math.Sqrt(v_mod);

	for (i = 0;i < 3;i++)
	{
		Ver_out[i] = V_in[i]/sqrtv_mod;
	}
}


}

