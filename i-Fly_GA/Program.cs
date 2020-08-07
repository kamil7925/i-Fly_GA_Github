using I_Fly.Logic.Extensions.Genetic_Algorithm;
using I_Fly.Models;
using I_Fly.Models.Genetic_Algorithm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using System.Web.Script.Serialization;

namespace i_Fly_GA
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string thread_marker = args[0];
                //string thread_marker = @"D:\Documents\i-Fly\i-Fly\i-Fly\App_Data\I-Fly_GA_26.07.2020_02.53.17.684.Captain_Milka";

                using (var fs = new FileStream(thread_marker + @"\GA_Parameters.txt", FileMode.Open, FileAccess.Read))
                {
                    Genetic_Algorithm_Parameters ga_parameters = fs.DeSerialize<Genetic_Algorithm_Parameters>();

                    DateTime start_process_dt_process = DateTime.Now;

                    Run_Results result = new Run_Results();

                    DateTime start_process_dt_ga = DateTime.Now;

                    if (ga_parameters.Quick_Run == false)
                    {
                        while (true)
                        {
                            if (DateTime.Now.Subtract(start_process_dt_ga).TotalMilliseconds <= ga_parameters.Max_Runtime)
                            {
                                ga_parameters.Transactions_Dictionary.Fitness_Evaluation();

                                Run_Results temp_result = new Run_Results().Run_Genetic_ALgorithm(ga_parameters.Transactions_Dictionary, ga_parameters.Player, ga_parameters.Starting_Post, ga_parameters.Nb_Stops, ga_parameters.Salesman);

                                if (temp_result.Max_Profit > result.Max_Profit)
                                {
                                    result = temp_result;
                                }

                                GC.Collect();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        result = new Run_Results().Run_Genetic_ALgorithm(ga_parameters.Transactions_Dictionary, ga_parameters.Player, ga_parameters.Starting_Post, ga_parameters.Nb_Stops, ga_parameters.Salesman);
                    }

                    double Total_Runtime_GA = DateTime.Now.Subtract(start_process_dt_ga).TotalMilliseconds;

                    double Total_Runtime_Process = DateTime.Now.Subtract(start_process_dt_process).TotalMilliseconds;

                    result.Runtime = Total_Runtime_Process.ToString();

                    using (var fs2 = new FileStream(thread_marker + @"\GA_Results.txt", FileMode.Create, FileAccess.Write))
                    {
                        result.Serialize(fs2);
                    }

                    Console.WriteLine(result.Max_Profit.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public static class Serializer
    {
        public static byte[] ToByteArray<T>(this T graph)
        {
            using (var ms = new MemoryStream())
            {
                graph.Serialize(ms);

                return ms.ToArray();
            }
        }

        public static T FromByteArray<T>(this byte[] serialized)
        {
            using (var ms = new MemoryStream(serialized))
            {
                return ms.DeSerialize<T>();
            }
        }

        public static void Serialize<T>(this T graph, Stream target)
        {
            // create the formatter:
            IFormatter formatter = new BinaryFormatter
            {
                // set the binder to the custom binder:
                Binder = TypeOnlyBinder.Default
            };

            // serialize the object into the stream:
            formatter.Serialize(target, graph);

        }

        public static T DeSerialize<T>(this Stream source)
        {
            // create the formatter:
            IFormatter formatter = new BinaryFormatter
            {
                // set the binder to the custom binder:
                Binder = TypeOnlyBinder.Default
            };

            // serialize the object into the stream:
            return (T)formatter.Deserialize(source);
        }

        /// <summary>
        /// removes assembly name from type resolution
        /// </summary>
        public class TypeOnlyBinder : SerializationBinder
        {
            /// <summary>
            /// look up the type locally if the assembly-name is "NA"
            /// </summary>
            /// <param name="assemblyName"></param>
            /// <param name="typeName"></param>
            /// <returns></returns>
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (assemblyName.Equals("i-Fly_GA"))
                {
                    return Type.GetType(typeName);
                }
                else
                {
                    return defaultBinder.BindToType(assemblyName, typeName);
                }
            }

            /// <summary>
            /// override BindToName in order to strip the assembly name. Setting assembly name to null does nothing.
            /// </summary>
            /// <param name="serializedType"></param>
            /// <param name="assemblyName"></param>
            /// <param name="typeName"></param>
            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                // but null out the assembly name
                assemblyName = "i-Fly";

                if (serializedType.FullName == "System.Collections.Generic.List`1[[I_Fly.Models.Transaction, i-Fly_GA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]")
                {
                    typeName = "System.Collections.Generic.List`1[[I_Fly.Models.Transaction, i-Fly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]";
                }
                else
                {
                    typeName = serializedType.FullName;
                }
            }

            private static SerializationBinder defaultBinder = new BinaryFormatter().Binder;

            private static readonly object locker = new object();
            private static TypeOnlyBinder _default = null;

            public static TypeOnlyBinder Default
            {
                get
                {
                    lock (locker)
                    {
                        if (_default == null)
                            _default = new TypeOnlyBinder();
                    }
                    return _default;
                }
            }
        }


    }
}
