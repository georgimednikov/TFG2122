package algoritmoGenetico.individuos;

import java.util.Random;
import java.util.Vector;

public abstract class IndividuoEntero extends Individuo<Integer>{
	//Constructor vacio
	IndividuoEntero() {};
		
	//Construye el idnividuo segun la precision
	public IndividuoEntero(double precision){
		this();	//setup inicial
		setSize(precision);	
		for(int i = 0; i < this.fenotipo.capacity(); ++i) {
			//para cada gen, generacion random de valores dentro del dominio del mismo
			Random r = new Random();
			Integer mini = this.min.elementAt(i).intValue();
			Integer recorrido = this.max.elementAt(i).intValue() - mini;
			cromosoma.setElementAt(mini + r.nextInt(recorrido + 1), i);
		}
		setFenotipo();
	}
				
	//asigan un fenotipo a cada individuo segun su gen y datos
	public void setFenotipo() {
		for(int index = 0; index < fenotipo.size(); ++index) {
			this.fenotipo.setElementAt(Double.valueOf(this.cromosoma.elementAt(index)), index);
		}
	}
		
	//asinga cromsoma directamente
	public void setCromosoma(Vector<Integer> cromosoma) {
		this.cromosoma=cromosoma;
	}
		
	//hace un set del tamano de los genes segun sus menores, mayores y la precisionn
	public void setSize(double precision) {
		int nGenes = this.fenotipo.capacity();	
		for(int i = 0; i < this.fenotipo.capacity(); ++i) {
			this.begin.add(i);
			this.end.add(i + 1);
		}
		this.cromosoma = new Vector<Integer>(nGenes);
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
	
	public Vector<Integer> getCromosoma() {
		return this.cromosoma;
	}
	
	//metodo para debugear, imprime el cromosoma con 0 y 1
	public void printCromosoma() {
		for(Integer b: cromosoma) {
			System.out.print(b);
		}
		System.out.println();
	}
	
	//Muta un gen especifico, usado para hacer mutacion uniforme genetica
	public void MutaGen(int index) {
		//No va a haber mutacion uniforme, asi que irrelevante
		/*for(int i=this.begin.elementAt(index); i<this.end.elementAt(index);i++){
			Random r = new Random();
			if(r.nextDouble() > 0.5) cromosoma.setElementAt(true, i);
			else cromosoma.setElementAt(false, i);*/
	} //Basicamente reinicializa el gen
}
