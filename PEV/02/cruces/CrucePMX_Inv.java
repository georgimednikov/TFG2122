package algoritmoGenetico.cruces;

import java.util.Random;
import java.util.Vector;
import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoEntero;

//El cruce por emparejamiento parcial inverso (PMX_Inv) cruza dos individuos seleccionando dos puntos de cruce aleatorios 
//del cromosoma e intercambia los genes que no estan entre esos puntos. Es el mismo cruce que el PMX pero al reves, en vez de 
//intercambiar los genes entre los puntos, intercambia los que no estan entre esos puntos
//Para los genes que estan entre los dos puntos se intercambian con el homologo. 

public class CrucePMX_Inv extends Cruce{
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
	//crea dos random que son los puntos donde intercambiar genes de ambos individuos
	// y luego los demas no los modifica a no ser que esten entre dichos puntos
	private void cruza(IndividuoEntero ind1, IndividuoEntero ind2) {
		//generamos los dos puntos randoms de cruce
		int rand=0+ (int)(Math.random() * ((ind1.getCromosoma().size() +0) ));//genera un random entre 0 y el numero de genes
		int rand2=0+ (int)(Math.random() * ((ind1.getCromosoma().size() +0) ));//genera un random entre 0 y el numero de genes
		
		if(rand==rand2)
			return;		
		
		//hace que siempre sea menor rand que rand2
		if(rand > rand2) {
			int temp=rand2;
			rand2=rand;
			rand=temp;
		}
		if(rand2==26)
			System.out.println();
		Vector<Integer> aux = new Vector<Integer>();//variable en la que se va a copiar el individuo1
		Vector<Integer> aux2 = new Vector<Integer>();//variable en la que se va a copiar el individuo2
		aux.addAll(ind1.getCromosoma());
		aux2.addAll(ind2.getCromosoma());
		
		//vectores donde se van a guardar los genes intercambiados si no estan repetidos, 
		//-1 == repetido y al final se quitaran los -1 por su homologo 
		Vector<Integer> cromAux = new Vector<Integer>();
		Vector<Integer> cromAux2 = new Vector<Integer>();
		for(int i=0;i<aux.size();++i) {
			cromAux.add(-1);
			cromAux2.add(-1);
		}
		
		//vectores que guardan las posiciones de los elementos que hay que intercambiar despues
		Vector<Integer> lista = new Vector<Integer>();
		Vector<Integer> lista2 = new Vector<Integer>();
		
		
		//intercambia los genes que no esten entre los dos puntos
		for(int i=0;i<aux.size();i++) {
			if(i==rand) i=rand2;
			int temp=aux.elementAt(i);			
			cromAux.setElementAt(aux2.elementAt(i), i);
			cromAux2.setElementAt(temp , i);
		}
		
		//volver a poner si no esta ya puesto
		int k=rand;
		while(k!=rand2) {
			//si esta repetido el gen en el ind1 se añade a la lista para buscar su homologo en ind2
			if(!comprueba(aux.elementAt(k),cromAux,rand,rand2)) {				
				lista.add(k);
			}
			else {
				cromAux.setElementAt(aux.elementAt(k), k);
			}
			//si esta repetido el gen en el ind2 se añade a la lista para buscar su homologo en ind1
			if(!comprueba(aux2.elementAt(k),cromAux2,rand,rand2)) {				
				lista2.add(k);
			}
			else {
				cromAux2.setElementAt(aux2.elementAt(k), k);
			}
			
			k++;
		}
		
		//acaba el cromosoma 1 con los elementos que estaban repetidos
		for(int i=0;i<lista.size();++i) {
			int temp=encuentraHomologo(cromAux2,cromAux,aux.elementAt(lista.elementAt(i)),lista.elementAt(i));
			cromAux.setElementAt(temp, lista.elementAt(i));
		}
		
		//acaba el cromosoma 2 con los elementos que estaban repetidos
		for(int i=0;i<lista2.size();++i) {
			int temp=encuentraHomologo(cromAux,cromAux2,aux2.elementAt(lista2.elementAt(i)),lista2.elementAt(i));
			cromAux2.setElementAt(temp, lista2.elementAt(i));
		}
		
		//pone el nuevo cromosoma a los individuos
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			ind1.getCromosoma().setElementAt(cromAux.elementAt(i), i);
			ind2.getCromosoma().setElementAt(cromAux2.elementAt(i), i);
		}
		
	}

	//devuelve true si el valor a buscar NO esta entre 0 y el punto1 y el punto2 y el fin de v
	protected boolean comprueba(int num, Vector<Integer> v, int punto1, int punto2) {
		for(int i=0;i<punto1;++i) {
			if(v.elementAt(i)==num) {
				return false;
			}
		}
		for(int i=punto2;i<v.size();++i) {
			if(v.elementAt(i)==num) {
				return false;
			}
		}
		return true;
	}
	
	//encuentra el elemento homologo, puede ser que el homologo ya este usado
	//por eso hace falta realizar una secuencia hasta encontrar el que no se repita
	protected int encuentraHomologo(Vector<Integer> v1,Vector<Integer> v2, int elem, int pos) {
		
		int temp=v2.indexOf(elem);
		elem=v1.elementAt(temp);
		for(int i=0;i<v1.size();++i) {
			//if(comprueba(elem,v2,0,v1.size()))
			if(!v2.contains(elem))
				return elem;
			else {
				temp=v2.indexOf(elem);
				elem=v1.elementAt(temp);
			}
		}		
		return elem;
	}
	
	
}