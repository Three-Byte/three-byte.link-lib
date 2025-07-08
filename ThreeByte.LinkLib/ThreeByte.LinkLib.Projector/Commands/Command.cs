namespace ThreeByte.LinkLib.ProjectorLink.Commands
{
    public abstract class Command
    {
        public delegate void CommandResultHandler(Command sender, CommandResponse response);

        protected CommandResponse _cmdResponse;

        internal virtual string GetCommandString()
        {
            //NOOP
            return "";
        }

        internal virtual bool ProcessAnswerString(string a)
        {

            if (a.IndexOf("=ERR1") >= 0)
            {
                _cmdResponse = CommandResponse.UNDEFINED_CMD;
            }
            else if (a.IndexOf("=ERR2") >= 0)
            {
                _cmdResponse = CommandResponse.UNDEFINED_CMD;
            }
            else if (a.IndexOf("=ERR3") >= 0)
            {
                _cmdResponse = CommandResponse.UNVAILABLE_TIME;
            }
            else if (a.IndexOf("=ERR4") >= 0)
            {
                _cmdResponse = CommandResponse.PROJECTOR_FAILURE;
            }
            else if (a.IndexOf(" ERRA") >= 0)
            {
                _cmdResponse = CommandResponse.AUTH_FAILURE;
            }
            else //OK or query answer...
            {
                _cmdResponse = CommandResponse.SUCCESS;
            }

            return _cmdResponse == CommandResponse.SUCCESS;
        }

        public CommandResponse CmdResponse
        {
            get { return _cmdResponse; }
        }

        public virtual string DumpToString()
        {
            return "";
        }
    }
}
