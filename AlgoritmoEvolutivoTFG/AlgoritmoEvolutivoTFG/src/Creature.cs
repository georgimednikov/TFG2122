using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// La creatura
    /// </summary>
    class Creature : IEntity
    {
        /// <summary>
        /// Constructor para factorias
        /// </summary>
        public Creature()
        {
            r = new Random();
        }

        /// <summary>
        /// Inicia una criatura en un mundo y posición
        /// </summary>
        /// <param name="w">El mundo en el que tomara residencia</param>
        /// <param name="x">Posición x</param>
        /// <param name="y">Posicion y</param>
        public void Init(World w, int x, int y)
        {
            world = w;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Paso de simulación
        /// </summary>
        public void Tick()
        {
            Move();
        }

        /// <summary>
        /// Intenta mover la criatura a una posicon contigua aleatoria
        /// </summary>
        void Move()
        {
            int nX = x + r.Next(-1, 2),
                nY = y + r.Next(-1, 2);
            if (world.canMove(nX, nY)) { x = nX; y = nY; }
        }

        // Posicion en el mapa del mundo
        public int x, y;
        // Mundo en el que existe la criatura
        World world;
        // Generador de números lolrandom
        Random r;
    }
}
