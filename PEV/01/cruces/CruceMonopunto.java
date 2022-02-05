package algoritmoGenetico.cruces;

import java.util.Collection;
import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;



//escoge los individuos a cruzar dependiendo de la probabilidad de cruce
//escoge un punto aleatorio del cromosoma e intercambia a partir de ahi el resto de cromosoma
public class CruceMonopunto extends Cruce{
	public  Vector<Individuo> cruce(Vector<Individuo> pob, double probCruce) {
		Vector<Individuo> pobCruzar= new Vector<Individuo>();//individuos a cruzar
		Vector<Integer> index = new Vector<Integer>();//guarda la posicion de la poblacion de cada indv a cruzar		
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

			Individuo ind1=pobCruzar.elementAt(0).copia();
			Individuo ind2=pobCruzar.elementAt(1).copia();

			cruza(ind1,ind2);//aqui ya tenemos los dos nuevos individuos cruzados
			
			//actualizamos fenotipo y fitness
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
	//escoge el punto donde cruzar los individuos y crea dos nuevos individuos cruzando a partir de dicho punto
	private void cruza(Individuo ind1, Individuo ind2) {
		int rand=0+ (int)(Math.random() * ((ind1.getCromosoma().size() +0) + 1));//genera un random entre 0 y el numero de bits
		Individuo hijo1 = ind1, hijo2=ind2;
		var aux = hijo1.getCromosoma();//variable en la que se va a copiar el individuo1, ind2 es una copia por referencia
		aux = new Vector<Object>();
		aux.addAll(hijo1.getCromosoma());


		for(int i=rand;i<ind1.getCromosoma().size();i++) {

			aux.setElementAt(ind2.getCromosoma().elementAt(i), i);
			hijo2.getCromosoma().setElementAt(ind1.getCromosoma().elementAt(i), i);


		}
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			hijo1.getCromosoma().setElementAt(aux.elementAt(i), i);
		}

	}



}
