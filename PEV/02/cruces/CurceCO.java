package algoritmoGenetico.cruces;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoEntero;

/*cruza los elementos de dos cromosomas ordenandolos primero de forma ordinal
luego realizando un cruce monopunto y por ultimo deshaciendo la ordenacion
Para generar el cromosoma ordenado mira su gen y busca la posicion que ocupa en la lista
y lo borra de la lista. En el ejemplo el 0 esta en la pos 0 y que la lista 1 2 3 4
luego el siguiente gen es el 1 que ahora ocupa la pos 0 de la lista y la lista queda 2 3 4...
Y despues de realizar el cruce monopunto se deshace esta ordenacion haciendo lo mismo
EJEMPLO: 
CROMOSOMA		LISTA			CROMOSOMA ORDENADO	CRUCE MONOPUNTO		LISTA 	CROMOSOMA FINAL
 				0 1 2 3 4											0 1 2 3 4
 0 1 4 2 3						0 0 2 0 0			0 0 0 0 0					0 1 2 3 4
 				0 1 2 3 4											0 1 2 3 4
 0 2 1 3 4						0 1 0 0 0			0 1 2 0 0					0 2 4 0 0
*/

//Cruce Ordinal
public class CurceCO extends Cruce{
	public  Vector<Individuo> cruce(Vector<Individuo> pob, double probCruce) {
		Vector<Individuo> pobCruzar= new Vector<Individuo>();//individuos a cruzar
		Vector<Integer> index = new Vector<Integer>();//guarda la posicion en la poblacion de cada indv a cruzar		
		Random r=new Random();
		//seleccionamos los individuos a cruzar
		for(int i=0;i<pob.size();i++) {
			if(r.nextDouble()<probCruce) {
				index.add(i);
				pobCruzar.add(pob.elementAt(i));
			}
		}
		//cruzamos mientras queden 2 individuos por cruzar o mas		
		while(pobCruzar.size()>=2) {

			IndividuoEntero ind1=(IndividuoEntero)pobCruzar.elementAt(0).copia();
			IndividuoEntero ind2=(IndividuoEntero)pobCruzar.elementAt(1).copia();

			cruza(ind1,ind2);//aqui ya tenemos los dos nuevos individuos cruzados
			
			//actualizar fenotipos y fitness
			ind1.setFenotipo();
			ind1.calculateFitness();
			ind2.setFenotipo();
			ind2.calculateFitness();
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
	//ordena los cromosomas de manera ordinal, los cruza usando el cruce monopunto 
	//y despues invierte la ordenacion ordinal dando lugar a dos cromosomas cruzados
	private void cruza(IndividuoEntero ind1, IndividuoEntero ind2) {
		
		//Individuo hijo1 = ind1, hijo2=ind2;

		//vectores donde vamos a guardar el cromosoma de manera ordinal y realizar el cruce monopunto
		Vector<Integer> aux = new Vector<Integer>();
		Vector<Integer> aux2 = new Vector<Integer>();
		
		//vectores donde vamos a guardar los cromosomas cruzados
		Vector<Integer> cromAux = new Vector<Integer>();
		Vector<Integer> cromAux2 = new Vector<Integer>();
		
		//vectores para ayudarnos a guardar los cromosomas ordenados de manera ordinal
		Vector<Integer> lista = new Vector<Integer>();//vector que contiene el orden de los genes que se cogen en el ind1
		Vector<Integer> lista2 = new Vector<Integer>();//vector que contiene el orden de los genes que se cogen en el ind2
		
		for(int i=0;i<ind1.getCromosoma().size();++i) {
			lista.add(i);
			lista2.add(i);
		}
		
		//se ordena de manera ordinal los cromosomas
		for(int i=0;i<ind1.getCromosoma().size();++i) {
			
			int temp=lista.indexOf(ind1.getCromosoma().elementAt(i));//aumentamos en uno las posiciones
			aux.add(temp);
			lista.remove(temp);//eliminamos de la lista el que hemos usado
			temp=lista2.indexOf(ind2.getCromosoma().elementAt(i));//aumentamos en uno las posiciones
			aux2.add(temp);
			lista2.remove(temp);//eliminamos de la lista el que hemos usado
		}
		
		//los cruzamos usando monopunto 
		cruzaMonopunto(aux,aux2);
		
		//llenamos otra vez los vectores de la lista
		for(int i=0;i<ind1.getCromosoma().size();++i) {
			lista.add(i);
			lista2.add(i);
		}			
		//una vez cruzado los elementos usando monocruce
		//deshacemos la ordenacion que habiamos hecho previamente
		for(int i=0;i<ind1.getCromosoma().size();++i) {
			
			//individuo 1
			int temp=lista.elementAt(aux.elementAt(i));//aumentamos en uno las posiciones			
			cromAux.add(temp);
			lista.removeElement(temp);//eliminamos de la lista el que hemos usado
			
			//individuo2
			temp=lista2.elementAt(aux2.elementAt(i));//aumentamos en uno las posiciones
			cromAux2.add(temp);
			lista2.removeElement(temp);//eliminamos de la lista el que hemos usado
		}		
		//pone el nuevo cromosoma a los individuos
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			ind1.getCromosoma().setElementAt(cromAux.elementAt(i), i);
			ind2.getCromosoma().setElementAt(cromAux2.elementAt(i), i);
		}
	}

	
	//cruce monopunto
	private void cruzaMonopunto(Vector<Integer> ind1, Vector<Integer> ind2) {
		int rand=0+ (int)(Math.random() * ((ind1.size() +0) ));//genera un random entre 0 y el numero de genes
		Vector<Integer>aux = new Vector<Integer>();
		aux.addAll(ind1);

		//cruza los genes a partir del punto
		for(int i=rand;i<ind1.size();i++) {
			aux.setElementAt(ind2.elementAt(i), i);
			ind2.setElementAt(ind1.elementAt(i), i);
		}
		
		//pone ind1 con los valores de aux(el cromosoma del ind1 ya cruzado)
		for(int i=0;i<ind1.size();i++) {
			ind1.setElementAt(aux.elementAt(i), i);
		}

	}

}