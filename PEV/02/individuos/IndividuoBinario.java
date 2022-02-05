package algoritmoGenetico.individuos;

import java.util.Random;
import java.util.Vector;

public abstract class IndividuoBinario extends Individuo<Boolean> {
	//Constructor vacio
	IndividuoBinario() {};
	
	//Construye el idnividuo segun la precision
	public IndividuoBinario(double precision){
		this();	//setup inicial
		setSize(precision);	
		for(int i = 0; i < this.fenotipo.capacity(); ++i) {
			//para cada gen, generacion random de valores dentro del dominio del mismo
			Random r = new Random();
			for(int j = begin.elementAt(i); j < end.elementAt(i); ++j) {
				if(r.nextDouble() > 0.5) cromosoma.add(j, true);
				else cromosoma.add(j, false);
			}
		}
		setFenotipo();
	}
	
	
//------------------------PUBLIC-------------------------------
	//asigan un fenotipo a cada individuo segun su gen y datos
	public void setFenotipo() {
		for(int index = 0; index < fenotipo.size(); ++index) {
			int beg = this.begin.elementAt(index); 
			int en = this.end.elementAt(index); 
			int tam = en - beg;	
			double b2d = bin2dec(beg, en);
			double secparam = (max.elementAt(index) - min.elementAt(index))/(Math.pow(2, tam) - 1);
			this.fenotipo.setElementAt(min.elementAt(index) + b2d * secparam, index); //formula diapositiva
		}
	}
	
	//asinga cromsoma directamente
	public void setCromosoma(Vector<Boolean> cromosoma) {
		this.cromosoma=cromosoma;
	}
	
	//hace un set del tamano de los genes segun sus menores, mayores y la precisionn
	public void setSize(double precision) {
		int nGenes = this.fenotipo.capacity();
		int index = 0;
		for(int i = 0; i < nGenes; ++i) {
			int tgenes = (int)(log2(1 + ((this.max.elementAt(i) - this.min.elementAt(i))/precision))); //formula diapositivas
			//esto establece los inicios y finales (de forma [begin, end)) de cada gen
			this.begin.add(i, index); index += tgenes; 
			this.end.add(i, index);
		}
		this.cromosoma = new Vector<Boolean>(index);
	};
	
	//setters y getters generales
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
	
	public Vector<Boolean> getCromosoma() {
		return this.cromosoma;
	}
	
	//metodo para debugear, imprime el cromosoma con 0 y 1
	public void printCromosoma() {
		for(Boolean b: cromosoma) {
			if(b) System.out.print(1);
			else System.out.print(0);
		}
		System.out.println();
	}
	
	//Muta un gen especifico, usado para hacer mutacion uniforme genetica
	public void MutaGen(int index) {
		for(int i=this.begin.elementAt(index); i<this.end.elementAt(index);i++){
			Random r = new Random();
			if(r.nextDouble() > 0.5) cromosoma.setElementAt(true, i);
			else cromosoma.setElementAt(false, i);
		} //Basicamente reinicializa el gen
	}

//------------------------PRIVATE-------------------------------
	//Logaritmo en base 2
	private double log2(double a) {
		return Math.log10(a)/Math.log10(2);
	}
	
	//Pasa un segmento del cromosoma de binario a decimal 
	private double bin2dec(int begin, int end) {
		double ret = 0; int cont = 0;
		for(int i = begin; i < end; ++i) {
			if(this.cromosoma.elementAt(i)) ret += Math.pow(2, cont);
			cont++;
		}
		return ret;
	}	
}
