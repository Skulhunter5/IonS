namespace IonS {

    abstract class Warning {}

    abstract class TypeCheckerWarning : Warning {
        public override string ToString() {
            return "[TypeChecker] WARNING: ";
        }
    }

    // Implicit cast warnings

    sealed class ImplicitCastWarning : TypeCheckerWarning {
        public ImplicitCastWarning(DataType from, DataType to, Operation operation) {
            From = from;
            To = to;
            Operation = operation;
        }

        public DataType From { get; }
        public DataType To { get; }
        public Operation Operation { get; }

        public override string ToString() {
            return base.ToString() + "Implicit cast from " + From + " to " + To + " during " + Operation;
        }
    }

    sealed class ImplicitCastWhenReturningWarning : TypeCheckerWarning {
        public ImplicitCastWhenReturningWarning(DataType from, DataType to, Function function) {
            From = from;
            To = to;
            Function = function;
        }

        public DataType From { get; }
        public DataType To { get; }
        public Function Function { get; }

        public override string ToString() {
            return base.ToString() + "Implicit cast from " + From + " to " + To + " when returning from " + Function;
        }
    }

}