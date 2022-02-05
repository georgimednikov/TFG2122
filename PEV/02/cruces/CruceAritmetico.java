package algoritmoGenetico.cruces;

import java.util.Random;
import java.util.Vector;
import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoReal;

//el cruce aritmetico esta disponible solo para individuos reales
//escoge los individuos a cruzar dependiendo de la probabilidad de cruce
//genera un random de 0 a 1 y se queda con una combinacion del gen de cada individuo
//por ejemplo, si el random es 0.6, coge (0.6*genInd1 + 0.4*genInd2) para el ind 1 y (0.4*genInd1 + 0.6*genInd2) para el ind2
public class CruceAritmetico extends Cruce{
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

			IndividuoReal ind1=(IndividuoReal)pobCruzar.elementAt(0).copia();
			IndividuoReal ind2=(IndividuoReal)pobCruzar.elementAt(1).copia();

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
	//depende del random generado, combina los genes de los dos individuos en funcion de este random
	private void cruza(IndividuoReal ind1, IndividuoReal ind2) {
		double rand=Math.random(); //genera un random entre 0 y 1genes

		Individuo hijo1 = ind1, hijo2=ind2;

		Vector<Double> aux = new Vector<Double>();//variable en la que se va a copiar el individuo1, ind2 es una copia por referencia
		aux.addAll(hijo1.getCromosoma());


		//esto hace todo
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			aux.setElementAt(ind1.getCromosoma().elementAt(i) * rand +
					ind2.getCromosoma().elementAt(i) * (1-rand), i);
			hijo2.getCromosoma().setElementAt(ind1.getCromosoma().elementAt(i) * (1-rand) +
					ind2.getCromosoma().elementAt(i) * rand, i);
		}
		//pone el nuevo cromosoma al ind1
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			hijo1.getCromosoma().setElementAt(aux.elementAt(i), i);
		}
	}



}
