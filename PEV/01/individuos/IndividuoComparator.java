package algoritmoGenetico.individuos;

import java.util.Comparator;
//Se ocupa de comparar los individuos
public class IndividuoComparator implements Comparator<Individuo>{
	
	public IndividuoComparator(){
	}
	
	@Override
	public int compare(Individuo o1, Individuo o2) {
		//Compara aptitud desplazada, lo que hace que siempre ordene bien ya se quiera minimizar o maximizar
		return Double.compare(o2.getFitnessD(), o1.getFitnessD());
	}

}
