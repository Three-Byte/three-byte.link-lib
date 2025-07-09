using System.Text;

namespace ThreeByte.LinkLib.ProjectorLink.Commands
{
    public class ProjectorNameCommand : Command
    {
        private string _name = "";

        public ProjectorNameCommand()
        {
        }

        internal override string GetCommandString()
        {
            return "%1NAME ?";
        }

        internal override bool ProcessAnswerString(string a)
        {
            if (!base.ProcessAnswerString(a))
            {
                return false;
            }

            a = a.Replace("%1NAME=", "");

            byte[] dec = Encoding.ASCII.GetBytes(a);
            _name = Encoding.UTF8.GetString(dec);

            return true;
        }


        public override string DumpToString()
        {
            return "Name: " + _name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
