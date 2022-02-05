package algoritmoGenetico.mutaciones;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.IndividuoArboreo;
import algoritmoGenetico.tree.Nodo;
import algoritmoGenetico.tree.Operando;

public class MutacionContraccion extends Mutacion{
	public Vector<IndividuoArboreo> Mutacion(Vector<IndividuoArboreo> pob, double probMut){
		Random r=new Random();
		
		//recorre la poblacion
		for(int i=0;i<pob.size();++i) {
			IndividuoArboreo indv = pob.elementAt(i);
			if(r.nextDouble()<probMut
					&& indv.cromosoma.numNodos > 2) {	//nos aseguramos antes de entrar a la mutacion que el individuo no sea tan solo un elemento terminal por si solo
				
				//buscamos un subarbol aleatorio
				Nodo nodoActual;
				
				do {
					nodoActual = indv.cromosoma.buscaNodo(r.nextInt(indv.cromosoma.numNodos));
				} while(nodoActual.valor == Operando.AVANZA || nodoActual.valor == Operando.DERECHA || nodoActual.valor == Operando.IZQUIERDA);
				
				//en nodo actual tenemos un sub-arbol, lo tenemos que sustituir por un nodo terminal				
				nodoActual.valor = indv.cromosoma.randomTerminal();
				nodoActual.setHijosNull();
				
				//recalculamos profundidad para evitar errores
				indv.cromosoma.recalculaNodos_Prof();
				indv.setNuevo(true);
			}
		}
		
		
		return pob;
	}
}