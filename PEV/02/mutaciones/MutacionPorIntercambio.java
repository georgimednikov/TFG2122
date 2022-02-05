package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoCifrado;

public class MutacionPorIntercambio extends Mutacion {	
	public Vector<Individuo> Mutacion(Vector<Individuo> pob, double probMut){
		Random r=new Random();

		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			if(r.nextDouble()<probMut) { //efectuamos una mutacion
				//sacamos el individuo
				IndividuoCifrado individuo = (IndividuoCifrado)pob.elementAt(i);
				
				Vector<Integer> cromosoma = individuo.getCromosoma();
				Vector<Double> fenotipo = individuo.getFenotipo();
				
				//sacamos los dos puntos
				int puntoA = r.nextInt(cromosoma.size());
				int puntoB;
				do{
					puntoB = r.nextInt(cromosoma.size());
				}while (puntoA == puntoB);
				
				//se intercambian los valores tanto al cromosoma como al fenotipo
				int aux = cromosoma.elementAt(puntoA);				
				cromosoma.setElementAt(cromosoma.elementAt(puntoB), puntoA);
				cromosoma.setElementAt(aux, puntoB);

				double auxd = fenotipo.elementAt(puntoA);
				fenotipo.setElementAt(fenotipo.elementAt(puntoA), puntoA);
				fenotipo.setElementAt(auxd, puntoB);
			}
		}

		
		return pob;
	}
}
