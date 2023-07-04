namespace SharpCircuit
{

    public class AndGate : LogicGate
    {

        public override bool calcFunction()
        {
            bool f = true;
            for (int i = 0; i != inputCount; i++)
            {
                f &= getInput(i);
            }

            return f;
        }

    }
}