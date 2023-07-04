namespace SharpCircuit
{

    public class NandGate : AndGate
    {

        public override bool isInverting() { return true; }

    }
}