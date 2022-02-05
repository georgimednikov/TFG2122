package algoritmoGenetico.mutaciones;

import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
//clase abtracta de mutacion
public abstract class Mutacion {
	public abstract Vector<Individuo> Mutacion(Vector<Individuo> pob, double probMut);
}
