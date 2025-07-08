namespace ThreeByte.LinkLib.ProjectorLink.Commands
{
    public class ProductNameCommand : Command
    {
        private string _name = "";

        public ProductNameCommand()
        {
        }

        internal override string GetCommandString()
        {
            return "%1INF2 ?";
        }

        internal override bool ProcessAnswerString(string a)
        {
            if (!base.ProcessAnswerString(a))
            {
                return false;
            }

            _name = a.Replace("%1INF2=", "");

            return true;
        }

        public override string DumpToString()
        {
            string toRet = "ProductName: " + _name;
            return toRet;
        }

        public string ProductName
        {
            get { return _name; }
        }
    }
}
