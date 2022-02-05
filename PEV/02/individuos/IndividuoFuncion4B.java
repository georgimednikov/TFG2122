package algoritmoGenetico.individuos;

import java.util.Random;
import java.util.Vector;

public class IndividuoFuncion4B extends IndividuoBinario {	
	//necesario para hacer copia de este individuo
	private double precision;
	
	//constructor por copia
	public IndividuoFuncion4B(IndividuoFuncion4B aux) {
		this.fenotipo = aux.fenotipo;
		this.min = aux.min;
		this.max = aux.max;
		this.begin = aux.begin;
		this.end = aux.end;
		this.cromosoma = aux.cromosoma;
		this.precision = aux.precision;
	}
	
	//constructor por precision y numero de genes
	public IndividuoFuncion4B(double prec, int n) {
		this.precision = prec;
		this.fenotipo = new Vector<Double>(n);
		//incializacion de cada gen
		for(int i = 0; i < n; ++i) {
			this.fenotipo.add(0.0);
		}
		this.min = new Vector<Double>(n);
		this.max = new Vector<Double>(n);
		this.begin = new Vector<Integer>(n);
		this.end = new Vector<Integer>(n);
		for(int i = 0; i < n; ++i) {
			this.min.add(0.000); 
			this.max.add(Math.PI);
		}
		
		setSize(this.precision);	
		for(int i = 0; i < this.fenotipo.capacity(); ++i) {
			//para cada gen, generacion random de valores dentro del dominio del mismo
			Random r = new Random();
			for(int j = begin.elementAt(i); j < end.elementAt(i); ++j) {
				double aaa = r.nextDouble();
				if(aaa > 0.5) cromosoma.add(j, true);
				else cromosoma.add(j, false);
			}
		}
		setFenotipo();
	}
	
	public void calculateFitness() {
		//funcion de aptitud
		double res = 0;
		for(int i=1;i<=fenotipo.size();++i) {
			res += Math.sin(fenotipo.elementAt(i-1))*Math.pow(Math.sin(((i+1)*Math.pow(fenotipo.elementAt(i-1), 2)/Math.PI)), 20);
		}		
		this.fitness = -res;
	}
	
	//Hace una copia sin referencia 
	public Individuo copia() {
		IndividuoFuncion4B copiado=new IndividuoFuncion4B(this.precision, this.fenotipo.size());
		for(int i=0; i< begin.size();++i) {
			int a=begin.elementAt(i);
			copiado.begin.add( i,a);
		}
		for(int i=0; i< end.size();++i) {
			copiado.end.add(  i,end.elementAt(i));
		}
		copiado.cromosoma=new Vector<Boolean>(cromosoma.size());
		for(int i=0; i< cromosoma.size();++i) {
			copiado.cromosoma.add(i, cromosoma.elementAt(i));
		}
		for(int i=0; i< fenotipo.size();++i) {
			copiado.fenotipo.setElementAt((Double) fenotipo.elementAt(i), i);
		}
		
		for(int i=0; i< max.size();++i) {
			copiado.max.setElementAt((Double) max.elementAt(i), i);
		}
		for(int i=0; i< min.size();++i) {
			copiado.min.setElementAt((Double) min.elementAt(i), i);
		}	
		copiado.puntuacion=puntuacion;
		copiado.fitnessDesp=fitnessDesp;
		copiado.fitness=fitness;

		return copiado;
	}
}
