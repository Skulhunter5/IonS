using System;
using System.IO;

namespace IonS
{

    class IonS
    {
        static void Main(string[] args) {
            if(args.Length == 0 || args[0] == "-i" || args[0] == "--interpret") Interpret();
            else if(args[0] == "--compile") {
                if(args.Length == 1) Compile("res/test.ions");
                else if(args.Length >= 2) {
                    if(args[1].StartsWith("\"")) {
                        Console.WriteLine("Unimplemented feature");
                        return;
                    }
                    Compile(args[1]);
                }
            } else if(args[0] == "-s" || args[0] == "--simulate") Simulate();
            else Console.WriteLine("Invalid argument: '" + args[0] + "'");
        }

        private static int CountLines(string text) {
            int count = 0;
            for(int i = 0; i < text.Length; i++) if(text[i] == '\n') count++;
            return count;
        }
        private static void Compile(string file) {
            var asmTscr = new AssemblyTranscriber(File.ReadAllText(file).Replace("\r\n", "\n"), file);
            AssemblyTranscriptionResult result = asmTscr.run();
            if(result.Error != null) {
                Console.WriteLine(result.Error);
                Environment.ExitCode = 1;
                return;
            }
            if(result.Asm == null) {
                Console.WriteLine("No assembly has been generated.");
                Environment.ExitCode = 1;
                return;
            }
            File.WriteAllText("\\\\wsl$\\Ubuntu-20.04\\shared\\test.ions.asm", result.Asm);
            Console.WriteLine("Generated assembly with " + CountLines(result.Asm) + " lines.");
        }

        private static void Interpret() {
            var simulator = new Simulator(100000);
            while(true) {
                Console.Write("> ");
                var line = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(line)) {
                    string msg = "[";
                    foreach(int i in simulator.stack) {
                        msg += i + ", ";
                    }
                    if(simulator.stack.Count > 0) msg = msg.Substring(0, msg.Length-2);
                    msg += "]";
                    Console.WriteLine(msg);
                }
                SimulationResult result = simulator.run(line, "<stdin>");
                if(result.Error != null) Console.WriteLine(result.Error);
                if(result.Exitcode != 0) Console.WriteLine("Exited with code " + result.Exitcode + ".");
            }
        }

        private static void Simulate() {
            SimulationResult result = new Simulator(100000).run(File.ReadAllText("res/test.ions"), "res/test.ions");
            if(result.Error != null) {
                Console.WriteLine(result.Error);
                return;
            }
            Console.WriteLine("Exited with code " + result.Exitcode + ".");
        }
    }

}
