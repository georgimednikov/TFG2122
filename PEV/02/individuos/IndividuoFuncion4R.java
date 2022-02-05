package algoritmoGenetico.individuos;

import java.util.Random;
import java.util.Vector;

public class IndividuoFuncion4R extends IndividuoReal {	
	//necesario para hacer copia de este individuo
	private double precision;
	
	//constructor por copia
	public IndividuoFuncion4R(IndividuoFuncion4R aux) {
		this.fenotipo = aux.fenotipo;
		this.min = aux.min;
		this.max = aux.max;
		this.begin = aux.begin; //0,1,2..
		this.end = aux.end;
		this.cromosoma = aux.cromosoma;
		this.precision = aux.precision;
	}
	
	//constructor por precision y numero de genes
	public IndividuoFuncion4R(double prec, int n) {
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
		generateCromosoma();
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
		IndividuoFuncion4R copiado=new IndividuoFuncion4R(this.precision, this.fenotipo.size());
		for(int i=0; i< begin.size();++i) {
			int a=begin.elementAt(i);
			copiado.begin.setElementAt(a,i);
		}
		for(int i=0; i< end.size();++i) {
			copiado.end.setElementAt(end.elementAt(i),i);
		}
		copiado.cromosoma=new Vector<Double>(cromosoma.size());
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
		copiado.fitness=fitness;
		copiado.fitnessDesp=fitnessDesp;

		return copiado;
	}
}
