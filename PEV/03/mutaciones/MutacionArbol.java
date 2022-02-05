package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.IndividuoArboreo;
import algoritmoGenetico.tree.Nodo;
import algoritmoGenetico.tree.Operando;

public class MutacionArbol extends Mutacion {
	public Vector<IndividuoArboreo> Mutacion(Vector<IndividuoArboreo> pob, double probMut){
		Random r=new Random();

		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			IndividuoArboreo indv = pob.elementAt(i);
			if(r.nextDouble()<probMut
					&& indv.cromosoma.numNodos > 3) {	//nos aseguramos antes de entrar a la mutacion que el individuo no sea tan solo un elemento terminal por si solo
				
				//buscamos un subarbol aleatorio
				Nodo nodoActual;

				do {
					nodoActual = indv.cromosoma.buscaNodo(r.nextInt(indv.cromosoma.numNodos));							
				} while(nodoActual.valor == Operando.AVANZA || nodoActual.valor == Operando.DERECHA || nodoActual.valor == Operando.IZQUIERDA);
				
				if(nodoActual == indv.cromosoma.raiz) continue;

				copiaNodo(nodoActual, indv.cromosoma.completa(nodoActual.padre)); //creamos un arbol aleatorio y se lo asignamos a nodoactual	
				
				//indv.cromosoma.recalculaNodos_Prof();;
				indv.cromosoma.numNodos = cuentaNodos(indv.cromosoma.raiz);
				
				indv.setNuevo(true);			
			}
		}
		
		return pob;
	}
	
	private int cuentaNodos(Nodo nodo) {
		
		int cont = 0;
		
		if(nodo.hijoIzquierdo != null) cont += cuentaNodos(nodo.hijoIzquierdo) + 1;
		if(nodo.hijoCentral != null) cont += cuentaNodos(nodo.hijoCentral) + 1;
		if(nodo.hijoDerecho != null) cont += cuentaNodos(nodo.hijoDerecho) + 1;
		
		return cont;
	}

	private void copiaNodo(Nodo nodoActual, Nodo nodoNuevo) {
		nodoActual.valor = nodoNuevo.valor;
		nodoActual.hijoIzquierdo = nodoNuevo.hijoIzquierdo;
		nodoActual.hijoDerecho = nodoNuevo.hijoDerecho;
		nodoActual.hijoCentral = nodoNuevo.hijoCentral;
		nodoActual.profundidad = nodoNuevo.profundidad;
	}

}
