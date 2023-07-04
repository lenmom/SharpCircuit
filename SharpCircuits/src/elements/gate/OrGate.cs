namespace SharpCircuit
{

    public class OrGate : LogicGate
    {

        public override bool calcFunction()
        {
            bool f = false;
            for (int i = 0; i != inputCount; i++)
            {
                f |= getInput(i);
            }

            return f;
        }

    }
}