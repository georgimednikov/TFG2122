package algoritmoGenetico.mutaciones;

import java.util.Vector;
import algoritmoGenetico.individuos.IndividuoArboreo;

//clase abtracta de mutacion
public abstract class Mutacion {
	public abstract Vector<IndividuoArboreo> Mutacion(Vector<IndividuoArboreo> pob, double probMut);
}
