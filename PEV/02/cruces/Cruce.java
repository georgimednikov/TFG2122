package algoritmoGenetico.cruces;

import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
//clase abstracta de cruce
public abstract class Cruce {
	public abstract Vector<Individuo> cruce(Vector<Individuo> pob, double probcurce); 
} 