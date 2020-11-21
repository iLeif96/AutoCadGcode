using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{

    public class ValidProperties
    {
        public bool Pumping { get; set; } = false;
        public bool Command { get; set; } = false;
        public bool DisablePumping { get; set; } = false;
        public int Order { get; set; } = 0;
        public int StopAndPump { get; set; } = 0;
        public CommandType CommandType { get; set; } = CommandType.None;

        public ValidProperties(CommandType cType)
        {
            CommandType = cType;
        }

        public ValidProperties(Properties props)
        {
            Pumping = props.Pumping;
            Order = props.Order;
            Command = props.Command;
            StopAndPump = props.StopAndPump;
            DisablePumping = props.DisablePumping;
            CommandType = props.CommandType;
        }
    }
}
