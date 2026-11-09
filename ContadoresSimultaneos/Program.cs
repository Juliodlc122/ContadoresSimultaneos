using System;
using System.Collections.Generic;
using System.Threading;
//Julio Antonio Ruiz De La Cruz Matricula:2024-2009 
namespace ContadoresSimultaneos
{
    class Program
    {
        // Lista global para almacenar los contadores activos
        static List<CounterWorker> contadores = new List<CounterWorker>();
        static int contadorIdGlobal = 1;

        static void Main(string[] args)
        {
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA DE CONTADORES SIMULTÁNEOS ===");
                Console.WriteLine("1. Iniciar un nuevo contador");
                Console.WriteLine("2. Detener un contador");
                Console.WriteLine("3. Mostrar estado actual (Tiempo Real)");
                Console.WriteLine("4. Salir del programa");
                Console.Write("\nSeleccione una opción: ");

                string? opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        IniciarContador();
                        break;
                    case "2":
                        DetenerContador();
                        break;
                    case "3":
                        MostrarEstadoEnTiempoReal();
                        break;
                    case "4":
                        DetenerTodos();
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opción no válida. Presione Enter para continuar...");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void IniciarContador()
        {
            Console.Write("\nIngrese el intervalo de tiempo en milisegundos (ej. 500 para medio segundo): ");
            if (int.TryParse(Console.ReadLine(), out int intervalo) && intervalo > 0)
            {
                var nuevoContador = new CounterWorker(contadorIdGlobal++, intervalo);
                contadores.Add(nuevoContador);
                nuevoContador.Start();

                Console.WriteLine($"\nContador #{nuevoContador.Id} iniciado exitosamente.");
            }
            else
            {
                Console.WriteLine("\nValor inválido. Debe ser un número entero mayor a cero.");
            }
            Console.WriteLine("Presione Enter para volver al menú...");
            Console.ReadLine();
        }

        static void DetenerContador()
        {
            Console.WriteLine("\n--- Contadores Activos ---");
            bool hayActivos = false;
            foreach (var c in contadores)
            {
                if (c.IsRunning)
                {
                    Console.WriteLine($"ID: {c.Id} | Conteo actual: {c.CurrentCount} | Intervalo: {c.IntervalMs}ms");
                    hayActivos = true;
                }
            }

            if (!hayActivos)
            {
                Console.WriteLine("No hay contadores en ejecución.");
                Console.WriteLine("Presione Enter para volver al menú...");
                Console.ReadLine();
                return;
            }

            Console.Write("\nIngrese el ID del contador que desea detener: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var contador = contadores.Find(c => c.Id == id);
                if (contador != null && contador.IsRunning)
                {
                    contador.Stop();
                    Console.WriteLine($"Contador #{id} detenido de manera segura.");
                }
                else
                {
                    Console.WriteLine("ID no encontrado o el contador ya está detenido.");
                }
            }
            Console.WriteLine("Presione Enter para volver al menú...");
            Console.ReadLine();
        }

        static void MostrarEstadoEnTiempoReal()
        {
            Console.Clear();
            Console.WriteLine("Mostrando progreso en tiempo real... (Presione cualquier tecla para volver al menú)\n");

            // Bucle que refresca la pantalla hasta que el usuario presione una tecla
            while (!Console.KeyAvailable)
            {
                // Guardamos la posición del cursor para no causar parpadeos bruscos
                Console.SetCursorPosition(0, 2);

                if (contadores.Count == 0)
                {
                    Console.WriteLine("No hay contadores creados aún.");
                }
                else
                {
                    foreach (var c in contadores)
                    {
                        string estado = c.IsRunning ? "En ejecución" : "Detenido";
                        // Se rellenan espacios en blanco al final para limpiar residuos de números anteriores
                        Console.WriteLine($"Contador #{c.Id} | Estado: {estado,-12} | Conteo: {c.CurrentCount,-10}");
                    }
                }

                Thread.Sleep(100); // Pausa breve para no saturar el procesador
            }

            // Consumir la tecla presionada para que no salte en el próximo menú
            Console.ReadKey(true);
        }

        static void DetenerTodos()
        {
            Console.WriteLine("\nDeteniendo todos los hilos de manera segura...");
            foreach (var c in contadores)
            {
                c.Stop();
            }
            Console.WriteLine("Programa finalizado.");
        }
    }

    // Clase que maneja la lógica individual de cada hilo
    public class CounterWorker
    {
        public int Id { get; }
        public int CurrentCount { get; private set; }
        public int IntervalMs { get; }
        public bool IsRunning => _isRunning;

        // Se usa 'volatile' para asegurar que el cambio de estado se refleje inmediatamente en el hilo
        private volatile bool _isRunning;
        private Thread? _thread;

        public CounterWorker(int id, int intervalMs)
        {
            Id = id;
            IntervalMs = intervalMs;
            CurrentCount = 0;
            _isRunning = false;
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            // Creación del hilo apuntando al método DoWork
            _thread = new Thread(DoWork)
            {
                // IsBackground = true permite que la aplicación se cierre aunque el hilo siga corriendo
                IsBackground = true
            };
            _thread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private void DoWork()
        {
            // El hilo incrementa el contador mientras _isRunning sea verdadero
            while (_isRunning)
            {
                CurrentCount++;
                Thread.Sleep(IntervalMs);
            }
        }
    }
}