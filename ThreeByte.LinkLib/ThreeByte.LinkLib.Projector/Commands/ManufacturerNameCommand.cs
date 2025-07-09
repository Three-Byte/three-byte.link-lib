namespace ThreeByte.LinkLib.ProjectorLink.Commands
{
    public class ManufacturerNameCommand : Command
    {
        private string _name = "";

        public ManufacturerNameCommand()
        {
        }

        internal override string GetCommandString()
        {
            return "%1INF1 ?";
        }

        internal override bool ProcessAnswerString(string a)
        {
            if (!base.ProcessAnswerString(a))
            {
                return false;
            }

            _name = a.Replace("%1INF1=", "");

            return true;
        }


        public override string DumpToString()
        {
            string toRet = "Manufacturer: " + _name;
            return toRet;
        }

        public string Manufacturer
        {
            get { return _name; }
        }
    }
}
