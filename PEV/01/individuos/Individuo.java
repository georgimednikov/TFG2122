package algoritmoGenetico.individuos;

import java.util.Vector;

public abstract class Individuo<T>  {
	//Cromosoma reprsentado mediante T
	Vector<T> cromosoma; 
	//Fenotipo de cada gen
	Vector<Double> fenotipo;
	//Minimo y maximo valores del dominio del fenotipo de cada gen
	Vector<Double> min; Vector<Double> max;
	//Indice de inicio y fin de cada gen en el cromosoma
	Vector<Integer> begin; Vector<Integer> end;
	//Aptitud del individuo
	double fitness; 
	//Aptitud desplazada (usada para la puntuacion)
	double fitnessDesp;
	//Aptitud relativa a la aptitud total de la poblacion
	double puntuacion;
	//Metodos abstractos de individuo
	abstract void setSize(double precision);
	public abstract void setFenotipo();
	abstract void setCromosoma(Vector<T> cromosoma);
	public abstract void setPuntuacion(double total);
	public abstract void calculateFitness();
	public abstract void setFitness(double fit);
	public abstract void setFitnessD(double fitD);
	public abstract Individuo copia();
	public abstract void MutaGen(int index);
	
	public abstract Vector<T> getCromosoma();
	public abstract double getFenotipo(int index);
	public abstract Vector<Double> getFenotipo();
	//Getters para consultar datos del individuo
	public double getFitness() {return fitness;}
	public double getFitnessD() {return fitnessDesp;}
	public double getPuntuacion() {return puntuacion;}
	public Vector<Double> getMin() {return min;}
    public Vector<Double> getMax() {return max;}
    public Vector<Integer> getBegin() {return begin;}
    public Vector<Integer> getEnd() {return end;}
}
