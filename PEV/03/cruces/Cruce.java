package algoritmoGenetico.cruces;

import java.util.Vector;
import algoritmoGenetico.individuos.IndividuoArboreo;

//clase abstracta de cruce
public abstract class Cruce {
	public abstract Vector<IndividuoArboreo> cruce(Vector<IndividuoArboreo> pob, double probcurce); 
} 