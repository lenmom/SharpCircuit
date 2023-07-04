namespace SharpCircuit
{
    /// <summary>
    /// Ì½ÕëÆ÷¼þ
    /// </summary>
    public class Probe : CircuitElement
    {
        public Circuit.Lead leadIn { get { return lead0; } }
        public Circuit.Lead leadOut { get { return lead1; } }

        /*public override void getInfo(String[] arr) {
			arr[0] = "scope probe";
			arr[1] = "Vd = " + getVoltageText(getVoltageDiff());
		}*/

        public override bool leadsAreConnected(int n1, int n2)
        {
            return false;
        }

    }
}