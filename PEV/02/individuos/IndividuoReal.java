package algoritmoGenetico.individuos;

import java.util.Random;
import java.util.Vector;

public abstract class IndividuoReal extends Individuo<Double>{
	//Constructor vacio
	IndividuoReal() {};

	//Construye el individuo
	public IndividuoReal(double precision){
		this();	//setup inicial
		setSize(precision);	
		generateCromosoma();
		setFenotipo();
	}


	//------------------------PROTECTED-------------------------------
	//para cada gen, generacion random de valor dentro del dominio del mismo
	protected void generateCromosoma() {
		for(int i = 0; i < this.fenotipo.capacity(); ++i) {
			double mi = this.min.elementAt(i);
			double ma = this.max.elementAt(i);
			
			double numero = mi + Math.random() * (ma-mi);
			this.cromosoma.add(numero);
		}
	}
	
	//------------------------PUBLIC-------------------------------
	//asigna un fenotipo por cada gen, que en este caso son iguales al cromosoma por la representacion
	public void setFenotipo() {
		for(int index = 0; index < fenotipo.size(); ++index) {
			this.fenotipo.setElementAt(this.cromosoma.get(index), index);
		}
	}

	public void setCromosoma(Vector<Double> cromosoma) {
		this.cromosoma=cromosoma;
	}
		

	//hace un set del tamaño de los genes, que con esta representacion siempre sera 1
	public void setSize(double precision) {
		int nGenes = this.fenotipo.capacity();	
		for(int i = 0; i < this.fenotipo.capacity(); ++i) {
			this.begin.add(i);
			this.end.add(i+1);
		}
		this.cromosoma = new Vector<Double>(nGenes);
	};

	//getters y setters
	public void setPuntuacion(double total) {
		this.puntuacion = this.fitnessDesp / total;
	}

	public void setFitness(double fit) {
		this.fitness = fit;
	}
	
	public void setFitnessD(double fit) {
		this.fitnessDesp = fit;
	}

	public double getFenotipo(int index) {
		return this.fenotipo.elementAt(index);
	}

	public Vector<Double> getFenotipo() {
		return this.fenotipo;
	}

	public Vector<Double> getCromosoma() {
		return this.cromosoma;
	}

	public void printCromosoma() {
		for(Double b: cromosoma) {
			System.out.print(b);
		}
		System.out.println();
	}
	
	public void MutaGen(int index) {
		double mi = this.min.elementAt(index);
		double ma = this.max.elementAt(index);
		
		double numero = mi + Math.random() * (ma-mi);
		this.cromosoma.setElementAt(numero, index);
	}
}
