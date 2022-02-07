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
    public class Creature : IEntity
    {
        /// <summary>
        /// Clase privada para representar los estados
        /// El estado tiene un identificador y una accion asociada
        /// </summary>
        private class State
        {
            public StateID name;
            public Action action;

            public State(StateID n, Action a)
            {
                name = n; action = a;
            }
        }

        /// <summary>
        /// Constructor para factorias
        /// </summary>
        public Creature()
        {
            r = new Random();
        }

        /// <summary>
        /// Inicia una criatura en un mundo y posicion
        /// </summary>
        /// <param name="w">El mundo en el que tomara residencia</param>
        /// <param name="x">Posicion x</param>
        /// <param name="y">Posicion y</param>
        public void Init(World w, int x, int y)
        {
            world = w;
            this.x = x;
            this.y = y;
            this.fsm = new Stateless.StateMachine<State, TriggerID>(
                () => currState, 
                s => currState = s, 
                Stateless.FiringMode.Queued
            );
            ConfigureStateMachine();
        }

        /// <summary>
        /// Paso de simulacion
        /// </summary>
        public void Tick()
        {
            // TODO: Poner los Fire en las acciones de los estados
            // y quitar esto de aquí
            if (currState.name == StateID.Dead) return;
            Speed--;
            if (Speed <= 0)
            {
                Speed = 0;
                fsm.Fire(TriggerID.Dies);
            }
            else if (Speed % 2 == 0)
            {
                fsm.Fire(TriggerID.Moves);
            }
            else fsm.Fire(TriggerID.Stops);

            // Ejecuta la accion correspondiente al estado actual
            currState.action();
            // evaluateTransitions();
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

        /// <summary>
        /// Devuelve el estado actual de la creatura
        /// </summary>
        /// <returns></returns>
        public string GetState()
        {
            return currState.name.ToString();
        }

        /// <summary>
        /// Configura la maquina de estados de la creatura
        /// con los estados que se indiquen.
        /// TODO: los estados los estamos poniendo a pelo
        /// </summary>
        void ConfigureStateMachine()
        {
            // Setup de estados
            State Alive = new State(
               StateID.Alive,
               () => { /*TODO: Borrar*/Console.WriteLine("Idle"); }
           );
            State Dead = new State(
                 StateID.Dead,
                () => { /*TODO: Borrar*/Console.WriteLine("Dead"); }
            );

            State Idle = new State(
                StateID.Idle,
                () => { /*TODO: Borrar*/Console.WriteLine("Idle"); }
            );


            State Moving = new State(
                 StateID.Moving,
                () => { /*TODO: Borrar*/Console.WriteLine("Moving"); }
            );

            // Estado inicial
            currState = Moving;

            // Establecer cada estado en la FSM y los 
            // triggers de transiciones
            fsm.Configure(Alive)
                .Permit(TriggerID.Dies, Dead)
                .InitialTransition(Moving);

            fsm.Configure(Dead);

            fsm.Configure(Idle)
                .SubstateOf(Alive);
                //.Permit(TriggerID.Moves, Moving);

            fsm.Configure(Idle)
                //.SubstateOf(Alive)
                .Permit(TriggerID.Moves, Moving);

            fsm.Configure(Moving)
                .SubstateOf(Alive)
                .Permit(TriggerID.Stops, Idle);

            _State moving = new Moving();
            mfsm = new StatelessFSM(moving);
        }

        // Posicion en el mapa del mundo
        public int x, y;
        // Mundo en el que existe la criatura
        World world;
        // Generador de números lolrandom
        Random r;
        // Maquina de estados
        // Esquema: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        Stateless.StateMachine<State, TriggerID> fsm;
        StatelessFSM mfsm;
        // Estado actual
        State currState;

        // TODO: Velocidad, pero ahora solo es para probar la FSM
        public float Speed = 10;
    }
}
