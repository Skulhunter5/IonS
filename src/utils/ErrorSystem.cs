using System;
using System.Collections.Generic;

namespace IonS {
    
    class ErrorSystem {

        private static bool terminateAfterStep = false;

        private static List<Error> errors = new List<Error>();
        private static List<Warning> warnings = new List<Warning>();

        // Add error and exit right away
        public static void AddError_i(Error error) {
            errors.Add(error);
            WriteAndExit();
        }

        // Add error and set the terminateAfterStep flag
        public static void AddError_s(Error error) {
            errors.Add(error);
            terminateAfterStep = true;
        }

        // Returns whether the terminateAfterStep flag has been set
        public static bool ShouldTerminateAfterStep() {
            return terminateAfterStep;
        }

        // Writes all errors and warnings to the Console and exits with exitcode 1 afterwards
        public static void WriteAndExit() {
            if(errors.Count > 0) for(int i = 0; i < errors.Count; i++) Console.Error.WriteLine(errors[i]);
            else for(int i = 0; i < warnings.Count; i++) Console.WriteLine(warnings[i]);

            Environment.Exit(1);
        }

        // Writes the given message to the Console and exits
        public static void InternalError(string msg) {
            Console.Error.WriteLine("INTERNAL ERROR: " + msg);
        }

    }

}