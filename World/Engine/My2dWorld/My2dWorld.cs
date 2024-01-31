using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.VisualBasic.Devices;
using World.Engine.My2dWorld.Primitives;
using Rectangle = World.Engine.Primitives.Rectangle;

namespace World.Engine.My2dWorld
{
    public class My2dWorld
    {
        private static int _stepPerSecond = 60;

        private TimeSpan _estimatedStepTime = TimeSpan.FromMilliseconds((double)1000 / _stepPerSecond);
        private TimeSpan _nextStepTime = TimeSpan.Zero;
        private TimeSpan _stepCalcTime = TimeSpan.Zero;

        private CancellationTokenSource _cancellationTokenSource;
        private Stopwatch _stepStopwatch;
        private Stopwatch _globalTimeStopwatch { get; set; }

        public int CurrentStep { get; private set; } = 0;
        public TimeSpan WorldTime => _globalTimeStopwatch.Elapsed;


        public int Width { get; }
        public int Height { get; }

        private Vector.Vector2 Gravity { get; set; }
        private Dictionary<IPhysicsEntity, Dictionary<string, Vector.Vector>> CustomForces { get; set; } = new();

        public List<PhysicsBaseEntity> Entities { get; set; }

        public My2dWorld(int width, int height)
        {
            Width = width;
            Height = height;

            Entities = new List<PhysicsBaseEntity>();

            /*
            for (var i = 0; i < 50; i++)
            {
                var rect = new PhysicsRectangle(100, 100, 100f, 100f);
                rect.SetSpeed(new Vector.Vector(0.5f+i*0.1f, -1));
                rect.SetAngularSpeed(5f);

                rect.SetAcceleration(new Vector.Vector(0, 0.01f));
                rect.SetAngularAcceleration(-0.02f);
                
                Entities.Add(rect);
            }
            */

            var x = new PhysicsRectangle(200, 200, 150, 50, 2);
            var y = new PhysicsRectangle(400, 200, 50, 50, 1);



            //x.SetSpeed(new Vector.Vector(1, 0));
            //y.SetSpeed(new Vector.Vector(-1, 0));


            Entities.Add(x);
            Entities.Add(y);
            //Entities.Add(z);

            //AddCustomForce("gravity",new Vector.Vector(0, 0.005f));
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    _stepStopwatch = new Stopwatch();
                    _stepStopwatch.Start();

                    _globalTimeStopwatch = new Stopwatch();
                    _globalTimeStopwatch.Start();
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var frameWasUpdated = Tick();
                        if (frameWasUpdated)
                        {
                            CurrentStep++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _globalTimeStopwatch.Stop();
        }

        public void AddCustomForce(string name, Vector.Vector force, IPhysicsEntity? applyTo = null)
        {
            if (applyTo != null)
            {
                if (!CustomForces.ContainsKey(applyTo))
                {
                    CustomForces.Add(applyTo, new Dictionary<string, Vector.Vector>());
                }

                CustomForces[applyTo][name] = force;
            }
            else
            {
                foreach (var entity in Entities)
                {
                    if (!CustomForces.ContainsKey(entity))
                    {
                        CustomForces.Add(entity, new Dictionary<string, Vector.Vector>());
                    }

                    CustomForces[entity][name] = force;

                    entity.AddForce("up", new Vector.Vector(0, -0.01f));
                    entity.RemoveForce("down");
                }
            }
        }

        public void RemoveCustomForce(string name, IPhysicsEntity? applyTo = null)
        {
            if (applyTo != null)
            {
                if (CustomForces.ContainsKey(applyTo) && CustomForces[applyTo].ContainsKey(name))
                {
                    CustomForces[applyTo].Remove(name);

                    applyTo.RemoveForce(name);
                }
            }

            foreach (var entity in Entities)
            {
                if (CustomForces.ContainsKey(entity) && CustomForces[entity].ContainsKey(name))
                {
                    CustomForces[entity].Remove(name);
                }

                entity.RemoveForce(name);
            }
        }

        public void DoLogic()
        {
            /*
            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                AddCustomForce("up", new Vector.Vector(0, -0.01f));
                RemoveCustomForce("down");
            }

            if ((Control.ModifierKeys & Keys.Alt) != 0)
            {
                AddCustomForce("down", new Vector.Vector(0, 0.01f));
                RemoveCustomForce("up");
            }
            */
        }

        public void Update()
        {
            foreach (var entity in Entities)
            {
                entity.Forces.Clear();

                if (CustomForces.ContainsKey(entity))
                {
                    var entityForces = CustomForces[entity];
                    foreach (var force in entityForces)
                    {
                        entity.AddForce(force.Key, force.Value);
                    }
                }

                entity.Update();
            }

            //CustomForces.Clear();

            /*
            var tuples = new List<Tuple<PhysicsBaseEntity, PhysicsBaseEntity>>();
            foreach (var entity in Entities)
            {
                foreach (var otherEntity in Entities)
                {
                    if (entity == otherEntity)
                    {
                        continue;
                    }

                    if (tuples.Contains(new Tuple<PhysicsBaseEntity, PhysicsBaseEntity>(entity, otherEntity)))
                    {
                        continue;
                    }

                    if (tuples.Contains(new Tuple<PhysicsBaseEntity, PhysicsBaseEntity>(otherEntity, entity)))
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<PhysicsBaseEntity, PhysicsBaseEntity>(entity, otherEntity));
                }
            }

            foreach (var tuple in tuples)
            {
                var entity1 = tuple.Item1;
                var entity2 = tuple.Item2;

                if (entity1.IsIntersect(entity2))
                {
                    var newAcc1 = new Vector.Vector(entity2.Acceleration.X, entity2.Acceleration.Y);
                    var newSpeed1 = new Vector.Vector(entity2.Velocity.X, -entity2.Velocity.Y);

                    var newAcc2 = new Vector.Vector(entity1.Acceleration.X, entity1.Acceleration.Y);
                    var newSpeed2 = new Vector.Vector(entity1.Velocity.X, entity1.Velocity.Y);

                    entity1.SetAcceleration(newAcc1);
                    entity1.SetSpeed(newSpeed1);

                    entity2.SetAcceleration(newAcc2);
                    entity2.SetSpeed(newSpeed2);
                }
            }            
           
            */
        }

        public int GetFps()
        {
            return (int)(1000 / _stepCalcTime.TotalMilliseconds);
        }

        /// <summary>
        /// Обновить состояние мира, если пришло время
        /// </summary>
        /// <returns></returns>
        private bool Tick()
        {
            if (_nextStepTime <= _stepStopwatch.Elapsed)
            {
                _stepCalcTime = _stepStopwatch.Elapsed;
                _stepStopwatch.Restart();
                
                DoLogic();
                Update();

                var elapsed = _stepStopwatch.Elapsed;
                var remains = _estimatedStepTime - elapsed;

                _nextStepTime = elapsed + remains;

                return true;
            }
            
            return false;
        }
    }
}
