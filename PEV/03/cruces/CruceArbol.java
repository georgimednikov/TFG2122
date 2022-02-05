package algoritmoGenetico.cruces;
import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.IndividuoArboreo;
import algoritmoGenetico.tree.Arbol;
import algoritmoGenetico.tree.Nodo;

public class CruceArbol extends Cruce {
	public  Vector<IndividuoArboreo> cruce(Vector<IndividuoArboreo> pob, double probCruce) {
		Vector<IndividuoArboreo> pobCruzar = new Vector<IndividuoArboreo>();//individuos a cruzar
		Vector<Integer> index = new Vector<Integer>();//guarda la posicion en la poblacion de cada indv a cruzar		
		Random r = new Random();
		//seleccionamos los individuos a cruzar
		for(int i=0; i < pob.size(); i++) {
			if(r.nextDouble() < probCruce) {
				index.add(i);
				pobCruzar.add(pob.elementAt(i));
			}
		}

		//cruzamos mientras queden 2 individuos por cruzar o mas		
		while(pobCruzar.size()>=2) {

			IndividuoArboreo ind1 = pobCruzar.elementAt(0).copia();
			IndividuoArboreo ind2 = pobCruzar.elementAt(1).copia();

			cruza(ind1, ind2);//aqui ya tenemos los dos nuevos individuos cruzados
			
			pob.setElementAt(ind1.copia(), index.elementAt(0));
			pob.setElementAt(ind2.copia(), index.elementAt(1));

			//los quitamos de los individuos a cruzar
			pobCruzar.remove(0);
			pobCruzar.remove(0);
			index.remove(0);
			index.remove(0);
		}

		return pob;
	}


	//se encarga de cruzar como tal dos individuos
	//depende del random generado, combina los genes de los dos individuos en funcion de este random
	private void cruza(IndividuoArboreo ind1, IndividuoArboreo ind2) {
		Arbol arbol1=ind1.cromosoma;
		Arbol arbol2=ind2.cromosoma;
		/*arbol1.print();
		arbol2.print();*/
		
		//elegimos el nodo aleatorio donde cruzar
		int numNodos=Math.min(arbol1.numNodos, arbol2.numNodos);		
		int nodoCruce = (int) Math.floor(Math.random()*(numNodos));	
		
		//intercambia los subarboles en la posicion seleccionada "nodoCruce"
		arbol1.intercambiaSubarboles(arbol2.buscaNodo(nodoCruce), nodoCruce);
		arbol1.recalculaNodos_Prof();
		arbol2.recalculaNodos_Prof();
		/*arbol1.print();
		arbol2.print();*/


	}
}
