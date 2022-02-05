package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.IndividuoArboreo;
import algoritmoGenetico.tree.Nodo;
import algoritmoGenetico.tree.Operando;

public class MutacionFuncionalSimple extends Mutacion {
	public Vector<IndividuoArboreo> Mutacion(Vector<IndividuoArboreo> pob, double probMut){
		Random r=new Random();
		
		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			IndividuoArboreo indv = pob.elementAt(i);
			if(r.nextDouble()<probMut
					&& indv.cromosoma.numNodos > 2) {	//nos aseguramos antes de entrar a la mutacion que el individuo no sea tan solo un elemento terminal por si solo
				
				//buscamos un operador no terminal aleatorio
				Nodo nodoActual;
				
				do {
					nodoActual = indv.cromosoma.buscaNodo(r.nextInt(indv.cromosoma.numNodos));
				} while(nodoActual.valor == Operando.AVANZA || nodoActual.valor == Operando.DERECHA || nodoActual.valor == Operando.IZQUIERDA);
				
				//cambair el valor del nodo actual
				if(nodoActual.valor == Operando.PROGN3) continue; //no tenemos otro operando que tome 3 argumentos
				//intercambiamos los dos valores
				else if(nodoActual.valor == Operando.PROGN2) nodoActual.valor = Operando.SICOMIDA;
				else if(nodoActual.valor == Operando.SICOMIDA) nodoActual.valor = Operando.PROGN2;
			}
			indv.setNuevo(true);
		}
		
		
		return pob;
	}
}
