package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoBinario;

//  crea un random por gen en caso de mutar en el intervalo de la funcion
public class MutacionUniforme extends Mutacion {
	public Vector<Individuo> Mutacion(Vector<Individuo> pob, double probMut) {		
		Random r=new Random();
		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			boolean cambios = false;
			Individuo indv = pob.elementAt(i);
			Vector<Boolean> genes =indv.getCromosoma();
			//recorre cada gen del individuo
			for(int j=0; j<indv.getBegin().size(); j++) {
				if(r.nextDouble()<probMut) {//muta cada gen si el random es menor que la prob de mutar
										
					indv.MutaGen(j);
					
					cambios=true;//marcamos que hay cambios para luego actualizar el fenotipo del gen
				}
			}
			//actualizar fenotipo y fitness si ha habido alguna mutacion
			if(cambios) {
				indv.setFenotipo();
				indv.calculateFitness();
			}
		}
		return pob;
	}
}
