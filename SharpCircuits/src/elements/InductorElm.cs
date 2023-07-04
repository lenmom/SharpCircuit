namespace SharpCircuit
{
    /// <summary>
    /// 电感器,在电路中因为存在电磁感应的效果，所以存在一定的电感性，能够起到阻止电流变化的作用.
    /// </summary>
    public class InductorElm : CircuitElement
    {

        public Circuit.Lead leadIn { get { return lead0; } }
        public Circuit.Lead leadOut { get { return lead1; } }

        /// <summary>
        /// Inductance (H)
        /// </summary>
        public double inductance { get; set; }
        public bool isTrapezoidal { get; set; }

        private int[] nodes;
        private double compResistance;
        private double curSourceValue;

        public InductorElm() : base()
        {
            nodes = new int[2];
            inductance = 1;
        }

        public InductorElm(double induc) : base()
        {
            nodes = new int[2];
            inductance = induc;
        }

        public override void reset()
        {
            current = lead_volt[0] = lead_volt[1] = 0;
        }

        public override void stamp(Circuit sim)
        {
            nodes[0] = lead_node[0];
            nodes[1] = lead_node[1];
            if (isTrapezoidal)
            {
                compResistance = 2 * inductance / sim.timeStep;
            }
            else
            {
                compResistance = inductance / sim.timeStep; // backward euler
            }
            sim.stampResistor(nodes[0], nodes[1], compResistance);
            sim.stampRightSide(nodes[0]);
            sim.stampRightSide(nodes[1]);
        }

        public override void beginStep(Circuit sim)
        {
            double voltdiff = lead_volt[0] - lead_volt[1];
            if (isTrapezoidal)
            {
                curSourceValue = voltdiff / compResistance + current;
            }
            else
            {
                curSourceValue = current; // backward euler
            }
        }

        public override bool nonLinear() { return true; }

        public override void calculateCurrent()
        {
            double voltdiff = lead_volt[0] - lead_volt[1];
            if (compResistance > 0)
            {
                current = voltdiff / compResistance + curSourceValue;
            }
        }

        public override void step(Circuit sim)
        {
            double voltdiff = lead_volt[0] - lead_volt[1];
            sim.stampCurrentSource(nodes[0], nodes[1], curSourceValue);
        }

        /*public override void getInfo(String[] arr) {
			arr[0] = "inductor";
			getBasicInfo(arr);
			arr[3] = "L = " + getUnitText(inductance, "H");
			arr[4] = "P = " + getUnitText(getPower(), "W");
		}*/

    }
}