package algoritmoGenetico.cruces;

import java.util.Random;
import java.util.Vector;

import algoritmoGenetico.individuos.Individuo;
import algoritmoGenetico.individuos.IndividuoEntero;

//El cruce por ciclos (CX) cruza dos individuos. El ind1 no intercambia nada con el ind2 hasta que haya un ciclo. 
//Para detectar ciclos el ind1 mira el gen 0 del ind2, y mira en su cromosoma en la posicion de ese gen, si es la misma
//que la anterior hay ciclo y se intercambian los demas genes, sino avanza haciendo lo mismo hasta encontrar un ciclo

//Ejemplo: 		(ind1)pone el 2, el 1 y el 3
//original			//hijos hasta ciclo			//hijos despues ciclo
//2 3 1 4			2 3 1 _						2 3 1 4
//1 2 3 4			1 2 3 _ 					1 2 3 4
//HIJO1: primero guarda su pos 0 (valor2), mira la pos 0 del hijo2 (valor 1), busca la pos del valor 1 (pos2) y lo pone,
//mira la pos2 del ind2 (valor3) y busca la pos del valor 3  en su gen(pos1) y lo pone,
//mira la pos1 del ind2 (valor2) y busca la pos del valor 2  en su gen(pos0), como esta posicion ya ha sido visitada, hay un ciclo
// al haber un ciclo se intercambian el resto de genes que queden. Hacer lo mismo con el ind2
public class CruceCX extends Cruce{
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
	//deja los genes como estan hasta que encuentra un ciclo, entonces intercambia todos los genes que no esten en el ciclo
	private void cruza(IndividuoEntero ind1, IndividuoEntero ind2) {

		Individuo hijo1 = ind1.copia(), hijo2=ind2.copia();

		Vector<Integer> aux = new Vector<Integer>();//variable en la que se va a copiar el individuo1
		Vector<Integer> aux2 = new Vector<Integer>();//variable en la que se va a copiar el individuo2
		//aux.addAll(hijo1.getCromosoma());
		//aux2.addAll(hijo2.getCromosoma());
		
		for(int i=0; i< hijo1.getCromosoma().size();i++) {
			aux.add(-1);
			aux2.add(-1);
		}
		
		boolean sigue=true;
		int k=0;//no haria falta pero es un por si acaso
		int temp1=0,temp2=0;
		//se encarga de intercambiar los genes 
		while(sigue && k < aux.size()) {
			//hijo1
			aux.setElementAt((Integer) hijo1.getCromosoma().elementAt(temp1), temp1);			
			temp1=hijo2.getCromosoma().indexOf(aux.elementAt(temp1));
			if(aux.elementAt(temp1)!=-1)
				sigue=false;
			//hijo2
			aux2.setElementAt((Integer) hijo2.getCromosoma().elementAt(temp2), temp2);
			temp2=hijo1.getCromosoma().indexOf(aux2.elementAt(temp2));
			/*if(aux2.elementAt(temp2)!=-1)
				sigue=false;*/			
			k++;
		}
		//intercambia los genes que no se hayan "visitado" antes
		for(int i=0; i< hijo1.getCromosoma().size();i++) {
			if(aux.elementAt(i) == -1) { 
				aux.setElementAt((Integer)hijo2.getCromosoma().elementAt(i), i);
				aux2.setElementAt((Integer)hijo1.getCromosoma().elementAt(i), i);
			}
		}		
		
		//pone el nuevo cromosoma de los dos individuos
		for(int i=0;i<ind1.getCromosoma().size();i++) {
			ind1.getCromosoma().setElementAt(aux.elementAt(i), i);
			ind2.getCromosoma().setElementAt(aux2.elementAt(i), i);
		}
	}
}