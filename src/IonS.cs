using System;
using System.IO;

namespace IonS {

    enum Action {
        Compile,
    }

    class IonS {

        static void Main(string[] args) {
            // parameters
            Action action = Action.Compile;
            Assembler assembler = Assembler.nasm_linux_x86_64;
            string filename = null;
            // parsing the parameters
            int i = 0;
            while(i < args.Length) {
                if(i == 0) {
                    if(args[i] == "c" || args[i] == "compile") action = Action.Compile;
                    else {
                        Console.WriteLine("Invalid action: '" + args[i] + "'");
                        Environment.ExitCode = 2;
                        return;
                    }
                    i++;
                    continue;
                }
                if(action == Action.Compile) {
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
                        if(args[i] == "nasm-linux-x86_64" || args[i] == "nasm") assembler = Assembler.nasm_linux_x86_64;
                        else if(args[i] == "fasm-linux-x86_64" || args[i] == "fasm") assembler = Assembler.fasm_linux_x86_64;
                        else if(args[i] == "iasm-linux-x86_64" || args[i] == "iasm") assembler = Assembler.iasm_linux_x86_64;
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
            AssemblyTranscriptionResult result = asmTscr.run(assembler);
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

    }

}
