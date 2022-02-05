package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoCifrado;

public class MutacionHeuristica extends Mutacion {
	public Vector<Individuo> Mutacion(Vector<Individuo> pob, double probMut){
		Random r=new Random();		

		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			if(r.nextDouble()<probMut) { //efectuamos una mutacion
				//sacamos el individuo
				IndividuoCifrado individuo = (IndividuoCifrado)pob.elementAt(i);
				
				
				Vector<Integer> cromosoma = individuo.getCromosoma();
				
				//sacamos cuantos cromosomas queremos elegir para hacer la mutacion
				int variaciones = r.nextInt(5); //si no tarda mucho en crear todas las opciones
				Vector<Integer> posiciones = new Vector<Integer>();
				
				//sacamos las posiciones de los cromosomas que vamos a mutar
				for(int j=0;j<variaciones;j++) {
					int pos;
					do{
						pos = r.nextInt(cromosoma.size());
					}
					while(posicionRepetida(posiciones, pos)); //nos aseguramos que ese valor no esté ya seleccionado
					posiciones.add(pos);
				}				
				//generamos todos los individuos y nos vamos quedando con el mejor hasta el momento
				generarSoluciones(posiciones, individuo);				
			}		
		}		
		return pob;
	}

	
	private void generarSoluciones(Vector<Integer> posiciones, IndividuoCifrado individuo) {
		//sacamos los valores del cromosoma en cada una de esas posiciones
		Vector<Integer> cromosoma = individuo.getCromosoma();
		Vector<Integer> valores = new Vector<Integer>(); //contendra todos los valores del cromosoma de las posiciones dadas por el vector "posiciones"
		
		//en el vector "posiciones" guardamos las posiciones del cromosoma donde se van a poner los valores del vector "valores"
		for(int i = 0; i < posiciones.size(); i++) {
			valores.add(Integer.valueOf(cromosoma.elementAt(posiciones.elementAt(i))));
		}
				
		//la mejor solucion que luego pasara a ser el individuo elegido
		IndividuoCifrado mejorInd = individuo;
		
		//sacamos todas las posibles combinaciones
		Vector<Vector<Integer>> ordenaciones = new Vector<Vector<Integer>>();
		Vector<Boolean> usado = new Vector<Boolean>(valores.size());
		for(int i=0; i<valores.size();i++) usado.add(false);
		
		Vector<Integer> aux = new Vector<Integer>();
		for(int i=0;i<valores.size();i++) aux.add(i);
		combinacionRec(ordenaciones, valores, 0, usado, aux);
		
		//recorremos todas las combinaciones y no quedamos con la mejor
		for(Vector<Integer> combinacion:ordenaciones) {
			IndividuoCifrado ind = (IndividuoCifrado)individuo.copia();
			
			Vector<Integer> cromosomaInd = ind.getCromosoma();
			Vector<Double> fenotipoInd = ind.getFenotipo();
			
			//cambiar el cromosoma y el fenotipo del "ind" y aplicar el getFitness()
			for(int i=0;i<valores.size();i++) {
				cromosoma.setElementAt(combinacion.elementAt(i), posiciones.elementAt(i));
				fenotipoInd.setElementAt(Double.valueOf(combinacion.elementAt(i)), posiciones.elementAt(i));
			}
			
			
			//sacamos el fitness
			ind.calculateFitness();
			//nos quedamos con el que tenaga el mejor fitness
			if(ind.getFitness() < mejorInd.getFitness()) mejorInd = ind;
		}		
		individuo = mejorInd;
	}
	
	private void combinacionRec(Vector<Vector<Integer>> ordenaciones, Vector<Integer> valores, 
			Integer pos, Vector<Boolean> usado, Vector<Integer> aux){		
		if(pos == valores.size()) {
			ordenaciones.add(new Vector<Integer>(aux)); //lo añadimos a la "base de datos" asegurandonos de que no hay problemas de referencias
		}
		else {
			for(int i=0; i < valores.size();i++) {
				if(!usado.elementAt(i)) {
					usado.setElementAt(true, i);
					aux.setElementAt(Integer.valueOf(valores.elementAt(pos)), i);
					combinacionRec(ordenaciones, valores, (pos+1), usado, aux);
					usado.setElementAt(false, i);
				}
			}		
		}
	}
	
	
	private Boolean posicionRepetida(Vector<Integer> posiciones, Integer pos) {
		for(Integer p:posiciones) {
			if(p == pos) return true; //hemos encontrado uno igual por tanto no compensa seguir buscando
		}
		return false; //no hemos encontrado esa posicion en el valor
	}
}
