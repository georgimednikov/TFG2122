package algoritmoGenetico.individuos;

import java.util.Random;
import java.util.Vector;

public class IndividuoFuncion3 extends IndividuoBinario{
	//datos especificos del individuo
	public IndividuoFuncion3(){
		this.fenotipo = new Vector<Double>(2);
		for(int i = 0; i < 2; ++i) {
			this.fenotipo.add(0.0);
		}
		this.min = new Vector<Double>(2);
		this.max = new Vector<Double>(2);
		this.begin = new Vector<Integer>(2);
		this.end = new Vector<Integer>(2);
		this.min.add(-10.000); this.max.add(10.000);
		this.min.add(-10.000); this.max.add(10.000);
	}
	
	//constructor por copia
	public IndividuoFuncion3(IndividuoFuncion3 aux) {
		this.fenotipo=aux.fenotipo;
		this.min=aux.min;
		this.max = aux.max;
		this.begin = aux.begin;
		this.end = aux.end;
		this.cromosoma=aux.cromosoma;
	}
	
	//constructor por precision
	public IndividuoFuncion3(double precision) {
		this();	//setup inicial
		setSize(precision);	
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
		double x1 = this.fenotipo.elementAt(0);
		double x2 = this.fenotipo.elementAt(1);
		
		this.fitness = - Math.abs(Math.sin(x1) * Math.cos(x2) * Math.exp(Math.abs(1 - (Math.sqrt(Math.pow(x1, 2) + Math.pow(x2, 2))/Math.PI))));
	}
	

	//Hace una copia sin referencia 
	public Individuo copia() {
		IndividuoFuncion3 copiado=new IndividuoFuncion3();
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
		copiado.fitness=fitness;
		copiado.fitnessDesp=fitnessDesp;

		return copiado;
	}
}
