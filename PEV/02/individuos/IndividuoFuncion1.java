package algoritmoGenetico.individuos;

import java.util.Random;
import java.util.Vector;

public class IndividuoFuncion1 extends IndividuoBinario{
	//datos especificos del individuo
	public IndividuoFuncion1(){
		this.fenotipo = new Vector<Double>(2);
		for(int i = 0; i < 2; ++i) {
			this.fenotipo.add(0.0);
		}
		this.min = new Vector<Double>(2);
		this.max = new Vector<Double>(2);
		this.begin = new Vector<Integer>(2);
		this.end = new Vector<Integer>(2);
		this.min.add(-3.000); this.max.add(12.100);
		this.min.add(4.000); this.max.add(5.800);
	}
	
	//constructor por copia
	public IndividuoFuncion1(IndividuoFuncion1 aux) {
		this.fenotipo=aux.fenotipo;
		this.min=aux.min;
		this.max = aux.max;
		this.begin = aux.begin;
		this.end = aux.end;
		this.cromosoma=aux.cromosoma;
	}
	
	//constructor con precision
	public IndividuoFuncion1(double precision) {
		this();	//setup inicial
		setSize(precision);
		for(int i = 0; i < this.fenotipo.capacity(); ++i) {
			//para cada gen, generacion random de valores dentro del ambito del mismo
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
		this.fitness = 21.5f + x1*Math.sin(4*Math.PI*x1) + x2*Math.sin(20*Math.PI*x2);
	}
	

	//Hace una copia sin referencia 
	public Individuo copia() {
		IndividuoFuncion1 copiado=new IndividuoFuncion1();
		for(int i=0; i< begin.size();++i) {
			int a=begin.elementAt(i);
			copiado.begin.add(i,a);
		}
		for(int i=0; i< end.size();++i) {
			copiado.end.add(i,end.elementAt(i));
		}
		//setSize(precision);
		copiado.cromosoma=new Vector<Boolean>(cromosoma.size());
		for(int i=0; i< cromosoma.size();++i) {
			copiado.cromosoma.add(i, cromosoma.elementAt(i));
		}
		for(int i=0; i< fenotipo.size();++i) {
			copiado.fenotipo.setElementAt(fenotipo.elementAt(i), i);
		}
		
		for(int i=0; i< max.size();++i) {
			copiado.max.setElementAt(max.elementAt(i),i);
		}
		for(int i=0; i< min.size();++i) {
			copiado.min.setElementAt( min.elementAt(i),i);
		}	
		copiado.puntuacion=puntuacion;
		copiado.fitness=fitness;
		copiado.fitnessDesp=fitnessDesp;

		return copiado;
	}

	
}
