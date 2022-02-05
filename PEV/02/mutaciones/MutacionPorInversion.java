package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoCifrado;

public class MutacionPorInversion extends Mutacion {

	public Vector<Individuo> Mutacion(Vector<Individuo> pob, double probMut) {	
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
				
				//intercambiar tanto el cromosoma como el fenotipo y recalcular el fitness
				int inicio = Math.min(puntoA, puntoB);
				int fin = Math.max(puntoA, puntoB);
				
				for(int j=0;j<(fin-inicio)/2;++j) {
					//se intercambian las posiciones del cromosoma
					int aux = cromosoma.elementAt(fin-j);
					cromosoma.setElementAt(cromosoma.elementAt(inicio+j), fin-j);
					cromosoma.setElementAt(aux, inicio+j);

					//se intercambian las posiciones del fenotipo ya que van a corde con el cromomsoma
					double auxd = fenotipo.elementAt(fin-j);
					fenotipo.setElementAt(fenotipo.elementAt(inicio+j), inicio+j);
					fenotipo.setElementAt(auxd, fin-j);
				}
			}
		}
		
		return pob;
	}
}
