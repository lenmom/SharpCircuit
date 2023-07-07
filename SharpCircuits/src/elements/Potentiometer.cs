namespace SharpCircuit
{
    /// <summary>
    /// ��λ��/��ѹ��,��λ���Ǿ������������ˡ���ֵ�ɰ�ĳ�ֱ仯���ɵ��ڵĵ���Ԫ����
    /// ��λ��ͨ���ɵ�����Ϳ��ƶ��ĵ�ˢ��ɡ�����ˢ�ص������ƶ�ʱ��������˼������λ������һ����ϵ�ĵ���ֵ���ѹ��
    /// </summary>
    public class Potentiometer : CircuitElement
    {
        #region Field

        private const double DefaultMaxResistance = 1000;
        private const double DefaultPosition = 0.5;

        /// <summary>
        /// maxResistance * position
        /// </summary>
        private double resistance1;

        /// <summary>
        /// maxResistance * (1 - position)
        /// </summary>
        private double resistance2;

        #endregion

        #region Property

        public Circuit.Lead leadOut { get { return lead0; } }

        public Circuit.Lead leadIn { get { return lead1; } }

        public Circuit.Lead leadVoltage { get { return new Circuit.Lead(this, 2); } }

        public double position { get; set; }

        /// <summary>
        /// Resistance (ohms)
        /// </summary>
        public double maxResistance { get; set; }

        /// <summary>
        /// (lead_volt[0] - lead_volt[2]) / resistance1
        /// </summary>
        public double current1 { get; private set; }

        /// <summary>
        /// (lead_volt[1] - lead_volt[2]) / resistance2
        /// </summary>
        public double current2 { get; private set; }

        /// <summary>
        ///  -current1 - current2
        /// </summary>
        public double current3 { get; private set; }

        #endregion

        #region Constructor

        public Potentiometer() : base()
        {
            maxResistance = DefaultMaxResistance;
            position = DefaultPosition;
        }

        #endregion

        #region Public Method

        public override int getLeadCount()
        {
            return 3;
        }

        public override void calculateCurrent()
        {
            current1 = (lead_volt[0] - lead_volt[2]) / resistance1;
            current2 = (lead_volt[1] - lead_volt[2]) / resistance2;
            current3 = -current1 - current2;
        }

        public override void stamp(Circuit sim)
        {
            resistance1 = maxResistance * position;
            resistance2 = maxResistance * (1 - position);
            sim.stampResistor(lead_node[0], lead_node[2], resistance1);
            sim.stampResistor(lead_node[2], lead_node[1], resistance2);
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "potentiometer";
            //arr[1] = "Vd = " + getVoltageDText(getVoltageDiff());
            arr[2] = "R1 = " + getUnitText(resistance1, Circuit.ohmString);
            arr[3] = "R2 = " + getUnitText(resistance2, Circuit.ohmString);
            arr[4] = "I1 = " + getCurrentDText(current1);
            arr[5] = "I2 = " + getCurrentDText(current2);
        }*/

        #endregion
    }
}