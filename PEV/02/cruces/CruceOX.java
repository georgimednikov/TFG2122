package algoritmoGenetico.cruces;
import java.util.Random;
import java.util.Vector;


import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoEntero;

//El cruce orden (OX) cruza dos individuos seleccionando dos puntos de cruce aleatorios 
//del cromosoma e intercambia los genes que hay entre esos puntos. Para el resto de genes pone los mismos del cromosoma si no se repiten,.
//se pueden repetir con los genes intercambiados. Se ponen en orden empezando desde el segundo punto del cruce, si se repite se ignora 
// y se pone el siguiente gen, hasta encontrar uno no repetido. Si no se ha rellenado todo el cromosoma (alguno habia repetido)
// se busca los genes que queden por poner en los intercambiados previamente sin que quede ninguno repetido

public class CruceOX extends Cruce{
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
		int rand=0+ (int)(Math.random() * ((ind1.getCromosoma().size() +0) + 1));//genera un random entre 0 y el numero de genes
		int rand2=0+ (int)(Math.random() * ((ind1.getCromosoma().size() +0) + 1));//genera un random entre 0 y el numero de genes

		//solo hacer intercambios si los randoms son diferentes
		if(rand==rand2) 
			return;
		
		//hace que siempre sea menor rand que rand2
		if(rand > rand2) {
			int temp=rand2;
			rand2=rand;
			rand=temp;
		}
				
		Individuo hijo1 = ind1, hijo2=ind2;

		Vector<Integer> aux = new Vector<Integer>();//variable en la que se va a copiar el individuo1
		Vector<Integer> aux2 = new Vector<Integer>();//variable en la que se va a copiar el individuo2
		
		//ponemos los cromosomas pero la pos 0 ahora es la pos del primer punto de cruce
		//lo hacemos para tenerlo ya ordenado y que sea mas facil la busqueda del elemento no repetido
		for(int i=rand;i<rand2;++i) {
			aux.add(ind1.getCromosoma().elementAt(i));
			aux2.add(ind2.getCromosoma().elementAt(i));
		}
		int k=rand2;
		k%=ind1.getCromosoma().size();
		while(k!=rand) {
			aux.add(ind1.getCromosoma().elementAt(k));
			aux2.add(ind2.getCromosoma().elementAt(k));
			k++;
			k%=ind1.getCromosoma().size();
		}
		//ya tenemos los dos ordenados
		
		
		//vectores donde se van a guardar los genes intercambiados si no estan repetidos, 
		//-1 == repetido y al final se quitaran los -1 por su homologo 
		Vector<Integer> cromAux = new Vector<Integer>();
		Vector<Integer> cromAux2 = new Vector<Integer>();
		for(int i=0;i<aux.size();++i) {
			cromAux.add(-1);
			cromAux2.add(-1);
		}
		
		//intercambia los genes entre los dos puntos
		for(int i=0;i<rand2-rand;i++) {
			int temp=aux.elementAt(i);			
			cromAux.setElementAt(aux2.elementAt(i), i);
			cromAux2.setElementAt(temp , i);
		}
		
		//aqui rellenamos con los elementos que no estaban entre los dos puntos
		for(int i=rand2-rand; i<aux.size();++i) {
			int temp=encuentra(cromAux, aux.elementAt(i),i,rand2-rand,aux);
			cromAux.setElementAt(temp,i);
			
			temp=encuentra(cromAux2, aux2.elementAt(i),i,rand2-rand,aux2);
			cromAux2.setElementAt(temp,i);
		}
					
		//pone el nuevo cromosoma a los individuos
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			hijo1.getCromosoma().setElementAt(aux.elementAt(i), i);
			hijo2.getCromosoma().setElementAt(aux2.elementAt(i), i);
		}
		
	}

	//devuelve el siguiente elemento a poner, se asegura de que no se repita.
	//Hace uso del vector que se esta rellenando y del original ordenado
	protected int encuentra( Vector<Integer> v, int elem,int fin, int punto2, Vector<Integer> original) {
		for(int i=fin; i< v.size();++i) {
			if(comprueba(elem,v,0,fin)) {
				return elem;
			}			
			elem = v.elementAt(i);	
			if(elem==-1)
				elem=original.elementAt(i);
		}
		for(int i=0; i< punto2; ++i) {
			elem=original.elementAt(i);
			if(comprueba(elem,v,0,fin)) {
				return elem;
			}			
			elem = v.elementAt(i);	
		}		
		return 1;
	}
	
	
	//devuelve true si el valor a buscar NO esta entre los dos puntos expecificados en el vector
	protected boolean comprueba(int num, Vector<Integer> v, int punto1, int punto2) {
		for(int i=punto1;i<punto2;i++) {
			if(v.elementAt(i)==num) {
				return false;
			}
		}
		return true;
	}		
}