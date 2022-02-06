using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// Clase que define el mundo simulado
    /// </summary>
    public class World
    {
        /// <summary>
        /// Datos fisicos de cada casilla del mapa
        /// </summary>
        struct MapData
        {
            float height, humidty, temperature, flora;
        }

        /// <summary>
        /// Inicializa el mapa con una matriz cuadrada
        /// </summary>
        /// <param name="size">Tamanio de la matriz</param>
        public void Init(int size)
        {
            mapSize = size;
            map = new MapData[mapSize, mapSize];
            entities = new List<IEntity>();
            delete = new List<IEntity>();
        }

        /// <summary>
        /// Aniade una entidad a la lista del mundo
        /// </summary>
        /// <typeparam name="T">La entidad a aniadir</typeparam>
        /// <returns>Entidad aniadida</returns>
        public T AddEntity<T>() where T : IEntity, new()
        {
            T ent = new T();
            entities.Add(ent);
            return ent;
        }

        /// <summary>
        /// Marca a una entidad para que se elimine en el siguiente frame
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        public void Delete(IEntity entity)
        {
            delete.Add(entity);
        }

        /// <summary>
        /// Comprueba si las coordenadas objetivo se encuentran dentro del mapa
        /// </summary>
        /// <param name="x">Coordenada x</param>
        /// <param name="y">Coordenada y</param>
        /// <returns>Si las coordenadas se encuentran en el mapa</returns>
        public bool canMove(int x, int y)
        {
            return (x >= 0 && x < mapSize && y >= 0 && y < mapSize);
        }

        /// <summary>
        /// Paso de ejecucion de la simulacion del mundo
        /// </summary>
        public void Tick()
        {
            entities.ForEach(delegate (IEntity e) { e.Tick(); });   // Le dice a al entidad que se actualice

            delete.ForEach(delegate (IEntity e) { entities.Remove(e); });

            delete.Clear();
        }

        // Entidades en el mundo
        public List<IEntity> entities { get; private set; }
        // Entidades a eliminar
        List<IEntity> delete;
        // Mapa fisico
        MapData[,] map;
        // Tamanio del mapa
        public int mapSize { get; private set; }
    }
}
