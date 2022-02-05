package algoritmoGenetico.cruces;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;

//escoge los individuos a cruzar dependiendo de la probabilidad de cruce
//genera un random por cada posicion del cromosoma y decide si intercambiarlo o no
public class CruceUniforme extends Cruce {

	public  Vector<Individuo> cruce(Vector<Individuo> pob, double probCruce) {
		 Vector<Individuo> pobCruzar= new Vector<Individuo>();//individuos a cruzar
		 Vector<Integer> index = new Vector<Integer>();//guarda la posicion de la poblacion de cada indv a cruzar
		Random r=new Random();
		//seleccionamos los individuos a cruzar
		for(int i=0;i<pob.size();i++) {
			if(r.nextDouble()<probCruce) {
				index.add(i);
				pobCruzar.add(pob.elementAt(i).copia());
			}
		}
		
		//cruzamos mientras queden 2 individuos por cruzar o mas		
		while(pobCruzar.size()>=2) {
			
			Individuo ind1=pobCruzar.elementAt(0).copia();
			Individuo ind2=pobCruzar.elementAt(1).copia();
			
			cruza(ind1,ind2);//aqui ya tenemos los dos nuevos individuos cruzados
			
			//actualizamos fitness y fenotipo
			ind1.setFenotipo();
			ind1.calculateFitness();
			ind2.setFenotipo();
			ind2.calculateFitness();
			pob.setElementAt(ind1, index.elementAt(0));
			pob.setElementAt(ind2, index.elementAt(1));
			
			//los quitamos de los individuos a cruzar
			pobCruzar.remove(0);
			pobCruzar.remove(0);
			index.remove(0);
			index.remove(0);
		}
		
		return pob;
	}
	
	
	//se encarga de cruzar como tal dos individuos
	//recorre los bits de cada individuo y decide si intercambiarlos o no
	private void cruza(Individuo ind1, Individuo ind2) {
		double rand;//genera un random entre 0.0 y 1.0
		Individuo hijo1 = ind1, hijo2=ind2;
		Vector<Object> aux = new Vector<Object>();//variable en la que se va a copiar el individuo1, ind2 es una copia por referencia
		aux.addAll(hijo1.getCromosoma());
		
		
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			if((rand=Math.random())>0.5) {//solo intercambiar si es mayor a 0.5
				aux.setElementAt(ind2.getCromosoma().elementAt(i), i);
				hijo2.getCromosoma().setElementAt(ind1.getCromosoma().elementAt(i), i);
			}			
		}
		
		//pone el nuevo cromosoma al ind1
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			hijo1.getCromosoma().setElementAt(aux.elementAt(i), i);
		}
		
	}
			
		
	
}

