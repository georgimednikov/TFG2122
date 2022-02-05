package algoritmoGenetico.individuos;

import java.util.Comparator;
//Se ocupa de comparar los individuos
public class IndividuoComparator implements Comparator<IndividuoArboreo>{
	
	public IndividuoComparator(){}
	
	public int compare(IndividuoArboreo o1, IndividuoArboreo o2) {
		//Compara aptitud
		return Double.compare(o2.fitness, o1.fitness);
	}

}
