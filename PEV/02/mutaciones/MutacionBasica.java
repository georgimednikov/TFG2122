package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoBinario;

//Solo disponible para individuos binarios
//se genera un random por cada bit del cromosoma y si es menor que la probabilidad de mutacion, se muta
//mutar un bit es hacer un random de boolean
public class MutacionBasica extends Mutacion {
	
	public Vector<Individuo> Mutacion(Vector<Individuo> pob, double probMut) {
		
		Random r=new Random();
		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			boolean cambios = false;
			IndividuoBinario indv = (IndividuoBinario) pob.elementAt(i);
			Vector<Boolean> genes = indv.getCromosoma();
			//recorre cada bit del individuo
			for(int j=0; j<genes.size(); j++) {
				if(r.nextDouble()<probMut) {//muta cada bit si el random es menor que la prob de mutar
					genes.setElementAt(r.nextBoolean(), j);
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
