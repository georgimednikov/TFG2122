package algoritmoGenetico.individuos;

import java.util.Random;
import java.util.Vector;
import java.util.stream.Collectors;

import map.GramaMapa;

import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

public class IndividuoCifrado extends IndividuoEntero {
	private String mensajeCifrado;
	private String mensajeTraducido;
	private GramaMapa gMapa;
	
	//min y max no existen pues min es 0 y max 25 (la longitud del vector), correspondiendo a las letras del abecedario
	public IndividuoCifrado(){
		int tam=26;
		this.fenotipo = new Vector<Double>(tam);
		for(int i = 0; i < tam; ++i) {
			this.fenotipo.add(0.0);
		}
		this.begin = new Vector<Integer>(tam);
		this.end = new Vector<Integer>(tam);
	}
	
	//constructor por copia
	public IndividuoCifrado(IndividuoCifrado aux) {
		this.fenotipo = aux.fenotipo;
		this.min = aux.min;
		this.max = aux.max;
		this.begin = aux.begin;
		this.end = aux.end;
		this.cromosoma = aux.cromosoma;
		this.mensajeCifrado = aux.mensajeCifrado;
	}
	
	//constructor con precision
	public IndividuoCifrado(double precision) {
		this();	//setup inicial
		setSize(precision);	
		generarCromosoma();
		setFenotipo();
	}
	
	public void calculateFitness() {
		//Traduce el mensaje según el cromosoma
		translate();
		
		//Codigo de bigramas de diapositiva
		Map<String, Float> compare = extractGramas();
		
		//Recorrer todo este submapa para coger el total y dividir los valores entre el total para encontrar las freceuncias (muy costosos? preguntar profe)
		evaluarGramas(compare);
	}
	

	//Hace una copia sin referencia 
	public Individuo copia() {
		IndividuoCifrado copiado = new IndividuoCifrado();
		for(int i = 0; i < begin.size(); ++i) {
			int a = begin.elementAt(i);
			copiado.begin.add(i, a);
		}
		for(int i = 0; i < end.size(); ++i) {
			copiado.end.add(i, end.elementAt(i));
		}
		//setSize(precision);
		copiado.cromosoma = new Vector<Integer>(cromosoma.size());
		for(int i = 0; i < cromosoma.size(); ++i) {
			copiado.cromosoma.add(i, cromosoma.elementAt(i));
		}
		for(int i = 0; i < fenotipo.size(); ++i) {
			copiado.fenotipo.setElementAt(fenotipo.elementAt(i), i);
		}
		
		copiado.puntuacion = puntuacion;
		copiado.fitness = fitness;
		copiado.fitnessDesp = fitnessDesp;
		
		copiado.setMensaje(this.mensajeCifrado);
		copiado.mensajeTraducido = new String(this.mensajeTraducido);
		copiado.setGrama(this.gMapa);

		return copiado;
	}
	
	//genera numeros del 1 al 25 (alfabeto) y los shufflea
	//lo que hay en la posicion 0 (a) es el valor que se traduce (a en mensaje seria la letra del valor que hay en el vector)
	public void generarCromosoma() {
		for(int i = 0; i < this.fenotipo.capacity(); ++i) {
			this.cromosoma.add(i, i);
		}		
		Collections.shuffle(this.cromosoma);
	}
	
	//pone el mensaje cifrado al parametro
	public void setMensaje(String msg) {
		this.mensajeCifrado = new String(msg);
	}
	
	//pone el GramaMapa al individuo
	public void setGrama(GramaMapa m) {
		this.gMapa = m;
	}
	
	public String getDescifrado() {
		return mensajeTraducido;
	}
	
	//Traduce el mensaje segun el cromsoma
	private void translate() 
	{
		StringBuilder aux = new StringBuilder(mensajeCifrado);
		for(int i = 0; i < aux.length(); ++i) 
		{
			char thisOne = aux.charAt(i);
			if(thisOne >= 'A' && thisOne <= 'z' ) //Si son letras
			{	
				char pVal = thisOne;
				//Coge el valor de la tabla y lo cambia (teniendo en cuenta que puede ser mayuscula)
				thisOne = (char)('a' + this.getCromosoma().elementAt(Character.toLowerCase(thisOne) - 'a'));
				//Si era mayuscula la vuelve a poner mayuscula 
				if(pVal < 'a') {
					thisOne = Character.toUpperCase(thisOne);
				}
				
				aux.setCharAt(i, thisOne);
			}
		}
		this.mensajeTraducido = aux.toString();
	}
	
	//Obtiene un vector con las palabras del texto
	private Vector<String> cachearPalabras(String texto) 
	{
		Vector<String> words = new Vector<String>();
		String actual = "";
		for(int i = 0; i < texto.length(); ++i) 
		{
			char t = texto.charAt(i);
			if(t >= 'A' && t <= 'z') 
			{
				actual += t;
			}
			else if(actual != "")
			{
				words.add(actual);
				actual = "";
			}
		}
		if(actual != "") {
			words.add(actual);
		}
		
		return words;
	}
	
	//Extrae los gramas del texto
	private Map<String, Float> extractGramas()
	{
		HashMap<String, Float> res = new HashMap<>();
		Vector<String> words = cachearPalabras(mensajeTraducido);

		HashMap<String, Float> bi = new HashMap<>(); // 2
		int biC = 0;
		HashMap<String, Float> tri = new HashMap<>(); // 3
		int triC = 0;
		//HashMap<String, Float> cuadra = new HashMap<>(); // 4
		//int cuadraC = 0;
		
		//Por cada palabra del texto
		for(int i = 0; i < words.size(); ++i) 
		{   
			String sub = "";		
			for(int j = 0; j + 2 <= words.elementAt(i).length(); ++j) // Busca bigramas
			{
				sub = words.elementAt(i).substring(j,  j + 2).toLowerCase();
				if(bi.containsKey(sub)) {
					bi.put(sub, bi.get(sub) + 1);
				}else {
					bi.put(sub, Float.valueOf(1));
				}
				biC++;
			}
			
			for(int j = 0; j + 3 <= words.elementAt(i).length(); ++j) // Busca trigramas
			{
				sub = words.elementAt(i).substring(j,  j + 3).toLowerCase();
				if(tri.containsKey(sub)) {
					tri.put(sub, tri.get(sub) + 1);
				}else {
					tri.put(sub, Float.valueOf(1));
				}
				triC++;
			}
			
			/*for(int j = 0; j + 4 <= words.elementAt(i).length(); ++j) // Busca cuadragramas
			{
				sub = words.elementAt(i).substring(j,  j + 4).toLowerCase();
				if(cuadra.containsKey(sub)) {
					cuadra.put(sub, cuadra.get(sub) + 1);
				}else {
					cuadra.put(sub, Float.valueOf(1));
				}
				cuadraC++;
			}*/
		}
		
		for ( String key : bi.keySet() ) {
		    bi.put(key, bi.get(key) / biC);
		}
		
		for ( String key : tri.keySet() ) {
		    tri.put(key, tri.get(key) / triC);
		}
		
		/*for ( String key : cuadra.keySet() ) {
		    cuadra.put(key, cuadra.get(key) / cuadraC);
		}*/
		
		// Une todos con el final, que sera la respuesta
		res.putAll(bi);
		res.putAll(tri);
		//res.putAll(cuadra);	// Cuadragramas quitados a la hora de testear
		
		return res;
	}	
	
	private void evaluarGramas(Map<String, Float> trans) 
	{
		this.fitness = 0;
		for(String key: trans.keySet()) 
		{
			if(gMapa.RetValue(key) != null) {
				//Float frec = trans.get(key);
	            //double toSum = (1.0 - Math.abs(frec - gMapa.RetValue(key))) * key.length();	// Fitness de prueba
				//double toSum = Math.abs((frec * log2(frecIngles))); //* key.length());	
				
				Float frec = trans.get(key);
				Float frecIngles = gMapa.RetValue(key);
				
				double toSum = Math.abs((Math.log(1-Math.abs(frec-frecIngles))/Math.log(2)));
			
				this.fitness += toSum;
			}		
		}
	}
}
