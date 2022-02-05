package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoCifrado;

public class MutacionPorInsercion extends Mutacion{
	public Vector<Individuo> Mutacion(Vector<Individuo> pob, double probMut) {		
		Random r=new Random();
		
		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			if(r.nextDouble() < probMut) {
				//sacamos el individuo
				IndividuoCifrado individuo = (IndividuoCifrado)pob.elementAt(i);
				
				Vector<Integer> cromosoma = individuo.getCromosoma();
				Vector<Double> fenotipo = individuo.getFenotipo();
				
				//el punto A es donde se pondra el numero de la posicion B y luego el resto se desplazan
				int puntoA = r.nextInt(cromosoma.size());
				int puntoB;
				do{
					puntoB = r.nextInt(cromosoma.size());
				}while (puntoA == puntoB);


				//desplazamos hacia la derecha hasta llegar al punto B
				int pos = puntoB-1;
				
				//nos guardamos los valores iniciales
				int valIni = cromosoma.elementAt(puntoB);
				double valIniD = fenotipo.elementAt(puntoB);				
									
				while(pos != puntoA-1) {		
					if(pos<0) pos = cromosoma.capacity()-1; //damos la vuelta al vector si nos salimos por la izquierda
										
					if(pos == cromosoma.capacity()-1) { //en caso de que estemos en la ultima posicion
						cromosoma.setElementAt(cromosoma.elementAt(pos), 0);
						fenotipo.setElementAt(fenotipo.elementAt(pos), 0);
					}
					else { //se deplaza hacia la derecha
						cromosoma.setElementAt(cromosoma.elementAt(pos), pos+1);
						fenotipo.setElementAt(fenotipo.elementAt(pos), pos+1);
					}					
					
					pos--; //avanzamos en el vector
				}				
				//ponemos el valor que hemos movido
				cromosoma.setElementAt(valIni, puntoA);
				fenotipo.setElementAt(valIniD, puntoA);				
			}
		}
		return pob;
	}
}
