package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.IndividuoArboreo;
import algoritmoGenetico.tree.Nodo;
import algoritmoGenetico.tree.Operando;

public class MutacionTerminalSimple extends Mutacion {
	public Vector<IndividuoArboreo> Mutacion(Vector<IndividuoArboreo> pob, double probMut){
		Random r=new Random();
		
		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			if(r.nextDouble()<probMut) {				
				IndividuoArboreo indv = pob.elementAt(i);
				
				//pasamos a buscar un nodo terminar aleatorio del individuo
				Boolean encontrado = false;
				Nodo nodoActual = indv.cromosoma.raiz;
				
				while(!encontrado) {
					if(nodoActual.valor == Operando.PROGN3) nodoActual = buscaElementoTerminal(nodoActual, 3, r);
					else if(nodoActual.valor == Operando.PROGN2) nodoActual = buscaElementoTerminal(nodoActual, 2, r);
					else if(nodoActual.valor == Operando.SICOMIDA) nodoActual = buscaElementoTerminal(nodoActual, 2, r);
					else {
						encontrado = true;
					}
				}
								
				//en la variable nodoActual tenemos una referencia a un elemento terminal
				Operando aux = nodoActual.valor;
				do {
					nodoActual.valor = indv.cromosoma.randomTerminal();
				} while (nodoActual.valor == aux); //alomejor esto peta por cosas de referencia¿?¿?		
				indv.setNuevo(true);
			}						
		}
	
		return pob;
	}
	
	private Nodo buscaElementoTerminal(Nodo nodo, int numHijos, Random r) {
		int hijo = r.nextInt(numHijos);
		
		if(numHijos == 2) {
			if(hijo == 0) return nodo.hijoIzquierdo;
			else return nodo.hijoDerecho;
		}
		else { //es 3
			if(hijo == 0) return nodo.hijoIzquierdo;
			else if (hijo == 1) return nodo.hijoCentral;
			else return nodo.hijoDerecho;
		}
	}
}
