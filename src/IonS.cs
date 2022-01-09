using System;
using System.IO;

namespace IonS
{

    enum Action {
        Compile,
        Interpret,
        Simulate,
    }

    enum Assembler {
        NASM_Linux_x86_86,
        FASM_Linux_x86_86,
    }

    class IonS
    {
        static void Main(string[] args) {
            // parameters
            Action action = Action.Compile;
            Assembler assembler = Assembler.NASM_Linux_x86_86;
            string filename = null;
            // parsing the parameters
            int i = 0;
            while(i < args.Length) {
                if(i == 0) {
                    if(args[i] == "c" || args[i] == "compile") action = Action.Compile;
                    else if(args[i] == "s" || args[i] == "simulate") action = Action.Simulate;
                    else if(args[i] == "i" || args[i] == "interpret") action = Action.Interpret;
                    else {
                        Console.WriteLine("Invalid action: '" + args[i] + "'");
                        Environment.ExitCode = 2;
                        return;
                    }
                    i++;
                    continue;
                }
                if(action == Action.Compile || action == Action.Simulate) {
                    if(args[i] == "--file") {
                        i++;
                        if(i >= args.Length) {
                            Console.WriteLine("Missing argument for '--file'");
                            Environment.ExitCode = 2;
                            return;
                        }
                        filename = args[i++];
                        continue;
                    }
                }
                if(action == Action.Compile) {
                    if(args[i] == "-a" || args[i] == "--assembler") {
                        i++;
                        if(i >= args.Length) {
                            Console.WriteLine("Missing argument for '--assembler'");
                            Environment.ExitCode = 2;
                            return;
                        }
                        if(args[i] == "nasm-linux-x86_64" || args[i] == "nasm") assembler = Assembler.NASM_Linux_x86_86;
                        else if(args[i] == "fasm-linux-x86_64" || args[i] == "fasm") assembler = Assembler.FASM_Linux_x86_86;
                        else {
                            Console.WriteLine("Invalid assembler: '" + args[i] + "'");
                            Environment.ExitCode = 2;
                            return;
                        }
                        i++;
                        continue;
                    }
                }
                Console.WriteLine("Invalid argument: '" + args[i] + "'");
                Environment.ExitCode = 2;
                return;
            }
            if(action == Action.Compile) {
                if(filename == null) filename = "res/test.ions";
                Compile(filename, assembler);
            } else if(action == Action.Simulate) {
                Console.WriteLine("Feature is currently disabled.");
                Environment.ExitCode = 2;
                return;
                //if(filename != null) filename = "res/test.ions";
                //Simulate(filename);
            } else if(action == Action.Interpret) {
                Console.WriteLine("Feature is currently disabled.");
                Environment.ExitCode = 2;
                return;
                //Interpret();
            }
        }

        private static int CountLines(string text) {
            int count = 0;
            for(int i = 0; i < text.Length; i++) if(text[i] == '\n') count++;
            return count;
        }
        private static void Compile(string filename, Assembler assembler) {
            if(!File.Exists(filename)) {
                Console.WriteLine("Error: File not found: '" + filename + "'");
                Environment.ExitCode = 1;
                return;
            }
            string path = Path.GetFullPath(filename);
            var asmTscr = new AssemblyTranscriber(File.ReadAllText(path).Replace("\r\n", "\n"), path);
            AssemblyTranscriptionResult result = null;
            if(assembler == Assembler.NASM_Linux_x86_86) result = asmTscr.nasm_linux_x86_64();
            else if(assembler == Assembler.FASM_Linux_x86_86) result = asmTscr.fasm_linux_x86_64();
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
            File.WriteAllText("\\\\wsl$\\Ubuntu-20.04\\shared\\testIons.asm", result.Asm);
            Console.WriteLine("Generated assembly with " + CountLines(result.Asm) + " lines.");
        }

        /*
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
        */
    }

}
