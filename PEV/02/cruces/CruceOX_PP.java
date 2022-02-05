package algoritmoGenetico.cruces;
import java.util.Random;
import java.util.Vector;


import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoEntero;

//El cruce orden con posiciones prioritarias (OX-PP) intercambia dos genes aleatorios de los individios
//y pone el resto de genes en el mismo orden sin repetir ninguno. Se empieza a poner en orden desde el segundo 
//cromosoma intercambiado. Es un cruce parecido al de orden (es una variante)
public class CruceOX_PP extends Cruce{
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
	//crea dos random que son los genes que se intercambian  ambos individuos
	// y los demas genes los pone en el mismo orden que estaban si no se repite
	private void cruza(IndividuoEntero ind1, IndividuoEntero ind2) {
		//generamos los dos puntos randoms de cruce
		int rand=0+ (int)(Math.random() * ((ind1.getCromosoma().size() +0) ));//genera un random entre 0 y el numero de genes
		int rand2;
		//aseguramos que los dos elementos a cruzar sean distintos para hacer mas util al cruce
		do{
			rand2=0+ (int)(Math.random() * ((ind1.getCromosoma().size() +0) ));//genera un random entre 0 y el numero de genes
		}while(rand==rand2);

		
		//hace que siempre sea menor rand que rand2
		if(rand > rand2) {
			int temp=rand2;
			rand2=rand;
			rand=temp;
		}
				
		Individuo hijo1 = ind1, hijo2=ind2;

		Vector<Integer> aux = new Vector<Integer>();//variable en la que se va a guardar el nuevo cromosoma del individuo1
		Vector<Integer> aux2 = new Vector<Integer>();//variable en la que se va a guardar el nuevo cromosoma del individuo2
		
		
		for(int i=0;i<hijo1.getCromosoma().size();++i) {
			aux.add(-1);
			aux2.add(-1);
		}
		//intercambiar los valores
		int elem1=(Integer)hijo1.getCromosoma().elementAt(rand);
		int elem2=(Integer)hijo1.getCromosoma().elementAt(rand2);
		aux.setElementAt((Integer)hijo2.getCromosoma().elementAt(rand2), rand2);
		aux.setElementAt((Integer)hijo2.getCromosoma().elementAt(rand), rand);
		aux2.setElementAt(elem2, rand2);
		aux2.setElementAt(elem1, rand);
		
	
		//rellenamos los vectores aux con los elementos que les corresponde
		int j=rand2+1;
		j%=aux.size();
		for(int i=0; i<aux.size();++i) {
			if(j!=rand && j != rand2) {
				int temp=encuentra(aux, (Integer)hijo1.getCromosoma().elementAt(j),j,hijo1.getCromosoma());
				aux.setElementAt(temp,j);
				
				temp=encuentra(aux2, (Integer)hijo2.getCromosoma().elementAt(j),j,hijo2.getCromosoma());
				aux2.setElementAt(temp,j);
				
			}
			j++;
			j%=aux.size();
		}
		
		//pone el nuevo cromosoma a los individuos
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			hijo1.getCromosoma().setElementAt(aux.elementAt(i), i);
			hijo2.getCromosoma().setElementAt(aux2.elementAt(i), i);
		}
		
	}

	//devuelve el siguiente elemento a poner, se asegura de que no se repita.
	//Hace uso del vector que se esta rellenando y del original ordenado
	protected int encuentra( Vector<Integer> v, int elem,int pos, Vector<Integer> original) {
		
		for(int i=0; i< v.size();++i) {			
			if(!v.contains(elem))
				return elem;
			pos++;
			pos%=v.size();
			elem=original.elementAt(pos);			
		}
		return 1;
	}
				
}