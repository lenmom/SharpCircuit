using System;

namespace SharpCircuit
{
    /// <summary>
    /// 三极管,实现电流放大.其作用是把微弱信号放大成幅度值较大的电信号，也用作无触点开关。
    /// 三极管是在一块半导体基片上制作两个相距很近的PN结，
    /// 两个PN结把整块半导体分成三部分，中间部分是基区，两侧部分是发射区和集电区，排列方式有PNP和NPN两种。
    /// </summary>
    public class Triode : CircuitElement
    {

        public Circuit.Lead leadPlate { get { return lead0; } }
        public Circuit.Lead leadGrid { get { return lead1; } }
        public Circuit.Lead leadCath { get { return new Circuit.Lead(this, 2); } }

        public double currentp, currentg, currentc;

        private double mu, kg1;
        private double gridCurrentR = 6000;
        private double lastv0, lastv1, lastv2;

        public Triode() : base()
        {
            mu = 93;
            kg1 = 680;
        }

        public override bool nonLinear() { return true; }

        public override void reset()
        {
            lead_volt[0] = lead_volt[1] = lead_volt[2] = 0;
        }

        public override int getLeadCount()
        {
            return 3;
        }

        /*public override double getPower() {
			return (lead_volt[0] - lead_volt[2]) * current;
		}*/

        public override void step(Circuit sim)
        {
            double[] vs = new double[3];
            vs[0] = lead_volt[0];
            vs[1] = lead_volt[1];
            vs[2] = lead_volt[2];
            if (vs[1] > lastv1 + 0.5)
            {
                vs[1] = lastv1 + 0.5;
            }

            if (vs[1] < lastv1 - 0.5)
            {
                vs[1] = lastv1 - 0.5;
            }

            if (vs[2] > lastv2 + 0.5)
            {
                vs[2] = lastv2 + 0.5;
            }

            if (vs[2] < lastv2 - 0.5)
            {
                vs[2] = lastv2 - 0.5;
            }

            int grid = 1;
            int cath = 2;
            int plate = 0;
            double vgk = vs[grid] - vs[cath];
            double vpk = vs[plate] - vs[cath];
            if (Math.Abs(lastv0 - vs[0]) > 0.01 || Math.Abs(lastv1 - vs[1]) > 0.01 || Math.Abs(lastv2 - vs[2]) > 0.01)
            {
                sim.converged = false;
            }

            lastv0 = vs[0];
            lastv1 = vs[1];
            lastv2 = vs[2];
            double ids = 0;
            double gm = 0;
            double Gds = 0;
            double ival = vgk + vpk / mu;
            currentg = 0;
            if (vgk > .01)
            {
                sim.stampResistor(lead_node[grid], lead_node[cath], gridCurrentR);
                currentg = vgk / gridCurrentR;
            }
            if (ival < 0)
            {
                // should be all zero, but that causes a singular matrix,
                // so instead we treat it as a large resistor
                Gds = 1E-8;
                ids = vpk * Gds;
            }
            else
            {
                ids = Math.Pow(ival, 1.5) / kg1;
                double q = 1.5 * Math.Sqrt(ival) / kg1;
                // gm = dids/dgk;
                // Gds = dids/dpk;
                Gds = q;
                gm = q / mu;
            }
            currentp = ids;
            currentc = ids + currentg;
            double rs = -ids + Gds * vpk + gm * vgk;
            sim.stampMatrix(lead_node[plate], lead_node[plate], Gds);
            sim.stampMatrix(lead_node[plate], lead_node[cath], -Gds - gm);
            sim.stampMatrix(lead_node[plate], lead_node[grid], gm);

            sim.stampMatrix(lead_node[cath], lead_node[plate], -Gds);
            sim.stampMatrix(lead_node[cath], lead_node[cath], Gds + gm);
            sim.stampMatrix(lead_node[cath], lead_node[grid], -gm);

            sim.stampRightSide(lead_node[plate], rs);
            sim.stampRightSide(lead_node[cath], -rs);
        }

        public override void stamp(Circuit sim)
        {
            sim.stampNonLinear(lead_node[0]);
            sim.stampNonLinear(lead_node[1]);
            sim.stampNonLinear(lead_node[2]);
        }

        /*public override void getInfo(String[] arr) {
			arr[0] = "triode";
			double vbc = lead_volt[0] - lead_volt[1];
			double vbe = lead_volt[0] - lead_volt[2];
			double vce = lead_volt[1] - lead_volt[2];
			arr[1] = "Vbe = " + getVoltageText(vbe);
			arr[2] = "Vbc = " + getVoltageText(vbc);
			arr[3] = "Vce = " + getVoltageText(vce);
		}*/

        // grid not connected to other terminals
        public override bool leadsAreConnected(int n1, int n2)
        {
            return !(n1 == 1 || n2 == 1);
        }
    }
}