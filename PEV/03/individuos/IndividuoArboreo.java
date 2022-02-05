package algoritmoGenetico.individuos;

import java.io.File;
import java.io.FileNotFoundException;
import java.util.Random;
import java.util.Scanner;

import algoritmoGenetico.tree.Arbol;
import algoritmoGenetico.tree.Nodo;
import algoritmoGenetico.tree.Operando;

public class IndividuoArboreo {
	// Codificacion del individuo
	public Arbol cromosoma;
	
	// Aptitud
	public float fitness;
	
	public float comida=0;
	
	// Aptitud entre la total de la poblacion
	public float puntuacion;
	
	// Camino de Santa Fe
	public Boolean[][] camino = new Boolean[32][32];
	
	// Posicion en el camino
	int pos[] = new int[2];
	
	// Direccion en la que mira
	int dir;
	
	// Vector de posibles direcciones
	int dirs[][] = new int[4][2];
	
	// Pasos realizados de momento
	int pasos = 0;
	
	// Pasos maximos a dar
	int pasosMaximos = 0;
	
	//si es nuevo se va a recalcular el fitness
	boolean nuevo=true;
	
	public IndividuoArboreo() {
		pos[0] = 0; pos[1] = 0;				// esquina superior izquierda
		nuevo=true;
		dirs[0][0] = 0; dirs[0][1] = 1;		// derecha
		dirs[1][0] = 1; dirs[1][1] = 0;		// abajo 
		dirs[2][0] = 0; dirs[2][1] = -1; 	// izquierda
		dirs[3][0] = -1; dirs[3][1] = 0; 	// arriba
	}
	
	public IndividuoArboreo(int prof, int type, int pasosMax) {
		this();
		pasosMaximos = pasosMax;
		cromosoma = new Arbol(prof, type);
		
	}	
	
	public IndividuoArboreo copia() {
		IndividuoArboreo nuevo = new IndividuoArboreo();		
		
		nuevo.cromosoma = cromosoma.copia();
		nuevo.fitness = fitness;
		nuevo.puntuacion = puntuacion;
		nuevo.pasosMaximos = pasosMaximos;
		nuevo.nuevo=true;
		nuevo.comida=comida;
		return nuevo;
	}
	
	public void calcularFitness(boolean eliminaIntrones) {
		leeArchivo();
		fitness = 0;
		pos[0] = 0; pos[1] = 0;		// esquina superior izquierda
		dir = 0;					// derecha
		pasos = 0;
		
		if(eliminaIntrones) Intron(cromosoma.raiz); //revisamos si existen intrones
		
		while(pasos < pasosMaximos) {
			doStep(cromosoma.raiz, null);
		}
		comida=fitness;
		fitness /= pasosMaximos;
	}
	
	private void doStep(Nodo actual, Boolean[][] rastro) {		
		if(pasos >= pasosMaximos) return;
		
		// Distincion de casos de valor
		if(!actual.isTerminal()) {
			switch(actual.valor) {
			case SICOMIDA:	
				int destX = pos[0] + dirs[dir][0];
				int destY = pos[1] + dirs[dir][1];
				if(camino[parse32(destX)][parse32(destY)]) doStep(actual.hijoIzquierdo, rastro);
				else doStep(actual.hijoDerecho, rastro);
				break;
			case PROGN2:
				doStep(actual.hijoIzquierdo, rastro);
				doStep(actual.hijoDerecho, rastro);
				break;
			case PROGN3:
				doStep(actual.hijoIzquierdo, rastro);
				doStep(actual.hijoCentral, rastro);
				doStep(actual.hijoDerecho, rastro);
				break;
			default:
				break;
			}
		}
		else {
			switch(actual.valor) {
			case AVANZA:	// Comer si hay en la nueva posicion
				pos[0] = parse32(pos[0] + dirs[dir][0]); 
				pos[1] = parse32(pos[1] + dirs[dir][1]);
				if(camino[pos[0]][pos[1]]) {
					camino[pos[0]][pos[1]] = false;
					fitness++;
				}
				if(rastro != null) rastro[pos[0]][pos[1]] = true;
				break;
			case DERECHA:
				dir = (dir + 1) % 4;
				break;
			case IZQUIERDA:
				if(dir == 0) dir = 3;
				else dir = (dir - 1) % 4;
				break;
			default:
				break;
			}
			pasos++;
		}
	}
	
	public void leeArchivo() {
	     try {
	         File myObj = new File("santafe.txt");
	         Scanner myReader = new Scanner(myObj);
	         int cIn = 0;
	         while (myReader.hasNextLine()) {
	           String data = myReader.nextLine();
	           for(int i = 0; i < 64; i += 2) {
	        	   if(data.charAt(i) == '#') camino[cIn][i / 2] = true;
	        	   else camino[cIn][i / 2] = false;
	           }
	           cIn++;
	         }
	         myReader.close();
	       } catch (FileNotFoundException e) {
	         System.out.println("An error occurred.");
	         e.printStackTrace();
	       }
	}
	
	public void copiaCamino() {
		for(int i = 0; i < 32; ++i) {
			for(int j = 0; j < 32; ++j) {
				camino[i][j] = camino[i][j];
			}
		}
	}
	
	public int parse32(int p) {
		int res = p;
		if(p < 0) res += 32;
		else if (p > 31) res -= 32;
		return res;
	}
	
	public float getProfundidad() {
		return cromosoma.profundidad;
	}
	
	public boolean getNuevo() {
		return this.nuevo;
	}
	
	public void setNuevo(boolean nuevo) {
		this.nuevo=nuevo;
	}
	
	public Boolean[][] getRastro(){
		Boolean[][] rastro = new Boolean[32][32];
		for(int i=0;i<32;i++) {
			for(int j=0;j<32;j++) {
				rastro[i][j] = false;
			}
		}
		
		leeArchivo();
		
		pos[0] = 0; pos[1] = 0;		// esquina superior izquierda
		dir = 0;					// derecha
		pasos = 0;
		
		while(pasos < pasosMaximos) {
			doStep(cromosoma.raiz, rastro);
		}
		
		return rastro;
	}

	public void print() {
		cromosoma.print();
		System.out.print(" => " + fitness + ", " + comida);
	}

	
	//devuelve el resultado final de ese arbol
	private Operando Intron(Nodo nodo) {			
		Random r=new Random();
		
		Operando op = nodo.valor;		
		//distinguimos si tenemos 2 o 3 hijos
		if(op == Operando.PROGN2) {
			//miramos hijo derecha y izquierda
			if((Intron(nodo.hijoDerecho) == Operando.DERECHA && Intron(nodo.hijoIzquierdo) == Operando.IZQUIERDA) || 
					(Intron(nodo.hijoDerecho) == Operando.IZQUIERDA && Intron(nodo.hijoIzquierdo) == Operando.DERECHA)) {
				//nos hemos quedado tal cual, por tanto elegimos al azar derecho o izquierda
				
				//asignamos un nueo operando aleatorio al nodo
				Integer val = r.nextInt(3);				
				Operando newOperando = Operando.values()[val];
				nodo.valor = newOperando;
				
				nodo.setHijosNull(); //reset
				//recalculamos la profundidad y nodos (no siempre se reducirá en la misma cantidad)
				cromosoma.recalculaNodos_Prof();
			}
		}
		else if(op == Operando.SICOMIDA) {
			//miramos hijo derecha y izquierda
			if(Intron(nodo.hijoDerecho) == Operando.DERECHA && Intron(nodo.hijoIzquierdo) == Operando.DERECHA) {
				//asignamos un nueo nodo terminal
				nodo.valor = Operando.DERECHA;
				
				nodo.setHijosNull(); //reset
				//recalculamos la profundidad y nodos (no siempre se reducirá en la misma cantidad)
				cromosoma.recalculaNodos_Prof();
			}
			else if(Intron(nodo.hijoDerecho) == Operando.IZQUIERDA && Intron(nodo.hijoIzquierdo) == Operando.IZQUIERDA) {
				//asignamos un nueo nodo terminal
				nodo.valor = Operando.IZQUIERDA;
				
				nodo.setHijosNull(); //reset
				//recalculamos la profundidad y nodos (no siempre se reducirá en la misma cantidad)
				cromosoma.recalculaNodos_Prof();
			}
		}
		else if (op == Operando.PROGN3) {
			
			//miramos los 3 hijos
			if(Intron(nodo.hijoDerecho) == Operando.DERECHA &&
					Intron(nodo.hijoCentral) == Operando.DERECHA &&
					Intron(nodo.hijoIzquierdo) == Operando.DERECHA) {
				//equivalente a un izquierda
				nodo.valor = Operando.IZQUIERDA;

				nodo.setHijosNull();
				//recalculamos la profundidad y nodos (no siempre se reducirá en la misma cantidad)
				cromosoma.recalculaNodos_Prof(); 
			}
			else if (Intron(nodo.hijoDerecho) == Operando.IZQUIERDA	 &&
					Intron(nodo.hijoCentral) == Operando.IZQUIERDA &&
					Intron(nodo.hijoIzquierdo) == Operando.IZQUIERDA) {
				//equivalente a un derecha
				nodo.valor = Operando.DERECHA;

				nodo.setHijosNull(); //reset
				//recalculamos la profundidad y nodos (no siempre se reducirá en la misma cantidad)
				cromosoma.recalculaNodos_Prof(); 
			}

		}

		return op;
	}
}
